using System.ComponentModel;
using System.Xml;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий описание
    /// ользователя для разграничения прав пользователя
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        string name;

        /// <summary>
        /// Текстовое представление 
        /// уровня доступа
        /// </summary>
        public string Access
        {
            get
            {

                if (AccesLevel == AccesLevels.Мастер)
                {
                    return "МАСТЕР";
                }

                if (AccesLevel == AccesLevels.Наладчик)
                {
                    return "НАЛАДЧИК";
                }

                return "ОПЕРАТОР";
            }
        }

        /// <summary>
        /// Штрихкод пользователя
        /// </summary>
        public string Barcode
        {
            get
            {
                return barcode;
            }
            set
            {
                barcode = value;
                OnPropertyChanged("Barcode");
            }
        }
        string barcode;

        /// <summary>
        /// ID пользователя
        /// </summary>
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged("ID");
            }
        }
        string id;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        string password;

        /// <summary>
        /// Уровень доступа пользователя
        /// </summary>
        public AccesLevels AccesLevel
        {
            get
            {
                return accesLevel;
            }
            set
            {
                accesLevel = value;
                OnPropertyChanged("AccesLevel");
            }
        }
        AccesLevels accesLevel = AccesLevels.Мастер;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public User()
        {
            UpdateProperties();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public User(XmlNode node)
        {
            //Парсим узел
            foreach (XmlNode attribut in node.ChildNodes)
            {
                //УЗЕЛ - ИМЯ
                if (attribut.Name.ToUpper() == "NAME")
                {
                    this.Name = attribut.InnerText;
                }

                //УЗЕЛ - ПАРОЛЬ
                if (attribut.Name.ToUpper() == "PASSWORD")
                {
                    this.Password = attribut.InnerText;
                }

                //УЗЕЛ - ШТРИХКОД
                if (attribut.Name.ToUpper() == "BARCODE")
                {
                    this.Barcode = attribut.InnerText;
                }

                //УЗЕЛ - ID
                if (attribut.Name.ToUpper() == "ID")
                {
                    this.ID = attribut.InnerText;
                }

                //УЗЕЛ - УРОВЕНЬ ДОСТУПА (0- наладчик, 1 мастер)
                if (attribut.Name.ToUpper() == "ACCESSLEVEL")
                {
                    if (attribut.InnerText == "0")
                    {
                        this.AccesLevel = AccesLevels.Наладчик;
                    }

                    if (attribut.InnerText == "1")
                    {
                        this.AccesLevel = AccesLevels.Мастер;
                    }
                }
            }
        }

        /// <summary>
        /// Метод для обновления свойств MVVM
        /// </summary>
        /// <returns></returns>
        public void UpdateProperties()
        {
            OnPropertyChanged("Name");
            OnPropertyChanged("Access");
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
