using S7Communication;
using SortingStantion.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    public class S7ToggleButton : Button, INotifyPropertyChanged
    {
        /// <summary>
        /// Состояние контролла
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        bool? isChecked = false;

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
        /// Адрес регистра
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;

                S7TAG = (S7_Boolean)DataBridge.S7Server.Devices[0].GetTagByAddress(address);

                //Если регистр не найден
                if (S7TAG == null)
                {
                    return;
                }

                //Подписка на изменение значение тэга
                S7TAG.ChangeValue += S7TAG_ChangeValue;

                //Подписка на возобновление соединения с ПЛК
                //S7TAG.device.GotConnection += Device_GotConnection;
            }
        }
        string address = "Не задан";

        /// <summary>
        /// Поле - связанный бит
        /// </summary>
        public S7_Boolean S7TAG
        {
            get;
            set;
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public S7ToggleButton()
        {
            //Установка контекста данных
            DataContext = this;

            //Подписка на событие по клику
            this.Click += S7ToggleButton_Checked;

            //Подписка на изменение текущего пользователя
            //для установки доступа к контролу
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
        /// Метод, вызываемый при попытке изменить
        /// состояние регистра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void S7ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Инвертируем значение, которое необходимо записать
            bool State = !(bool)IsChecked;

            //Если экзкмпляр бита не 
            //пустой объект
            if (S7TAG != null)
            {
                var result = S7TAG.Write(State);
            }
        }

        /// <summary>
        /// Mеняем состояние toggleButton при изменении
        /// состояния бита обратной связи
        /// </summary>
        /// <param name="obj"></param>
        private void S7TAG_ChangeValue(object oldvalue, object newvalue)
        {
            bool? value = (bool?)newvalue;

            Action action = () =>
            {
                this.Click -= S7ToggleButton_Checked;
                this.IsChecked = value;
                this.Click += S7ToggleButton_Checked;
            };
            this.Dispatcher.Invoke(action);
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
