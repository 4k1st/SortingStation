using SortingStantion.Controls;
using SortingStantion.UserAdmin;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml;

namespace SortingStantion.Models
{
    /// <summary>
    /// Перечисление уровней доступа
    /// </summary>
    public enum AccesLevels
    {
        [Description("Мастер")]
        Мастер = 1,

        [Description("Наладчик")]
        Наладчик = 2,
    }

    /// <summary>
    /// Класс, реализующий разграничение 
    /// прав пользователя
    /// </summary>
    public class AuthorizationEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// Коллекция пользователей
        /// </summary>
        public ObservableCollection<User> Users
        {
            get;
            set;
        }

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser
        {
            get
            {
                return currentUser;
            }
            set
            {
                currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
        }
        User currentUser = new User();

        /// <summary>
        /// Отображаемое уровнь - имя
        /// текущего пользователя
        /// на модели представления
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (CurrentUser == null)
                {
                    return "Мастер:";
                }

                //return $"{CurrentUser.AccesLevel}:\n{CurrentUser.Name}";
                return $"Мастер:\n{CurrentUser.Name}";
            }
        }

        /// <summary>
        /// Событие, возникающее при смене
        /// пользователя
        /// </summary>
        public event Action<int, User, bool> ChangeUser;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AuthorizationEngine()
        {
            //Инициализация текущего пользователя
            CurrentUser = null;

            //Инициализация коллекции пользователей
            Users = new ObservableCollection<User>();

            //Загружаем из файла пользователей
            LoadFromXML();
        }

        /// <summary>
        /// Метод для загрузки данных из файла
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool LoadFromXML()
        {
            //Создание экземпляра документа
            XmlDocument xDoc = new XmlDocument();

            //Заргузка из файла
            try
            {
                xDoc.Load(@"AppData\Users.xml");
            }
            catch (Exception ex)
            {
                return false;
            }

            //Получение корневого элемента
            XmlElement root = xDoc.DocumentElement;

            //Обход всех узлов в корневом элементе
            foreach (XmlNode userNode in root)
            {
                if (userNode.Name.ToUpper() == "USER")
                {
                    var user = new User(userNode);
                    this.Users.Add(user);
                }
            }

            return true;
        }


        /// <summary>
        /// Метод для авторизации
        /// </summary>
        public bool Login(string login, string Password, bool RepeatIgnore = true, bool Archive=true)
        {
            //Локальные переменные для вывода сообщения
            //в зоне информации
            UserMessage messageItem;

            foreach (var user in Users)
            {
                //Если имя пользователя не соответствует
                //имени пользователя из коллеции
                //переходим к следующему пользователю
                if (user.Name.ToUpper() != login.ToUpper())
                {
                    continue;
                }

                ////Если пользователь валидный
                ////извещаем подписчиков и делаем запись в базу данных
                if (user.Password == Password)
                {
                    //Если текущий пользователь тот же, который был,
                    //пишем
                    if (currentUser == user && RepeatIgnore == false)
                    {
                        messageItem = new Controls.UserMessage("Пользователь уже авторизован", DataBridge.myRed);
                        DataBridge.MSGBOX.Add(messageItem);

                        return false;
                    }

                    //Новый уровень доступа
                    var accesLevel = (int)user.AccesLevel;

                    //Сообщение
                    var message = $"Авторизован новый пользователь: {user.Name} с уровнем доступа: {user.AccesLevel.ToString()}";

                    //В случае успешной авторизации МАСТЕРА
                    if (accesLevel < 2)
                    {

                        if (Archive == true)
                        {
                            CurrentUser = user;
                            OnPropertyChanged("DisplayName");
                        }


                        //Уведомление подписчиков об авторизации нового пользователя
                        ChangeUser?.Invoke((int)CurrentUser.AccesLevel, user, Archive);

                        //Выводим сообщение об успешной авторизации
                        messageItem = new Controls.UserMessage(message, DataBridge.myGreen);
                        DataBridge.MSGBOX.Add(messageItem);
                    }
                    
                    //Добавление записи в базу данных
                    DataBridge.AlarmLogging.AddMessage(message, Models.MessageType.ChangeUser);

                    //Возврат результата
                    return true;
                }
            }

            //Выводим сообщение, что при ошибке возникла ошибка авторизации
            messageItem = new Controls.UserMessage("Ошибка авторизации", DataBridge.myRed);
            DataBridge.MSGBOX.Add(messageItem);

            return false;
        }

        /// <summary>
        /// Метод для проверки логина-пароля
        /// </summary>
        public bool CheckPassword(string login, string Password)
        {

            foreach (var user in Users)
            {
                //Если имя пользователя не соответствует
                //имени пользователя из коллеции
                //переходим к следующему пользователю
                if (user.Name.ToUpper() != login.ToUpper())
                {
                    continue;
                }

                ////Если пользователь валидный
                ////извещаем подписчиков и делаем запись в базу данных
                if (user.Password == Password)
                {
                    //Возврат результата
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Выход из логина
        /// </summary>
        public void Logout()
        {
            CurrentUser = new User();
            //ChangeUser?.Invoke((int)CurrentUser.AccesLevel);
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
