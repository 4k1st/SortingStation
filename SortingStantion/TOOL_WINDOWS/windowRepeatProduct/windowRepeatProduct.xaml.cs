using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace SortingStantion.ToolsWindows.windowRepeatProduct
{
    /// <summary>
    /// Логика взаимодействия для windowRepeatProduct.xaml
    /// </summary>
    public partial class windowRepeatProduct : Window
    {

        /// <summary>
        /// Указательн на сообщение
        /// в зоне информации, которое надо удалить
        /// </summary>
        UserMessage userMessage;

        /// <summary>
        /// Тэг, хранящий количество изделий, отбраковыных вручную
        /// </summary>
        S7_DWord QUANTITY_PRODUCTS_MANUAL_REJECTED;

        /// <summary>
        /// GTIN
        /// </summary>
        string GTIN;

        /// <summary>
        /// SerialNumber
        /// </summary>
        string serialnumber;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="GTIN"></param>
        /// <param name="serialnumber"></param>
        /// <param name="userMessage"></param>
        public windowRepeatProduct(string GTIN, string serialnumber, UserMessage userMessage)
        {
            //Инициализация UI
            InitializeComponent();

            //Перенос полей в буфер
            this.serialnumber = serialnumber;
            this.GTIN = GTIN;

            //Передача указателя на сообщение в зоне
            //информации, которое надо удалить при нажатии кнопки Отмена
            this.userMessage = userMessage;

            //Подписка на событие по приему данных от ручного сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Формирование правиьного сообщения
            txMessage.Text = $"Продукт GTIN {GTIN} номер {serialnumber.Substring(2, serialnumber.Length - 8)} считан повторно. Найдите его ручным сканером и удалите с конвейера.";

            //Подписка на события
            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Метод, вызываемый при получении новых
        /// данных от ручного сканера
        /// </summary>
        /// <param name="obj"></param>
        private void Scaner_NewDataNotification(string inputdata)
        {
            //Текст сообщения в зоне информации
            string message = string.Empty;

            //Инициализируем разделитель по полям
            var spliter = new DataSpliter();

            //Копируем входные данные в буфер
            var istr = inputdata;

            //Разделяем входные данные по полям
            spliter.Split(ref istr);

            /*
                Посторонний код
            */
            if (spliter.IsValid == false)
            {
                //Вывод сообщения в окно информации
                message = $"Код не распознан. Удалите продукт с конвейера.";
                ShowMessage(message, DataBridge.myRed);

                //Выход из функции
                return;
            }

            //Получение GTIN и SN с искуственно добавленным криптохвостом
            var gtin = spliter.GTIN;
            //  var serialnumber = spliter.SerialNumber;
            var serialnumber = String.Concat("21", spliter.SerialNumber, "\u001d93", spliter.Crypto);


            /*
                Если Посторонний продукт
            */
            if (DataBridge.WorkAssignmentEngine.GTIN != gtin)
            {
                //Вывод сообщения в окно информации
                message = $"Посторонний продукт GTIN {gtin} номер {serialnumber.Substring(2, serialnumber.Length - 8)}. Удалите его с конвейера.";
                ShowMessage(message, DataBridge.myRed);

                //Выход из функции
                return;
            }

            /*
               Продукт в браке
            */
            if (DataBridge.Report.IsDeffect(serialnumber) == true)
            {
                message = $"Продукт номер {serialnumber.Substring(2, serialnumber.Length - 8)} числиться в браке. Удалите его с конвейера.";
                ShowMessage(message, DataBridge.myRed);
                return;
            }

            /*
               Продукт считан повторно 
            */
            if (DataBridge.Report.IsContentsRepeatCollection(serialnumber) == true)
            {
                var brush = DataBridge.myRed;

                //Если мы считали тот код, на котором
                //возникла ошибка - выводим сообщение на зеленом фоне
                if (serialnumber == this.serialnumber)
                {
                    brush = DataBridge.myGreen;
                }

                message = $"Продукт номер {serialnumber.Substring(2, serialnumber.Length - 8)} считан повторно. Удалите его с конвейера.";
                ShowMessage(message, brush);
                return;
            }

            /*
                Если код в результате. 
            */
            if (DataBridge.Report.AsAResult(serialnumber) == true)
            {
                //Вывод сообщения в окно информации
                message = $"Продукт GTIN {gtin} номер {serialnumber.Substring(2, serialnumber.Length - 8)} в результате.";
                ShowMessage(message, DataBridge.myRed);

                //Выход из функции
                return;
            }


            /*
               Продукт «s/n» доступен для сериализации
            */
            message = $"Продукт GTIN {gtin} номер {serialnumber.Substring(2, serialnumber.Length - 8)} доступен для сериализации.";
            ShowMessage(message, DataBridge.myRed);
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
        /// Метод, вызываемый при клике по кнопке - ОТМЕНА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataBridge.MSGBOX.Remove(userMessage);
            this.Closing -= Window_Closing;
            DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;
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
