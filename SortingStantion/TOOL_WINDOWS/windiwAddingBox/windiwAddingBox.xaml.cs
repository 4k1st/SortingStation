using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SortingStantion.windiwAddingBox
{
    /// <summary>
    /// Логика взаимодействия для windiwAddingBox.xaml
    /// </summary>
    public partial class windiwAddingBox : Window
    {
        DispatcherTimer ShutdownTimer;

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
        /// Тэг - разрешить повтор кода продукта
        /// </summary>
        S7_Boolean REPEAT_ENABLE;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windiwAddingBox()
        {
            //Инициализация UI
            InitializeComponent();

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Инициализация сигналов от сканера
            REPEAT_ENABLE = (S7_Boolean)device.GetTagByAddress("DB1.DBX134.0");

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

                //Отписка от события по возникновлению новой ошибки
                DataBridge.NewAlarmNotification -= DataBridge_NewAlarmNotification;
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
            //Сброс таймера отсчитывающего временя бездействия
            TimerReset();

            //Текст сообщения в зоне информации
            string message = string.Empty;

            //Инициализируем разделитель по полям
            var spliter = new DataSpliter();

            //Копируем входные данные в буфер
            var istr = datastring;

            //Разделяем входные данные по полям
            spliter.Split(ref istr);

            /*
                Посторонний код
            */
            if (spliter.IsValid == false)
            {
                //Вывод сообщения в окно информации
                message = $"Код не распознан.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход из функции
                return;
            }

            //Получение GTIN и SN с искуственно добавленым криптохвостом
            var gtin = spliter.GTIN;


            // var serialnumber = spliter.SerialNumber;
            var serialnumber = String.Concat("21", spliter.SerialNumber, "\u001d93", spliter.Crypto);


            /*
                Если Посторонний продукт
            */
            if (DataBridge.WorkAssignmentEngine.GTIN != gtin)
            {
                //Вывод сообщения в окно информации
                message = $"Посторонний продукт не может быть добавлен в результат.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход из функции
                return;
            }

            /*
                Продукт в браке
            */
            if (DataBridge.Report.IsDeffect(serialnumber) == true)
            {
                message = $"Продукт номер {serialnumber.Substring(2,serialnumber.Length - 8)} числится в браке.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход
                return;
            }

            //Флаг, указывающий на то, является ли код повтором
            var IsRepeat = DataBridge.Report.IsRepeat(serialnumber);

            //Получение статуса тэга
            //ПОВТОР КОДА
            //остановки линии
            var RepeatEnable = REPEAT_ENABLE.Value;

            /*
                Продукт уже содержится в результате
            */
            if ((DataBridge.Report.AsAResult(serialnumber) == true || IsRepeat == true) &&  RepeatEnable == false)
            {
                //Формирование сообщения
                message = $"Продукт {serialnumber.Substring(2, serialnumber.Length - 8)} уже есть в результате.";
                ShowMessage(message, DataBridge.myBlue);

                //Выход
                return;
            }


            /*
                Текст сообщения
            */
            message = $"Продукт {serialnumber.Substring(2, serialnumber.Length - 8)} добавлен в результат.";

            //Текст, если код повторился
            if (DataBridge.Report.AsAResult(serialnumber) || DataBridge.Report.IsRepeat(serialnumber))
            {
                message = $"Продукт номер {serialnumber.Substring(2, serialnumber.Length - 8)} считан повторно.";

            }

            /*
                Добавление кода в результ
            */
            DataBridge.Report.AddBox(serialnumber);

            /*
                Добавление в базу данных (лог) записи
            */
            DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.Info);

            /*
                Добавление сообщения в зону информации
            */
            ShowMessage(message, DataBridge.myOrange);

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

        /// <summary>
        /// Метод для сброса таймера,
        /// по которому определяется бездействие
        /// </summary>
        void TimerReset()
        {
            ShutdownTimer.Stop();
            ShutdownTimer.Start();
        }

        /// <summary>
        /// Метод, вызываемый при нажатии кнопки ДОБАВИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
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


    }
}
