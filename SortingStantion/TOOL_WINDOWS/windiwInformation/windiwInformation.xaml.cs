using SortingStantion.Controls;
using System;
using System.Windows;
using System.Windows.Threading;

namespace SortingStantion.ToolsWindows.windiwInformation
{
    /// <summary>
    /// Логика взаимодействия для windiwInformation.xaml
    /// </summary>
    public partial class windiwInformation : Window
    {
        DispatcherTimer ShutdownTimer;

        /// <summary>
        /// Строка, содержащая серийный номер
        /// продукта, добавляемого в брак
        /// </summary>
        string CurrentSerialNumber
        {
            get
            {
                return currentSerialNumber;
            }
            set
            {
                //Запоминание серийного номера
                currentSerialNumber = value;

                //Сброс таймера отсчета времени бездействия
                ShutdownTimer.Stop();
                ShutdownTimer.Start();
            }
        }
        string currentSerialNumber;

        /// <summary>
        /// Текущий, просканированый 
        /// GTIN прдукта
        /// </summary>
        string CurrentGTIN
        {
            get
            {
                return currentGTIN;
            }
            set
            {
                currentGTIN = value;
            }
        }
        string currentGTIN;

        /// <summary>
        /// Текущий, просканированый 
        /// криптохвост прдукта
        /// </summary>
        string CurrentCrypto
        {
            get
            {
                return currentCrypto;
            }
            set
            {
                currentCrypto = value;
            }
        }
        string currentCrypto;


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windiwInformation()
        {
            //Инициализация UI
            InitializeComponent();

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Инициализация и запуск таймера для закрытия окна
            //при бездейсвии
            ShutdownTimer = new DispatcherTimer();
            int WindowTimeOut = GetWindowTimeOut();
            ShutdownTimer.Interval = new TimeSpan(0, 0, WindowTimeOut);
            ShutdownTimer.Tick += ShutdownTimer_Tick;
            ShutdownTimer.Start();

            //Подписка но событие по возникновлению новой ошибки
            DataBridge.NewAlarmNotification += DataBridge_NewAlarmNotification;

            //Выражение, вызываемое при закрытии
            //данного экземпляра окна
            this.Closing += (e, s) =>
            {
                //Отписка от обытия по получению данных от сканера
                DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;

                //Отписка от обытия по получению данных от сканера
                DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;
            };
        }

        /// <summary>
        /// Метод, вызываемый при возникновлении
        /// новой аварии
        /// </summary>
        private void DataBridge_NewAlarmNotification()
        {
            this.Close();
        }

        /// <summary>
        /// Метод, вызываемый при получении данных
        /// от сканера
        /// </summary>
        /// <param name="obj"></param>
        private void Scaner_NewDataNotification(string datastring)
        {
            //Разбор данных по полям
            var inputdata = datastring;
            DataBridge.DataSpliter.Split(ref inputdata);

            //Добавление номера продукта в буфер текущего окна
            bool isvalid = DataBridge.DataSpliter.IsValid;


            //искуственно добавляем криптохвост к серийнику
            CurrentSerialNumber = String.Concat("21", DataBridge.DataSpliter.GetSerialNumber(), "\u001d93", DataBridge.DataSpliter.GetCrypto());

            //CurrentSerialNumber = DataBridge.DataSpliter.GetSerialNumber();
            CurrentGTIN = DataBridge.DataSpliter.GetGTIN();
            //CurrentCrypto = DataBridge.DataSpliter.GetCrypto();
            
            // Сообщение из зоны информации
            string message = string.Empty;

            //Посторонний продукт
            if (isvalid == false)
            {
                message = "Посторонний код";
                ShowMessage(message);
                return;
            }

            //Посторонний GTIN
            if (DataBridge.WorkAssignmentEngine.GTIN != CurrentGTIN)
            {
                message = "Код другого продукта";
                ShowMessage(message);
                return;
            }

            //Продукт в браке
            if (DataBridge.Report.IsDeffect(CurrentSerialNumber) == true)
            {
                message = $"Продукт {CurrentSerialNumber.Substring(2, CurrentSerialNumber.Length - 8)} в браке.";

                ShowMessage(message);
                return;
            }

            //Продукт в результате
            if (DataBridge.Report.AsAResult(CurrentSerialNumber) == true)
            {
                message = $"Продукт {CurrentSerialNumber.Substring(2, CurrentSerialNumber.Length - 8)} в результате.";
                ShowMessage(message);
                return;
            }

            //Продукт «s/n» доступен для сериализации
            message = $"Продукт {CurrentSerialNumber.Substring(2, CurrentSerialNumber.Length - 8)} доступен для сериализации.";
            ShowMessage(message);



        }

        /// <summary>
        /// Метод для отображения сообщения в окне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, MSGTYPE.INFO);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод, вызываемый при автоматическом закрытии окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShutdownTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Метод для получения интервала из файла конфигурации
        /// </summary>
        /// <returns></returns>
        int GetWindowTimeOut()
        {
            //Получение оригинального значения из файла конфигурации
            string strvalue = DataBridge.SettingsFile.GetValue("WindowTimeOut");

            //Объявление переменной
            //для хранения значения интервала для закрытия окна
            int interval;

            //Приведение интервала к типу int
            var result = int.TryParse(strvalue, out interval);

            if (result == true)
            {
                return interval;
            }

            return 60;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
