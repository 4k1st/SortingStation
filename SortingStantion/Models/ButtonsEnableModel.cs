using S7Communication;
using System.ComponentModel;

namespace SortingStantion.Models
{
    /// <summary>
    /// Модель, управляющая доступом кнопок
    /// </summary>
    public class ButtonsEnableModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "НАСТРОЙКИ"
        /// </summary>
        public bool BtnSettingsEnable
        {
            get
            {
                /*
                    Флаг, указывающий на то, принято ли задание в работу 
                */
                var inwork = DataBridge.WorkAssignmentEngine.InWork;


                /*
                    Указатель на список принятых от L3 заданий
                */
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;


                /*
                    Указатель на текущего авторизованного пользователя
                */
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //1. Задание не прислано в комплекс
                //   - АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return true;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "АВТОРИЗАЦИЯ"
        /// </summary>
        public bool BtnAutorizationEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return true;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }
        bool _btnAutorizationEnable = false;

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на главном экране
        /// </summary>
        public bool BtnStartEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Флаг, указывающий на то, запущен ли конвейер
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //Если отсутсвует связь с ПЛК
                if (device.IsAvailable == false)
                {
                    return false;
                }

                //Если отсутствует деблокировка
                if (DataBridge.AlarmsEngine.Interlock == false)
                {
                    return false;
                }

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА и линия не запущена
                if (inwork == true && currentUser != null && lineisrun == false)
                {
                    return true;
                }

                //Возврат false
                return false;
            }

        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на главном экране
        /// </summary>
        public bool BtnStopEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Флаг, указывающий на то, запущен ли конвейер
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //Если отсутсвует связь с ПЛК
                if (device.IsAvailable == false)
                {
                    return false;
                }

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА и линия запущена
                if (inwork == true && currentUser != null && lineisrun == true)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на экране настроек
        /// </summary>
        public bool BtnStartForceEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Флаг, указывающий на то, запущен ли конвейер в принудительном режиме
                var lineisforcerun = DataBridge.Conveyor.LineIsForceRun;

                //Флаг, указывающий на то, запущен ли конвейер в нормальном режиме
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //Если отсутсвует связь с ПЛК
                if (device.IsAvailable == false)
                {
                    return false;
                }

                //Если отсутствует деблокировка
                if (DataBridge.AlarmsEngine.Interlock == false)
                {
                    return false;
                }

                //Если задание в работе, то доступность кнопки такая
                //же как и BtnStop
                if (inwork == true)
                {
                    return lineisrun == false;
                }

                //Если линия запущена в принудительном режиме
                return lineisforcerun == false;
            }

        }

        /// <summary>
        /// Флаг, разрешающий использовать кнопку
        /// "СТАРТ-СТОП" на экране настроек
        /// </summary>
        public bool BtnStopForceEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Флаг, указывающий на то, запущен ли конвейер в принудительном режиме
                var lineisforcerun = DataBridge.Conveyor.LineIsForceRun;

                //Флаг, указывающий на то, запущен ли конвейер в нормальном режиме
                var lineisrun = DataBridge.Conveyor.LineIsRun;

                //Если отсутсвует связь с ПЛК
                if (device.IsAvailable == false)
                {
                    return false;
                }

                //Если задание в работе, то доступность кнопки такая
                //же как и BtnStop
                if (inwork == true)
                {
                    return lineisrun == true;
                }

                //Если линия запущена в принудительном режиме
                return lineisforcerun == true;
            }
        }

        /// <summary>
        /// Флаз, задающий режим доступа к кнопке 
        /// ПРИНЯТЬ ЗАДАНИЕ на главном экране
        /// </summary>
        public bool BtnAcceptTaskEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //1. Задание не прислано в комплекс
                //   - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   - АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаз, задающий режим доступа к кнопке 
        /// ЗАВЕРШИТЬ ЗАДАНИЕ на главном экране
        /// </summary>
        public bool BtnFinishTaskEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return true;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }

        /// <summary>
        /// Флаг, задающий режим доступа к кнопкам 
        /// БРАК СПРАВКА ДОБАВИТЬ на главном экране
        /// </summary>
        public bool BtnToolsEnable
        {
            get
            {
                //Флаг, указывающий на то, принято ли задание в работу
                var inwork = DataBridge.WorkAssignmentEngine.InWork;

                //Указатель на список принятых от L3 заданий
                var workAssignmentsList = DataBridge.WorkAssignmentEngine.WorkAssignments;

                //Указатель на текущего авторизованного пользователя
                var currentUser = DataBridge.MainAccesLevelModel.CurrentUser;

                //1. Задание не прислано в комплекс - НЕ АКТИВНА
                if (workAssignmentsList.Count == 0 && inwork == false)
                {
                    return false;
                }

                //2. Задание прислано в комплекс
                //   но в работу не принято - НЕ АКТИВНА
                if (workAssignmentsList.Count > 0 && inwork == false)
                {
                    return false;
                }

                //3. Задание прислано в комплекс
                //   и принято в работу, но пользователь не авторизирован - НЕ АКТИВНА
                if (inwork == true && currentUser == null)
                {
                    return false;
                }

                //Если активна ошибка отправки отчета
                if (DataBridge.Report.SendReportErrorMemmoryFlag == true)
                {
                    return false;
                }

                //4. Задание прислано в комплекс
                //   и принято в работу, пользователь авторизирован - АКТИВНА
                if (inwork == true && currentUser != null)
                {
                    return true;
                }

                //Возврат false
                return false;
            }
        }


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ButtonsEnableModel()
        {
            DataBridge.LoadComplete += DataBridge_LoadComplete;           
        }

        /// <summary>
        /// Метод, вызываемый по окончании инициализации
        /// приложения
        /// </summary>
        private void DataBridge_LoadComplete()
        {
            //Подпись на событие по изменению пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (a1, a2, archive) =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnSettingsEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnToolsEnable");
                

            };

            //Подпись на событие по изменению сотояния статуса
            //линии в нормальном режиме
            DataBridge.Conveyor.ChangeOfStateInNormalMode += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnStartForceEnable");
                OnPropertyChanged("BtnStopForceEnable");
            };

            //Подпись на событие по изменению сотояния статуса
            //линии в принудительном режиме
            DataBridge.Conveyor.ChangeOfStateInForceMode += () =>
            {
                OnPropertyChanged("BtnStartForceEnable");
                OnPropertyChanged("BtnStopForceEnable");
            };

            //Подписка на событие по получению нового
            //задания от сервера L3
            DataBridge.WorkAssignmentEngine.NewWorkOrderHasArrivedNotification += (oreder) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Подпись на событие по принятию нового задания
            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (o) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Подпись на событие по завершению задания
            //и отправке результата
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (o) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Инициализация тэгов
            var IN_WORK_TAG = (S7_Boolean)device.GetTagByAddress("DB1.DBX148.0");
            IN_WORK_TAG.ChangeValue += (oldvalue, newvalue) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            //Изменение состояния деблокировки
            DataBridge.AlarmsEngine.ChangeInterlockState += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStartForceEnable");
            };

            //Изменение статуса отправки отчета
            DataBridge.Report.SendReportNotification += (status) =>
            {
                OnPropertyChanged("BtnSettingsEnable");
                OnPropertyChanged("BtnAutorizationEnable");

                OnPropertyChanged("BtnAcceptTaskEnable");
                OnPropertyChanged("BtnFinishTaskEnable");

                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnToolsEnable");
            };

            device.GotConnection += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnStartForceEnable");
                OnPropertyChanged("BtnStopForceEnable");
            };

            device.LostConnection += () =>
            {
                OnPropertyChanged("BtnStartEnable");
                OnPropertyChanged("BtnStopEnable");

                OnPropertyChanged("BtnStartForceEnable");
                OnPropertyChanged("BtnStopForceEnable");
            };
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
