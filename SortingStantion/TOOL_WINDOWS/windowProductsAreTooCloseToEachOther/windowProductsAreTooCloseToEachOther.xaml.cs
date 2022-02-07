using S7Communication;
using System.Windows;

namespace SortingStantion.ToolsWindows.windowProductsAreTooCloseToEachOther
{
    /// <summary>
    /// Логика взаимодействия для windowProductsAreTooCloseToEachOther.xaml
    /// </summary>
    public partial class windowProductsAreTooCloseToEachOther : Window
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
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean CLEAR_ITEMS_COLLECTION_CMD;

        public windowProductsAreTooCloseToEachOther(string message)
        {
            //Инициализация UI
            InitializeComponent();

            //Вывод сообщения в окне
            this.txMessage.Text = message;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Тэг для очистки коллекции изделий
            CLEAR_ITEMS_COLLECTION_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

        }

        /// <summary>
        /// Метод, вызываемый при нажатии кнопки - ЗАКРЫТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            //Очистка коллекции продуктов между сканером
            //и отбраковщиком
            CLEAR_ITEMS_COLLECTION_CMD.Write(true);

            //Закрытие окна
            this.Close();
        }
    }
}
