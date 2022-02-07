using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для Keypad.xaml
    /// </summary>
    public partial class Keypad : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Результат
        /// </summary>
        public string Result
        {
            get
            {
                return _result;
            }
            private set
            {
                _result = value;
                this.OnPropertyChanged("Result");
            }
        }
        private string _result;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Keypad()
        {
            InitializeComponent();
            DataBridge.keypad = this;
            this.DataContext = this;
            Result = "";

            //Обнуление указателя на экран клавиатуры
            this.Closing += (o, s) =>
            {
                DataBridge.keypad = null;
            };
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.CommandParameter.ToString())
            {
                case "ESC":
                    
                    this.DialogResult = false;
                    break;

                case "RETURN":
                    this.DialogResult = true;
                    break;

                case "BACK":
                    if (Result.Length > 0)
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    Result += button.Content.ToString();
                    break;
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
