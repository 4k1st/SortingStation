using Newtonsoft.Json;
using SortingStantion.Controls;
using SortingStantion.ToolsWindows.windowClearCollectionRequest;
using SortingStantion.Utilites;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace SortingStantion.Models
{
    /// <summary>
    /// Объект, осуществляющий управлением
    /// заданиями
    /// </summary>
    public class WorkAssignmentEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        public string ID
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return string.Empty;
                }

                return SelectedWorkAssignment.ID;
            }
        }

        /// <summary>
        /// Номер GTIN. (14 символов)
        /// </summary>
        public string GTIN
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return string.Empty;
                }

                return SelectedWorkAssignment.gtin;
            }
        }

        /// <summary>
        /// Наименование продукта (UTF-8)
        /// </summary>
        public string Product_Name
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return string.Empty;
                }

                return SelectedWorkAssignment.productName;
            }
        }

        /// <summary>
        /// Номер производственной серии (до 20 символов) 
        /// </summary>
        public string Lot_No
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return string.Empty;
                }

                return SelectedWorkAssignment.lotNo;
            }
        }

        /// <summary>
        /// Количество продуктов в коробе
        /// </summary>
        public int numPacksInBox
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return 0;
                }

                return SelectedWorkAssignment.numPacksInBox;
            }
        }

        /// <summary>
        /// Количество продуктов в серии
        /// </summary>
        public int numPacksInSeries
        {
            get
            {
                if (SelectedWorkAssignment == null)
                {
                    return 0;
                }

                return SelectedWorkAssignment.numPacksInSeries;
            }
        }

        /// <summary>
        /// Коллекция рабочих заданий
        /// </summary>
        public ObservableCollection<WorkAssignment> WorkAssignments
        {
            get;
            set;
        }

        /// <summary>
        /// Выбранное задание
        /// </summary>
        public WorkAssignment SelectedWorkAssignment
        {
            get
            {
                return selectedWorkAssignment;
            }
            set
            {
                selectedWorkAssignment = value;

                //уведомление подписчиков
                this.WorkOrderAcceptanceNotification?.Invoke(selectedWorkAssignment);

                //Уведомление UI
                OnPropertyChanged("ID");
                OnPropertyChanged("GTIN");
                OnPropertyChanged("Lot_No");
                OnPropertyChanged("Product_Name");                
            }
        }
        WorkAssignment selectedWorkAssignment;

        /// <summary>
        /// Флаг возвращает 
        /// принято ли задание
        /// </summary>
        public bool InWork
        {
            get
            {
                //Возвращение значения тэга
                return SelectedWorkAssignment != null;
            }
        }

        /// <summary>
        /// Событие, генерируемое при принятии
        /// нового задания
        /// </summary>
        public event Action<WorkAssignment> WorkOrderAcceptanceNotification;

        /// <summary>
        /// Событие, генерируемое при завершении
        /// рабочего задания
        /// </summary>
        public event Action<WorkAssignment> WorkOrderCompletionNotification;

        /// <summary>
        /// Событие, генерируемое при получении нового
        /// рабочего задания от L3
        /// </summary>
        public event Action<WorkAssignment> NewWorkOrderHasArrivedNotification;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WorkAssignmentEngine()
        {
            //Инициализация коллекции с рабочими заданиями
            WorkAssignments = new ObservableCollection<WorkAssignment>();

            //Загрузка задания из файла
            try
            {
                LoadFromFile();
            }
            catch (System.Net.HttpListenerException ex)
            {
                //Запись в лог
                Logger.AddExeption("WorkAssignmentEngine.cs", ex);
            }

            
            //Запуск задачи по прослушиванию
            //http
            Task.Factory.StartNew(() =>
            {
                try
                {
                    StartListener();
                }
                catch (System.Net.HttpListenerException ex)
                {
                    UserMessage messageItem = new Controls.UserMessage(ex.Message, DataBridge.myRed);
                    DataBridge.MSGBOX.Add(messageItem);
                }
               
            });
        }

        /// <summary>
        /// Метод для загрузки задания из файла
        /// в папке Orders
        /// </summary>
        void LoadFromFile()
        {
            var files = Directory.GetFiles("Orders");

            //Если файлов больше 1 выводим ошибку
            if (files.Length > 1)
            {
                string message = "Ошибка загрузки незавершенного задания. Обратитесь к наладчику";
                ShowMessage(message, DataBridge.myRed);
                return;
            }

            //Если файлы отсутсвуют, пропускаем загрузку файла
            if (files.Length == 0)
            {
                return;
            }

            //Получение файла незавершенного задания
            var file = $@"{files[0]}";

            //Десериализация задачи из памяти программы
            using (StreamReader fs = new StreamReader(file))
            {
                var text = fs.ReadToEnd();
                fs.Close();

                //Десериализация
                var deserializeWorkAssignment = JsonConvert.DeserializeObject<WorkAssignment>(text);

                //Передача десериализованного объекта в текущее задание
                WorkAssignments.Add(deserializeWorkAssignment);

                //Передача десериализованного объекта в текущее задание
                SelectedWorkAssignment = deserializeWorkAssignment; 
            }
        }

        /// <summary>
        /// Метод для сброса полей
        /// </summary>
        void Clear()
        {
            SelectedWorkAssignment = null;
        }

       /// <summary>
       /// Метод для проверки задания на наличае
       /// всех полей
       /// </summary>
       /// <param name="workAssignment"></param>
       /// <returns></returns>
        bool CheckTask(WorkAssignment workAssignment)
        {
            //Результат проверки
            bool result = true;

            Action action = null;
            string messege = string.Empty;


            if (this.InWork == false)
            {
                //Проверка ID
                if (string.IsNullOrEmpty(workAssignment.ID) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: ID", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка GTIN
                if (string.IsNullOrEmpty(workAssignment.gtin) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: GTIN", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                if (workAssignment.gtin.Length != 14)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Неверный GTIN", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка lotNo
                if (string.IsNullOrEmpty(workAssignment.lotNo) == true)
                {

                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: LotNo", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка productName
                if (string.IsNullOrEmpty(workAssignment.productName) == true)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: ProductName", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Проверка numPacksInSeries
                if (workAssignment.numPacksInSeries == 0)
                {
                    action = () =>
                    {
                        UserMessage messageItem = new Controls.UserMessage("Задание не может быть принято в работу. Не заполнено поле: numPacksInSeries", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                    };
                    DataBridge.UIDispatcher.Invoke(action);

                    return false;
                }

                //Ели проверка пройдена
                return true;
            }

            action = () =>
            {
                UserMessage messageItem = new Controls.UserMessage("Новое задание не может быть принято в работу поскольку текущее задание не завершено", DataBridge.myRed);
                DataBridge.MSGBOX.Add(messageItem);
            };
            DataBridge.UIDispatcher.Invoke(action);

            //Возврат отрицательного
            //результата
            return false;
        }

        /// <summary>
        /// Команда - принять задание
        /// </summary>
        public ICommand AcceptTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Если в коллекции заданий заданий нет -
                    //пишем ошибку - "Задание не может быть принято в работу"
                    if (WorkAssignments.Count == 0)
                    {
                        UserMessage messageItem = new Controls.UserMessage("В буфере нет заданий, которые могут быть приняты в работу", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);
                        return;
                    }

                    //Если задание не принято в работу
                    if (InWork == false)
                    {
                        //Вызываем окно авторизации
                        //Вызов окна для авторизации мастера
                        SortingStantion.ToolsWindows.Authorization.windowAuthorizationSuperUser windowAuthorizationSuperUser = new ToolsWindows.Authorization.windowAuthorizationSuperUser();
                        windowAuthorizationSuperUser.Owner = DataBridge.MainScreen;
                        windowAuthorizationSuperUser.ShowDialog();

                        //Если результат авторизации не удачный, выходим
                        if (windowAuthorizationSuperUser.AuthorizationResult == false)
                        {
                            return;
                        }

                        //Очищаем отчет от предыдущих операций
                        DataBridge.Report.ClearResult();

                        //Запись в отчет первого мастера, вошедшего в систему
                        var currentuser = DataBridge.MainAccesLevelModel.CurrentUser;
                        var historyitem = new UserAuthorizationHistotyItem(currentuser);
                        DataBridge.Report.AddAuthorizationUser(historyitem);

                        //Запись времени, когда принято задание
                        var timeStart = historyitem.startTime;
                        DataBridge.Report.startTime = timeStart;

                        //Переноcим задание в выбраное задание
                        SelectedWorkAssignment = WorkAssignments[0];

                        //Удаление принятого задания из списка заданий
                        WorkAssignments.Remove(SelectedWorkAssignment);

                        //Запись ID в отчет
                        DataBridge.Report.ID = SelectedWorkAssignment.ID;

                        //Формирование текста сообщения в зоне информации
                        string message = $"Задание {this.ID} принято в работу";

                        //Запись в базу данных о принятии задания
                        DataBridge.AlarmLogging.AddMessage(message, MessageType.TaskLogging);

                        //Вывод сообщения в зоне информации
                        var msg = new UserMessage(message, MSGTYPE.SUCCES);
                        DataBridge.MSGBOX.Add(msg);

                        //Сохранение принятого задания в файл
                        //резервной копии
                        CreateBackupFile();

                        //Выход из функции
                        return;
                    }
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для завершения задания
        /// </summary>
        public ICommand FinishTaskCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Проверка - работает ли линия
                    if (DataBridge.Conveyor.LineIsRun == true && DataBridge.S7Server.Devices[0].IsAvailable == true)
                    {
                        customMessageBox mb = new customMessageBox("Ошибка", "Задание в работе, перед завершением остановите конвейер.");
                        mb.Owner = DataBridge.MainScreen;
                        mb.ShowDialog();

                        return;
                    }


                    ////Проверка - имеются ли на линии невыпущенныые продукты
                    //if (DataBridge.BoxEngine.ProductCollectionLenght.Value > 0)
                    //{
                    //    customMessageBox mb = new customMessageBox("Ошибка", "Внимание! На ленте конвейера остались считанные, но не выпущенные продукты! Уберите их вручную и очистите очередь ПЛК перед завершением задания");
                    //    mb.Owner = DataBridge.MainScreen;
                    //    mb.ShowDialog();

                    //    return;
                    //}

                    //Вызываем окно авторизации
                    //Вызов окна для авторизации мастера
                    SortingStantion.ToolsWindows.Authorization.windowAuthorizationSuperUser windowAuthorizationSuperUser = new ToolsWindows.Authorization.windowAuthorizationSuperUser();
                    windowAuthorizationSuperUser.Owner = DataBridge.MainScreen;
                    windowAuthorizationSuperUser.ShowDialog();

                    //Если результат авторизации не удачный, выходим
                    if (windowAuthorizationSuperUser.AuthorizationResult == false)
                    {
                        return;
                    }

                    Action action = () =>
                    {
                        //Текст сообщения в зоне информации
                        string message = string.Empty;

                        //сообщение в зоне информации
                        UserMessage msg = null;

                        //Запоминание времени завершения задания
                        DataBridge.Report.endTime = DateTime.Now.GetDateTimeFormats()[43];

                        //Отправка результата на L3
                        var result = DataBridge.Report.SendReport();

                        //Если отправка отчета не удалась
                        if (result == false)
                        {
                            message = "Ошибка при выгрузке результата в ПО верхнего уровня. Восстановите связь и повторите выгрузку";

                            //вывод сообщения в зоне информации
                            msg = new UserMessage(message, DataBridge.myRed);
                            DataBridge.MSGBOX.Add(msg);

                            //Запись в базу данных о завершении задания
                            DataBridge.AlarmLogging.AddMessage("Ошибка отправки отчета на L3", MessageType.TaskLogging);

                            return;
                        }

                        //Сохранение отчета
                        DataBridge.Report.Save("ReportArchive");

                        //Сохранение завершенного задания в файл
                        SelectedWorkAssignment.Save("OrdersArhive");

                        //Удаление резервного файла с принятым, но
                        //не завершенным заданием
                        DeleteBackupFile();

                        //Запись в базу данных
                        message = $"Завершена работа по заданию ID: {SelectedWorkAssignment.ID}";

                        //Запись в базу данных о завершении задания
                        DataBridge.AlarmLogging.AddMessage(message, MessageType.TaskLogging);

                        msg = new UserMessage(message, MSGTYPE.SUCCES);
                        DataBridge.MSGBOX.Add(msg);                      

                        //Уведомление подписчиков о завершении задания
                        WorkOrderCompletionNotification?.Invoke(SelectedWorkAssignment);

                        //Очистка буфера заданий
                        WorkAssignments.Clear();

                        //Очищаем отчет от предыдущих операций
                        DataBridge.Report.ClearResult();

                        //Очистка данных
                        Clear();

                        //Очистка результата
                        DataBridge.Report.ClearResult();

                    };

                    var wcr = new windowClearCollectionRequest(action);

                    return;
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод для создания бэкап файла
        /// </summary>
        public void CreateBackupFile()
        {
            SelectedWorkAssignment.Save("Orders");
        }

        /// <summary>
        /// Метод для удаления резервного файла
        /// </summary>
        void DeleteBackupFile()
        {
            var fn = $@"Orders\{SelectedWorkAssignment.ID}.txt";

            //Удаление файла с заданием из папки Orders
            //по завершению задания
            if (File.Exists(fn))
            {
                File.Delete(fn);
            }
        }

        /// <summary>
        /// Метод для запуcка http listener
        /// </summary>
        async void StartListener()
        {
            //Получение адреса ПК, запросы от которого необходимо
            //прослушивать для получения задания
            var url_jobs = DataBridge.SettingsFile.GetValue("SrvL3UrlJobs");

            //Регистрация url
            NetAclChecker.AddAddress(url_jobs);

            //Инициализация экземпляра  listener
            HttpListener listener = new HttpListener();

            //Установка адресов  listener
            listener.Prefixes.Add(url_jobs);

            //Запуск слушателя
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                //Запись в лог
                Logger.AddExeption("WorkAssignmentEngine.cs", ex);

                //Выход из функции
                return;
            }


            //Цикл для бесконечной прослушки listener
            while (true)
            {
                //Объявление локального делегата
                Action action = null;

                //метод GetContext блокирует текущий поток, ожидая получение запроса
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                // получаем объект ответа
                HttpListenerResponse response = context.Response;

                //Читаем  данные из ответа
                string data = string.Empty;
                using (var reader = new StreamReader(request.InputStream, System.Text.Encoding.UTF8))
                {
                    data = reader.ReadToEnd();
                }

                //Объявление указателя на принятое задание
                WorkAssignment workAssignment = null;

                //Результат получения задания
                var result = false;

                // создаем ответ в виде кода html
                string responseStr = "201";

                //Если данные не пустая строка - производим десериализацию
                try
                {
                    //Десериализация принятого задания
                    workAssignment = JsonConvert.DeserializeObject<WorkAssignment>(data);
                }
                catch (Exception ex)
                {
                    //Удаление задания
                    if (WorkAssignments.Count > 0)
                    {
                        WorkAssignments.RemoveAt(0);
                        this.WorkOrderAcceptanceNotification?.Invoke(null);
                    }
                    
                    action = () =>
                    {
                        //При ошибке десериализации вывод сообщения в зоне информации и продолжение прослушивания сервера заданий
                        UserMessage msg = new Controls.UserMessage("Задание не может быть принято в работу. Неверный формат задания", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(msg);
                    };

                    DataBridge.MainScreen.Dispatcher.Invoke(action);
                    goto M00;
                }
                

                //Проверка задания и перенос задачи в текущую задачу
                result = CheckTask(workAssignment);               

                //В случае, если проверка прошла 
                //успешно
                if (result == true)
                {
                    //Инициализация делегата
                    action = () =>
                    {
                        var msg = new UserMessage($"Поступило задание {workAssignment.ID}. Задание может быть принято в работу.", DataBridge.myGreen);
                        DataBridge.MSGBOX.Add(msg);
                    };

                    //Выводим в потоке UI сообщение
                    DataBridge.MainScreen.Dispatcher.Invoke(action);

                    //Запись в базу данных о том, что получено новое задание
                    string message = $"От L3 поступило новое задание: {workAssignment.ID}";

                    //Запись в базу данных о принятии задания
                    DataBridge.AlarmLogging.AddMessage(message, MessageType.TaskLogging);

                    //Перенос свойств в задание которое может быть
                    //принято в работу
                    if (WorkAssignments.Count == 0)
                    {
                        //Добавление принятого задания в коллекцию заданий
                        WorkAssignments.Add(workAssignment);

                        //Уведомление подписчиков о получении нового задания от L3
                        NewWorkOrderHasArrivedNotification?.Invoke(workAssignment);
                    }
                    else
                    {
                        WorkAssignments[0] = workAssignment;
                    }
                    
                }

                response.StatusCode = 201;

                //В случае, если в процедуре принятия задания
                //произошла ошибка
                M00:  if (result == false)
                {
                    //формирование ответа                   
                    responseStr = "404 Bad Request";
                    response.StatusCode = 404;

                    //КОСТЫЛЬ для того, деактивировать кнопку
                    //ПРИНЯТЬ ЗАДАНИЕ
                    WorkAssignments.Clear();

                    //Уведомление подписчиков о получении нового задания от L3 (пустого)
                    NewWorkOrderHasArrivedNotification?.Invoke(null);
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);

                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // закрываем поток
                output.Close();
            }

            // останавливаем прослушивание подключений
            listener.Stop();
        }

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message, Brush color)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, color);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
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