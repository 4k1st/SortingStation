using System;

namespace SortingStantion.Models
{
    /// <summary>
    /// класс, описывающий структуру элемента 
    /// истории авторизации пользователей
    /// </summary>
    public class UserAuthorizationHistotyItem
    {
        /// <summary>
        /// Время входа в логин
        /// </summary>
        public string startTime
        {
            get;
            set;
        }

        /// <summary>
        /// Время выхода из логина
        /// </summary>
        public string endTime
        {
            get;
            set;
        }

        /// <summary>
        /// Указатель на пользователя
        /// </summary>
        public string id
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public UserAuthorizationHistotyItem()
        {
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public UserAuthorizationHistotyItem(User user)
        {
            this.id = user.ID;
            startTime = DateTime.Now.GetDateTimeFormats()[43];
        }


    }
}
