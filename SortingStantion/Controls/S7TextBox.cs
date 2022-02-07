using SortingStantion.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SortingStantion.Controls
{
    public class S7TextBox : TextBox, INotifyPropertyChanged
    {
        /// <summary>
        /// Указатель на оригинальный 
        /// контролл
        /// </summary>
        TextBox originalTextBox;

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
        /// Максимально допустимая величина
        /// значения
        /// </summary>
        public int HightValue
        {
            get
            {
                return hightValue;
            }
            set
            {
                hightValue = value;
            }
        }
        int hightValue = 32767;

        /// <summary>
        /// Минимально допустимая величина
        /// значения
        /// </summary>
        public int LowValue
        {
            get
            {
                return lowValue;
            }
            set
            {
                lowValue = value;
            }
        }
        int lowValue = 0;

        /// <summary>
        /// Имя параметра (для записи в базу данных)
        /// </summary>
        public string ParametrName
        {
            get;
            set;
        }

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

                //Если сервер не проинициализирован - инициализируем его
                if (DataBridge.S7Server == null)
                {
                    DataBridge.CreateSimaticClient();
                }

                S7TAG = DataBridge.S7Server.Devices[0].GetTagByAddress(address);

                //Если регистр не найден
                if (S7TAG == null)
                {
                    return;
                }

                //Подписка на изменение значение тэга
                S7TAG.ChangeValue += S7TAG_ChangeValue;

                //Подписка на возобновление соединения с ПЛК
                S7TAG.device.GotConnection += Device_GotConnection;

            }
        }
        string address;

        /// <summary>
        /// Флаг, разрешающий ведение лога
        /// </summary>
        bool IsLogging;

        /// <summary>
        /// Экземпляр регистра, связанного с полем
        /// </summary>
        public S7Communication.simaticTagBase S7TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Тип переменной
        /// </summary>
        Type Type;

        string text;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public S7TextBox()
        {
            //Передача контекста данных
            DataContext = this;

            //Подписка на событие по загрузке контролла
            this.Loaded += MbTextBox_Loaded;

            //Подписка на изменение пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += Users_ChangeUser;

            //Проверка на соответствие уровню
            //доступа элемента управления
            CheckAccesLevel();
        }

        /// <summary>
        /// Метод для поиска родителя, реализующего
        /// интерфейс IMbScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Отписка от события по загрузке контролла
            this.Loaded -= MbTextBox_Loaded;

            //Получение оригинального текстбокса
            this.originalTextBox = (TextBox)e.OriginalSource;

            //Подписка на события
            //this.LostKeyboardFocus += MbTextBox_LostKeyboardFocus;
            this.KeyDown += MbTextBox_KeyDown;
            //this.GotKeyboardFocus += MbTextBox_GotKeyboardFocus;
            this.TextChanged += MbTextBox_TextChanged;
            this.GotFocus += S7TextBox_GotFocus;
            this.LostFocus += S7TextBox_LostFocus;
        }



        /// <summary>
        /// Метод, вызываемый при изменении текста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (S7TAG == null)
            {
                return;
            }

            //Ограничение ввода символов 
            var os = (TextBox)e.OriginalSource;
        }

        /// <summary>
        /// Обработчик события по изминению
        /// состояния тэга
        /// </summary>
        /// <param name="obj"></param>
        private void S7TAG_ChangeValue(object oldvalue, object newvalue)
        {
            SetText(S7TAG.StatusText);
        }

        /// <summary>
        /// Метод, вызываемый при получении логического фокуса
        /// элементом управления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void S7TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Открытие клавиатуры
            Keypad keypadWindow = new Keypad();

            //Если нажата кнопка Enter - 
            //запись значения в поле
            if (keypadWindow.ShowDialog() == true)
            {
                this.originalTextBox.Text = keypadWindow.Result;
                var result = WriteValue(this.originalTextBox.Text);

                //Если результат записи данных
                //в плк успешен - покидаем функцию
                if (result == true)
                {
                    deFocus();
                    return;
                }

            }

            //Записываем в контролл значение
            //из регистра
            M0: SetText(S7TAG.StatusText);
            deFocus();
        }

        /// <summary>
        /// Метод, вызываемый при утрате 
        /// фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void S7TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (S7TAG == null)
            {
                return;
            }

            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Возвращаем подписку на изменение регистра
            S7TAG.ChangeValue += S7TAG_ChangeValue;
        }

        /// <summary>
        /// При потере фокуса клавиатурой
        /// начинаем обновлять значения в текстоксе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (S7TAG == null)
            {
                return;
            }

            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Возвращаем текст в контроле
            originalsourse.Text = text;

            //Возвращаем подписку на изменение регистра
            S7TAG.ChangeValue += S7TAG_ChangeValue;
        }

        /// <summary>
        /// Запись в регистр значения по 
        /// нажатию на кнопку Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ////Получение оригинального источника
            //TextBox originalsourse = (TextBox)e.OriginalSource;
            //this.originalTextBox = originalsourse;

            ////При нажатии на кнопку ввода
            ////записываем значение в регистр
            //if (e.Key == Key.Enter)
            //{
            //    WriteValue(this.originalTextBox.Text);
            //    return;
            //}

            ////При нажатии на кнопку эскейт
            ////возвращаем значение
            //if (e.Key == Key.Escape)
            //{
            //    //S7TAG_ChangeValue(true);
            //    //DataBridge.FocusElement.Focus();
            //}
        }

        /// <summary>
        /// Метод для записи значения
        /// </summary>
        /// <param name="value"></param>
        bool WriteValue(string value)
        {
            //Проверка на лимиты
            var result = CheckLimits(value);
            if (result == false)
            {
                deFocus();
                return false;
            }

            this.Text = value;

            //Если регистра нет, пишем по умолчанию ***
            if (S7TAG == null)
            {
                SetText("***");
                deFocus();
                return false;
            }

            //Запоинаем текст в контроле
            if ((bool)S7TAG?.Write(this.Text))
            {
                text = Text;
                deFocus();

                //Записываем в базу данных информацию об изминении
                DataBridge.AlarmLogging.AddMessage($"Значение параметра: {ParametrName} изменено на {this.Text}", MessageType.Event);

                //Возвращаем положительный результат
                //выполнения операции записи в ПЛК
                return true;
            }

            //Возврат отрицательного результата
            return false;
        }

        /// <summary>
        ///  Метод для проверки значений для записи
        /// </summary>
        /// <returns></returns>
        bool CheckLimits(string tValue)
        {
            //Преобразуем введеное текстовое значение
            //к типу uint
            uint value = 0;
            var resultconvertion = uint.TryParse(tValue, out value);

            //Если ввседеное значение не удается преобразовать в число
            //возвращаем отрицательный результат
            if (resultconvertion == false)
            {
                return false;
            }

            //Если Введеное значение ниже нижнего предела
            if (value < LowValue)
            {
                return false;
            }

            //Если Введеное значение выше верхнего предела
            if (value > HightValue)
            {
                return false;
            }

            //Если все проверки пройдены, 
            //возвращаем положительный результат проверки
            return true;
        }

        /// <summary>
        /// Метод для перемещения фокуса 
        /// на другой элемент управления
        /// </summary>
        void deFocus()
        {
            //Перевод фокуса на другой элемент
            var parent = (UIElement)this.Parent;
            parent.Focusable = true;
            parent.Focus();
        }


        /// <summary>
        /// Метод для установки текста в контролле
        /// </summary>
        void SetText(string text)
        {
            Action action = () =>
            {
                if (this.originalTextBox == null)
                {
                    this.Text = text;

                }
                else
                {
                    this.Text = text;
                    this.originalTextBox.Text = text;
                }
            };

            this.Dispatcher.Invoke(action);
        }


        /// <summary>
        /// Метод, вызываемый при возобновлении
        /// соединения с ПЛК
        /// </summary>
        private void Device_GotConnection()
        {
            Action action = () =>
            {
                //Преобразование к типу sting
                S7TAG.ObjectToString();

                this.Text = S7TAG.StatusText;
                //this.originalTextBox.Text = S7TAG.StatusText;
            };

            this.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// При получения фокуса ввода 
        /// с клавиатуры отписываемся
        /// от изминения текста по тэгу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbTextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //Получаем экземпляр TextBox
            TextBox originalsourse = (TextBox)e.OriginalSource;

            //Запоинаем текст в контроле
            text = originalsourse.Text;

            //Отписываемся от обновления 
            //значения тэга
            if (S7TAG != null)
            {
                S7TAG.ChangeValue -= S7TAG_ChangeValue;
            }
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

        #region РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
