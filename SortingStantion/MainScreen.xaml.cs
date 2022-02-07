using S7Communication;
using SortingStantion.Controls;
using System;
using System.Windows;

namespace SortingStantion.MainScreen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainScreen : Window
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
        /// Тэг, указывающий на то, что
        /// HMI запущен
        /// </summary>
        S7_Boolean S7_HMI_IS_RUN;

        public MainScreen()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;

            S7_HMI_IS_RUN = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.5");


            //Внесение в базу данных сообщения о запуске приложения 
            DataBridge.AlarmLogging.AddMessage("Запуск приложения", Models.MessageType.Event);

            //ПЕредача экземпляра окна
            DataBridge.MainScreen = this;

            //Передача  UI Dispatcher
            DataBridge.UIDispatcher = this.Dispatcher;

            this.Closing += MainScreen_Closing;

            //Уведомление подписчиков о завершении
            //загрузки приложения
            DataBridge.LoadCompleteNotification();

            //Запись в ПЛК, что HMI запущен
            S7_HMI_IS_RUN.Write(true);
        }

        /// <summary>
        /// Метод для остановки конвейера при 
        /// завершении работы программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainScreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Уведомление подписчиков о 
            //завершении работы комплекса 
            DataBridge.ShutdownNotification();

            //Запись файла с результатом
            //операций (если отчет не отправлен)
            DataBridge.Report.CreateBackupFile();

            //Запись в ПЛК, что работа HMI остановленно корректно
            S7_HMI_IS_RUN.Write(false);
        }
    }
}
