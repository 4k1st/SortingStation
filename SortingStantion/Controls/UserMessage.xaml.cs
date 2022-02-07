using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Типы сообщений
    /// </summary>
    public enum MSGTYPE
    {
        ERROR = 3,
        WARNING = 2,
        SUCCES = 1,
        INFO = 0,

    }

    /// <summary>
    /// Логика взаимодействия для MSG.xaml
    /// </summary>
    public partial class UserMessage : INotifyPropertyChanged
    {

        /// <summary>
        /// Текст сообщения
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
        /// Время возникновления сообщения
        /// </summary>
        string datetime = string.Empty;
        public string Datetime
        {
            get
            {
                return datetime;
            }
            set
            {
                datetime = value;
                OnPropertyChanged("Datetime");
            }
        }


        public Brush Brush
        {
            get;
            set;
        }

        /// <summary>
        /// Тип сообщения
        /// </summary>
        MSGTYPE _type = MSGTYPE.ERROR;
        public MSGTYPE Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name=""></param>
        public UserMessage(string message, MSGTYPE type)
        {
            InitializeComponent();

            DataContext = this;

            //Инициализация полей
            Message = message;
            Type = type;
            Datetime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

            //Определение цвета окна
            Brush = MSGTYPE_TO_BRUSH(type);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public UserMessage(string message, Brush color)
        {
            InitializeComponent();

            DataContext = this;

            //Инициализация полей
            Message = message;
            Type = MSGTYPE.WARNING;
            Datetime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

            //Определение цвета окна
            Brush = color;
        }

        /// <summary>
        /// Перевод типа сообщения в цвет кисти
        /// </summary>
        /// <returns></returns>
        SolidColorBrush MSGTYPE_TO_BRUSH(MSGTYPE type)
        {
            if (type == MSGTYPE.WARNING)
            {
                return DataBridge.myOrange;
            }

            if (type == MSGTYPE.SUCCES)
            {
                //Зеленый цвет
                return DataBridge.myGreen;
            }

            if (type == MSGTYPE.ERROR)
            {
                return DataBridge.myRed;
            }

            if (type == MSGTYPE.INFO)
            {
                return  DataBridge.myBlue;
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Команда для удаления текущего контрола
        /// </summary>
        public ICommand DeleteCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                   DataBridge.MSGBOX.Remove(this);
                },
                (obj) => (true));
            }
        }


        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
        #endregion
    }
}
