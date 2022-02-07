using SortingStantion.Controls;
using SortingStantion.ToolsWindows.windiwInformation;
using SortingStantion.ToolsWindows.windowAddDeffect;
using SortingStantion.ToolsWindows.windowClearCollectionRequest;
using SortingStantion.Utilites;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SortingStantion.Models
{
    /// <summary>
    /// Движок для управления экранами
    /// </summary>
    public class ScreenEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// Главный экран
        /// </summary>
        frameMain.frameMain frameMain = new SortingStantion.frameMain.frameMain();

        /// <summary>
        /// Экран настроек
        /// </summary>
        frameSettings.frameSettings frameSettings = new SortingStantion.frameSettings.frameSettings();

        /// <summary>
        /// Событие, генерируемое при смене экрана
        /// </summary>
        public event Action<UserControl> ChangeScreenNotification;

        /// <summary>
        /// Текущий экран
        /// </summary>
        object currentScreen;
        public object CurrentScreen
        {
            get
            {
                return currentScreen;
            }
            set
            {
                currentScreen = value;
                OnPropertyChanged("CurrentScreen");
            }
        }

        /// <summary>
        /// Акцессо для доступа к модели управления
        /// доступом к кнопкам приложения
        /// </summary>
        public ButtonsEnableModel ButtonsEnableModel
        {
            get
            {
                return DataBridge.ButtonsEnableModel;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ScreenEngine()
        {
            CurrentScreen = frameMain;
        }

        /// <summary>
        /// Команда открытия главного
        /// экрана
        /// </summary>
        public ICommand OpenMainScreenCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentScreen = frameMain;
                    ChangeScreenNotification?.Invoke(frameMain);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда открытия экрана
        /// настроек
        /// </summary>
        public ICommand OpenSettingsScreenCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //Вызываем окно авторизации
                    SortingStantion.UserAdmin.frameAuthorization frameAuthorization = new SortingStantion.UserAdmin.frameAuthorization();
                    frameAuthorization.ShowDialog();

                    //Если результат авторизации не удачный, выходим
                    if (frameAuthorization.AuthorizationResult == false)
                    {
                        return;
                    }


                    CurrentScreen = frameSettings;
                    ChangeScreenNotification?.Invoke(frameSettings);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для выключения комплекса
        /// </summary>
        public ICommand ShutdownCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Action action = () =>
                    {
                        //Проверка - работает ли линия
                        if (DataBridge.Conveyor.LineIsRun == true)
                        {
                            customMessageBox mb = new customMessageBox("Ошибка", "Линия в работе, перед завершением остановите конвейер.");
                            mb.Owner = DataBridge.MainScreen;
                            mb.ShowDialog();

                            return;
                        }

                        frameSettings.windowExit windowExit = new frameSettings.windowExit();
                        windowExit.Owner = DataBridge.MainScreen;
                        windowExit.ShowDialog();
                    };

                    windowClearCollectionRequest windowClearCollectionRequest = new windowClearCollectionRequest(action);
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для добавления нового короба
        /// (при нажатии на клавишу Добавить)
        /// </summary>
        public ICommand AddingBoxCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    windiwAddingBox.windiwAddingBox windiwAddingBox = new windiwAddingBox.windiwAddingBox();
                    windiwAddingBox.Owner = DataBridge.MainScreen;
                    windiwAddingBox.ShowDialog();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для открытия окна ДОБАВТЬ ДЕФФЕКТ
        /// </summary>
        public ICommand AddingDeffectCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    windowAddDeffect windowAddDeffect = new windowAddDeffect();
                    windowAddDeffect.Owner = DataBridge.MainScreen;
                    windowAddDeffect.ShowDialog();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Команда для открытия окна СПРАВКА
        /// </summary>
        public ICommand OpenInformationCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    windiwInformation windiwInformation = new windiwInformation();
                    windiwInformation.Owner = DataBridge.MainScreen;
                    windiwInformation.ShowDialog();
                },
                (obj) => (true));
            }
        }

        /// <summary>
        /// Метод для перехода 
        /// </summary>
        public void GoToMainWindow()
        {
            //Если открыта клавиатура - закрываем её
            if (DataBridge.keypad != null)
            {
                DataBridge.keypad.Close();
            }

            //Переходим на главный экран
            CurrentScreen = frameMain;
            ChangeScreenNotification?.Invoke(frameMain);
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
