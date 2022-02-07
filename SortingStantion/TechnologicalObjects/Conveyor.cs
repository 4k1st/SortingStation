using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Класс, реализующий модель
    /// конвейера
    /// </summary>
    public class Conveyor : INotifyPropertyChanged
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Команда ПУСК-ОСТАНОВКА в нормальном режиме
        /// </summary>
        S7_Boolean Run
        {
            get;
            set;
        }

        /// <summary>
        /// Команда ПУСК-ОСТАНОВКА в принудительним режиме
        /// </summary>
        S7_Boolean RunForce
        {
            get;
            set;
        }

        /// <summary>
        /// Тэг, полученный от сканера,
        /// что линия остановлена
        /// </summary>
        public S7_Boolean IsStopFromTimerTag
        {
            get;
            set;
        }

        /// <summary>
        /// Флаг, указывающий, что
        /// линия запущена в нормальном режиме (для View Model)
        /// </summary>
        public bool LineIsRun
        {
            get 
            {
                return Run.Value;
            }
        }

        /// <summary>
        /// Флаг, указывающий, что 
        /// линия запущена в принудительном режиме
        /// </summary>
        public bool LineIsForceRun
        {
            get
            {
                return RunForce.Value;
            }
        }

        /// <summary>
        /// Событие, генерируемое при 
        /// изменении состояния работы конвейера
        /// в нормальном режиме
        /// </summary>
        public event Action ChangeOfStateInNormalMode;

        /// <summary>
        /// Событие, генерируемое при 
        /// изменении состояния работы конвейера
        /// в принудительном режиме
        /// </summary>
        public event Action ChangeOfStateInForceMode;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Conveyor()
        {
            //Тэг для запуска - останова линии в нормальном режиме
            Run = new S7_Boolean("", "DB1.DBX98.6", group);

            //Тэг для запуска - останова линии в принудительном режиме
            RunForce = new S7_Boolean("", "DB1.DBX98.4", group);

            //Тэг, указывающий, что линия установлена с учетом задержки
            //на останов
            IsStopFromTimerTag = new S7_Boolean("", "DB1.DBX86.1", group);

            //Подпись на событие по изминеию статуса работы
            //линии
            Run.ChangeValue += Run_ChangeValue;

            //Вывод сообщения в окно информации (начальное)
            UserMessage msg = new UserMessage("Линия остановлена", MSGTYPE.WARNING);
            DataBridge.MSGBOX.Add(msg);

            /*
                 Процедура, вызываемая при смене экрана
            */
            DataBridge.ScreenEngine.ChangeScreenNotification += (screen) =>
            {
                //Если линия была запущена в принудительном режиме и
                //был сменен экран с НАСТРОЕК, выключаем принудительное
                //включение линии
                var lineisforcerun = RunForce.Value;
                if (lineisforcerun == true)
                {
                    RunForce.Write(false);
                    ChangeOfStateInForceMode?.Invoke();
                }
            };

            //Извещение подписчиков об изменении
            //состояния работы конвейера
            RunForce.ChangeValue += (ov, nv) =>
            {
                ChangeOfStateInForceMode?.Invoke();
            };

            /*
                 Процедура, вызываемая при закрытии приложения
            */
            DataBridge.Shutdown += () =>
            {
                //Выключение конвейера и комплекса
                Run.Write(false);
                RunForce.Write(false);
            };
        }

        /// <summary>
        /// Метод для запуска конвейера
        /// </summary>
        public void Start()
        {
            //Если задание принято, то запускаем линию
            //иначе уведомляем оператора сообщением
            if (DataBridge.WorkAssignmentEngine.InWork == false)
            {
                customMessageBox mb = new customMessageBox("Ошибка", "Запуск невозможен, задание не принято в работу");
                mb.Owner = DataBridge.MainScreen;
                mb.ShowDialog();

                return;
            }

            //Запись статуса в ПЛК
            Run.Write(true);

            //Внесение в базу данных сообщения о запуске комплекса
            DataBridge.AlarmLogging.AddMessage("Нажата кнопка СТАРТ. Линия запущена", Models.MessageType.Event);

            return;
        }

        /// <summary>
        /// Команда для запуска
        /// линии
        /// </summary>
        public ICommand StartLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Start();
                },
                (obj) => (true));
            }
        }



        /// <summary>
        /// Метод для остановки конвейера
        /// </summary>
        public void Stop()
        {
            //Запись статуса в ПЛК
            Run.Write(false);

            return;
        }

        /// <summary>
        /// Команда для остановки
        /// линии
        /// </summary>
        public ICommand StopLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Внесение в базу данных сообщения об остановке комплекса
                    DataBridge.AlarmLogging.AddMessage("Нажата кнопка СТОП. Линия остановлена", Models.MessageType.Event);

                    Stop();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для принудительного запуска
        /// конвейера
        /// </summary>
        public ICommand forceStartLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    var inswork = DataBridge.WorkAssignmentEngine.InWork;

                    //Если задание в работе
                    //запускаем линию
                    if (inswork == true)
                    {
                        this.Start();
                        return;
                    }

                    //Если задание не в работе -
                    //запускаем только конвейер
                    RunForce.Write(true);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для принулительного останова
        /// конвейера
        /// </summary>
        public ICommand forceStopLineCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    var inswork = DataBridge.WorkAssignmentEngine.InWork;

                    //Останавливаем всю линию
                    RunForce.Write(false);
                    Run.Write(false);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод, приводящий тип object к 
        /// bool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool ToBool(object value)
        {
            if (value is bool? == false)
            {
                return false;
            }

            return (bool)value;
        }

        /// <summary>
        /// Предыдущее состояние работы линии
        /// в нормальном режиме
        /// </summary>
        bool IsRunStateOldValue = false;

        /// <summary>
        /// Текущее состояние работы линии
        /// в нормальном режиме
        /// </summary>
        bool IsRunStateCureentValue = false;

        /// <summary>
        /// Метод, вызываемый при изминении
        /// статуса работы линии
        /// </summary>
        /// <param name="obj"></param>
        private void Run_ChangeValue(object oldvalue, object newvalue)
        {
            //Получение текущего состояния работы линии
            //в нормальном режиме
            IsRunStateCureentValue = ToBool(Run.Value);

            //Если состояние работы линии в нормальном режиме
            //не изменилось, покидаем функцию
            if (IsRunStateCureentValue == IsRunStateOldValue)
            {
                return;
            }

            //Запоминием состояние работы линии в 
            //нормальном режиме
            IsRunStateOldValue = IsRunStateCureentValue;


            //Если конвейер запущен
            if (IsRunStateCureentValue == true)
            {
                Action action = () =>
                {

                    //Вывод сообщения в окно информации
                    UserMessage msg = new UserMessage("Линия запущена", MSGTYPE.SUCCES);
                    DataBridge.MSGBOX.Add(msg);

                    //Уведомление подписчиков об изменении
                    //состояния линии
                    ChangeOfStateInNormalMode?.Invoke();

                };
                DataBridge.UIDispatcher.Invoke(action);

                
            }

            //Если конвейер остановлен
            if (IsRunStateCureentValue == false)
            {
                Action action = () =>
                {
                    //Вывод сообщения в окно информации
                    UserMessage msg = new UserMessage("Линия остановлена", MSGTYPE.WARNING);
                    DataBridge.MSGBOX.Add(msg);

                    //Уведомление подписчиков об изменении
                    //состояния линии
                    ChangeOfStateInNormalMode?.Invoke();
                };
                DataBridge.UIDispatcher.Invoke(action);
            }

            //уведомление модели представления
            OnPropertyChanged("LineIsRun");
            OnPropertyChanged("LineIsStop");

            //Уведомление UI
            OnPropertyChanged("btnForceLineRunEnable");
            OnPropertyChanged("btnForceLineStopEnable");
        }

        #region Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion

    }
}
