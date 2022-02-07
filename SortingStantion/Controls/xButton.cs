using SortingStantion.Models;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Класс, описывающий кнопку с 
    /// разграничением прав пользователей
    /// </summary>
    public class xButton : Button
    {
        /// <summary>
        /// Минимальный уровень доступа
        /// </summary>
        public int MinAccesLevel
        {
            get
            {
                return minAccesLevel;
            }
            set
            {
                minAccesLevel = value;
                CheckAccesLevel();
            }
        }
        int minAccesLevel = 1;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public xButton()
        {
            //Подписка на изменение пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += Users_ChangeUser;

            //Проверка на соответствие уровню
            //доступа элемента управления
            CheckAccesLevel();
        }

        /// <summary>
        /// Собылие при изменении пользователя
        /// </summary>
        /// <param name="accesslevel"></param>
        private void Users_ChangeUser(int accesslevel, User newuser, bool Archive)
        {
            CheckAccesLevel();
        }

        /// <summary>
        /// Проверка на доступность управления
        /// в зависимости от текущего уровня пользователя
        /// </summary>
        /// <returns></returns>
        void CheckAccesLevel()
        {
            //Если текущий пользователь не авторизован
            //полагаем, что уровень доступа 0
            int currentlevel = 0;

            if (DataBridge.MainAccesLevelModel.CurrentUser != null)
            {
                currentlevel = (int)DataBridge.MainAccesLevelModel.CurrentUser.AccesLevel;
            }

            //Установка свойства в зависимости от уровня доступа
            this.IsEnabled = currentlevel >= MinAccesLevel;
        }

    }
}
