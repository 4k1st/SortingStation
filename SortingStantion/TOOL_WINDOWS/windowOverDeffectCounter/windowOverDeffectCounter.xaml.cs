using S7Communication;
using SortingStantion.S7Extension;
using System;
using System.Threading;
using System.Windows;

namespace SortingStantion.ToolsWindows.windowOverDeffectCounter
{
    /// <summary>
    /// Логика взаимодействия для windowOverDeffectCounter.xaml
    /// </summary>
    public partial class windowOverDeffectCounter : Window
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
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        S7DiscreteAlarm alarm;

        /// <summary>
        /// Счетчик брака
        /// </summary>
        S7_DWord DEFFECT_PRODUCTS_COUNTER;

        /// <summary>
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean CLEAR_ITEMS_COLLECTION_CMD;


        public windowOverDeffectCounter(S7_DWord DEFFECT_PRODUCTS_COUNTER, S7DiscreteAlarm alarm)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача экземпляра тэга со счетчиком ошибок
            this.DEFFECT_PRODUCTS_COUNTER = DEFFECT_PRODUCTS_COUNTER;

            //Тэг для очистки коллекции изделий
            CLEAR_ITEMS_COLLECTION_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

            //Подписка на изменение значение счетчика
            this.DEFFECT_PRODUCTS_COUNTER.ChangeValue += DEFFECT_PRODUCTS_COUNTER_ChangeValue;

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.alarm = alarm;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"Конвейер остановлен после {DEFFECT_PRODUCTS_COUNTER.Value} продуктов, отбракованных подряд. Уберите все продукты между сканером и отбраковщиком, они не будут добавлены в результат.";

            //Подписка на события
            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Изменение значения счетчика
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void DEFFECT_PRODUCTS_COUNTER_ChangeValue(object arg1, object arg2)
        {
            Action action = () =>
            {
                //Формирование правиьного сообщения
                txMessage.Text = $"Конвейер остановлен после {DEFFECT_PRODUCTS_COUNTER.Value} продуктов, отбракованных подряд. Уберите все продукты между сканером и отбраковщиком, они не будут добавлены в результат.";
            };
            DataBridge.UIDispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            //Обнуление счетчика дефектных продуктов
            DEFFECT_PRODUCTS_COUNTER.Write(0);

            //Очистка коллекции невыпущенных продуктов
            CLEAR_ITEMS_COLLECTION_CMD.Write(true);

            //Квитирование аварии
            alarm.Write(false);

            //Отписка от метода, вызываемого при закрытии окна
            this.Closing -= Window_Closing;

            //Отписка от изменения значение счетчика
            this.DEFFECT_PRODUCTS_COUNTER.ChangeValue -= DEFFECT_PRODUCTS_COUNTER_ChangeValue;

            //Закрытие окна
            this.Close();
        }

        /// <summary>
        /// Метод, вызываемый при закрытии окна
        /// (отмена закрытия окна)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
