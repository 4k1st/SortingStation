using S7Communication;
using System;
using System.Windows;

namespace SortingStantion.ToolsWindows.windowClearCollectionRequest
{
    /// <summary>
    /// Логика взаимодействия для windowClearCollectionRequest.xaml
    /// </summary>
    public partial class windowClearCollectionRequest : Window
    {
        /// <summary>
        /// События, вызываемые по клику кнопки
        /// Продолжить
        /// </summary>
        public Action BtnContinueClickEvent;

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
        /// Количество отсканированных, но не выпущенных
        /// продуктов
        /// </summary>
        S7_DWord S7ProductCollectionLenght;

        /// <summary>
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean S7ClearItemsCollectionCMD;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public windowClearCollectionRequest(Action btnContinueClickAction)
        {
            //Инициализация UI
            InitializeComponent();

            this.Owner = DataBridge.MainScreen;

            //Передача события по клику кнопки
            BtnContinueClickEvent = btnContinueClickAction;

            //Инициализация тэга - количество невыпущенных продуктов
            S7ProductCollectionLenght = (S7_DWord)device.GetTagByAddress("DB5.DBD0-DWORD");

            //Тэг для очистки коллекции изделий
            S7ClearItemsCollectionCMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

            //Если количество невыпущенных продуктов
            //меньше нуля - высываем событие, которое вызывается при
            //нажатии кнопки "ПРОДОЛЖИТЬ"
            if (S7ProductCollectionLenght.Value == 0)
            {
                BtnContinueClickEvent?.Invoke();
                BtnContinueClickEvent = null;
                this.Close();
                return;
            }

            //Если Невыпущеных продуктов
            //больше 0, вызываем диалог
            this.ShowDialog();
        }

        /// <summary>
        /// Событие, вызваемое при клике по клавише
        /// "ОТМЕНА"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            BtnContinueClickEvent = null;
            this.Close();
        }

        /// <summary>
        /// Событие, вызываемое при нажатии на
        /// клавишу "ПРОДОЛЖИТЬ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnContinueClick(object sender, RoutedEventArgs e)
        {
            S7ClearItemsCollectionCMD.Write(true);
            BtnContinueClickEvent?.Invoke();
            BtnContinueClickEvent = null;
            this.Close();
        }
    }
}
