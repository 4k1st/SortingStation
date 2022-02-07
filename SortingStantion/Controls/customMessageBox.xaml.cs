using System.ComponentModel;
using System.Windows;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для customMessageBox.xaml
    /// </summary>
    public partial class customMessageBox : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
            }

        }

        /// <summary>
        /// Сообщение
        /// </summary>
        string message = string.Empty;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }

        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public customMessageBox(string caption, string message)
        {
            //Инициализация UI
            InitializeComponent();

            //Передача свойств
            Message = message;
            Caption = caption;

            //Передача указателя на окно, в центе которого 
            //надо разместить окна
            this.Owner = DataBridge.MainScreen;

            //Передача датаконтекста
            DataContext = this;
        }

        /// <summary>
        /// Метод вызываемый при нажатии на
        /// кнопку ЗАКРЫТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            { 
            
            }
        }

        /// <summary>
        /// Станичный метод для отображения окна
        /// сообщения
        /// </summary>
        /// <param name="message"></param>
        public static void Show(string message)
        {
            customMessageBox cmb = new customMessageBox("Ошибка", message);
            cmb.ShowDialog();
        }
    }
}
