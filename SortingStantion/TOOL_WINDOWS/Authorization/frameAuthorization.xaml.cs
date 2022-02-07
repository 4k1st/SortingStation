using SortingStantion.Controls;
using SortingStantion.Models;
using SortingStantion.Utilites;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SortingStantion.UserAdmin
{
    /// <summary>
    /// Логика взаимодействия для frameAuthorization.xaml
    /// </summary>
    public partial class frameAuthorization : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Таймер для автоматического закрытия окна
        /// при бездействии
        /// </summary>
        DispatcherTimer ShutdownTimer;

        /// <summary>
        /// Имена пользователей
        /// </summary>
        public ObservableCollection<User> Users
        {
            get
            { 
            return DataBridge.MainAccesLevelModel.Users;
            }
            set
            {
                DataBridge.MainAccesLevelModel.Users = value;
            }
        }

        /// <summary>
        /// Результат последней авторизации
        /// </summary>
        public bool AuthorizationResult
        {
            get;
            set;
        }


        /// <summary>
        /// Выбраный в combobox пользователь
        /// </summary>
        User selectedUser = null;
        public User SelectedUser
        {
            get
            {
                return selectedUser;
            }
            set
            {
                selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }

        /// <summary>
        /// Флаг, по которому определяется надо ли 
        /// игнорировать повторную авторизацию
        /// старого пользователя
        /// </summary>
        bool repeatIgnore;


        /// <summary>
        /// Конструктор класса
        /// </summary>
        public frameAuthorization(bool RepeatIgnore = true)
        {
            //Передача флага
            repeatIgnore = RepeatIgnore;

            //иИнициализация UI
            InitializeComponent();

            //Подписка на обытие по получению данных от сканера
            DataBridge.Scaner.NewDataNotification += Scaner_NewDataNotification;

            //Инициализация и запуск таймера для закрытия окна
            //при бездейсвии
            ShutdownTimer = new DispatcherTimer();
            int WindowTimeOut = GetWindowTimeOut();
            ShutdownTimer.Interval = new TimeSpan(0, 0, WindowTimeOut);
            ShutdownTimer.Tick += ShutdownTimer_Tick;
            ShutdownTimer.Start();

            this.Owner = DataBridge.MainScreen;

            //Выражение, вызываемое при закрытии
            //данного экземпляра окна
            this.Closing += (e, s) =>
            {
                //Отписка от обытия по получению данных от сканера
                DataBridge.Scaner.NewDataNotification -= Scaner_NewDataNotification;
            };

            //Передача контекста данных
            DataContext = this;
        }

        /// <summary>
        /// Метод, вызываемый при получении данных
        /// от ручного сканера
        /// </summary>
        /// <param name="obj"></param>
        private void Scaner_NewDataNotification(string scanresult)
        {
            //Текст сообщения в зоне информации
            string message = string.Empty;

            //Поиск пользователя по ID
            var id = scanresult;

            //Получение коллекции пользователей
            var users = DataBridge.MainAccesLevelModel.Users;

            //поиск пользователя по ID
            foreach(var user in users)
            {
                if (user.ID == id)
                {
                    SelectedUser = user;
                    break;
                }
            }          

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

        /// <summary>
        /// Метод, вызываемый при получении 
        /// фокуса пасвордбоксом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pwbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            //Получаем экземпляр TextBox
            PasswordBox originalsourse = (PasswordBox)e.OriginalSource;

            //Открытие клавиатуры
            Passwordpad keypadWindow = new Passwordpad();

            //Если нажата кнопка Enter - 
            //запись значения в поле
            if (keypadWindow.ShowDialog() == true)
            {
                originalsourse.Password = keypadWindow.Result;
                
            }

            rect.Focus();
        }


        /// <summary>
        /// Команда авторизации пользователя
        /// </summary>
        public ICommand AuthorizationCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AuthorizationResult = DataBridge.MainAccesLevelModel.CheckPassword(SelectedUser.Name, pwbPassword.Password);
                    DialogResult = AuthorizationResult;
                    this.Close();
                },
                (obj) => (SelectedUser != null));
            }
        }

        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion


    }
}
