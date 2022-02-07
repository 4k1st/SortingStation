using SortingStantion.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace SortingStantion.Models
{

    /// <summary>
    /// Класс, осуществляющий управление 
    /// сообщениями в информационном окне
    /// </summary>
    public class Message_Engine : INotifyPropertyChanged
    {
        /// <summary>
        /// Экземпляр контейнера, в котором будут
        /// хранится сообщения
        /// </summary>
        public object _CurrentMessage
        {
            get
            {
                return currentMessage;
            }
            set
            {
                currentMessage = value;
                DataBridge.UserMessagePresenter.Content = value;
            }
        }
        object currentMessage;

        /// <summary>
        /// Коллекция всех сообщений
        /// </summary>
        public List<UserMessage> MessageList = new List<UserMessage>();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Message_Engine()
        {
        }        

        /// <summary>
        /// метод для добавления сообщения в контейнер
        /// </summary>
        /// <param name="msg"></param>
        public void Add(UserMessage msg)
        {
            // Если новое сообщение ошибка
            if (msg.Type == MSGTYPE.ERROR)
            {
                _CurrentMessage = msg;
                goto M0;
            }

            if (_CurrentMessage == null)
            {
                _CurrentMessage = msg;
                goto M0;
            }

            if (_CurrentMessage is UserMessage == false)
            {
                return;
            }

            var cm = (UserMessage)_CurrentMessage;

            //Если текущее сообщение не ошибка
            //ПРИОРИТЕТ СООБЩЕНИЯ ОБ ОШИБКИ
            if (cm.Type != MSGTYPE.ERROR)
            {
                _CurrentMessage = msg;
            }

        //Добавляем сообщение в коллекцию
        M0: MessageList.Add(msg);

            //Если сообщений больше 3
            //удаляем самое первое
            if (MessageList.Count > 3)
            {
                MessageList.RemoveAt(0);
            }
        }


        /// <summary>
        /// Метод для удаления сообщения из 
        /// контейнера
        /// </summary>
        /// <param name="msg"></param>
        public void Remove(UserMessage msg)
        {
            //Если указателя на msg нет
            //выходим из функции
            if (msg == null)
            {
                return;
            }


            //Удаляем сообщение из списка
            MessageList.Remove(msg);

            //Если в истории сообщений есть сообщения
            //выводим их в контейнер
            if (MessageList.Count > 0)
            {
                _CurrentMessage = MessageList[MessageList.Count - 1];
            }
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
    }
}
