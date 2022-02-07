using System;
using System.Windows;
using System.Windows.Threading;

namespace SortingStantion.frameSettings
{
    /// <summary>
    /// Логика взаимодействия для windowExit.xaml
    /// </summary>
    public partial class windowExit : Window
    {
        /// <summary>
        /// Таймер для завершения работы окна
        /// при бездействии
        /// </summary>
        DispatcherTimer ShutdownTimer;

        public windowExit()
        {
            InitializeComponent();

            //Инициализация и запуск таймера для закрытия окна
            //при бездейсвии
            ShutdownTimer = new DispatcherTimer();
            int WindowTimeOut = GetWindowTimeOut();
            ShutdownTimer.Interval = new TimeSpan(0, 0, WindowTimeOut);
            ShutdownTimer.Tick += ShutdownTimer_Tick; 
            ShutdownTimer.Start();
        }



        /// <summary>
        /// Метод, вызываемый при подтверждении
        /// завершения работы комплекса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnYesClick(object sender, RoutedEventArgs e)
        {
            //Внесение в базу данных сообщения об завершении работы комплекса
            DataBridge.AlarmLogging.AddMessage("Завершение работы комплекса", Models.MessageType.Event);

            App.Current.Shutdown();
        }

        /// <summary>
        /// Метод, вызываемый при отмене
        /// завершения работы комплекса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancelClick(object sender, RoutedEventArgs e)
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
