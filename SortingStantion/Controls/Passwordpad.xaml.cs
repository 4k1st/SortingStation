using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для Keypad.xaml
    /// </summary>
    public partial class Passwordpad : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Результат
        /// </summary>
        public string Result
        {
            get
            {
                return passwordBox.Password;
            }
            private set
            {
                passwordBox.Password = value;
            }
        }
        private string _result;

        public Passwordpad()
        {
            InitializeComponent();
            this.DataContext = this;
            Result = "";
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
