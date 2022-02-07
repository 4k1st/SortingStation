using SortingStantion.Models;
using System;
using System.IO.Ports;
using System.Windows.Controls;

namespace SortingStantion.Controls
{
    /// <summary>
    /// Логика взаимодействия для ComboBoxSerialPorts.xaml
    /// </summary>
    public partial class ComboBoxSerialPorts : UserControl
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
        /// Настройка
        /// </summary>
        Setting setting;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ComboBoxSerialPorts()
        {
            //Инициализация UI
            InitializeComponent();

            //Получение текущего имени порта
            setting = DataBridge.SettingsFile.GetSetting("SerialPort232Name");

            //Загрузка портов
            var ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                var portname = ports[i];
                combobox.Items.Add(ports[i]);

                if (portname == ports[i])
                {
                    combobox.SelectedIndex = i;
                }
            }

            //Подпись на события
            combobox.DropDownOpened += ComboBox_DropDownOpened;
            combobox.SelectionChanged += combobox_SelectionChanged;

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

        /// <summary>
        /// Метод, вызываемый при открытии
        /// комбобокса. При открытии происходит
        /// 
        /// загрузка доступных портов в список
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var ports = SerialPort.GetPortNames();
            combobox.Items.Clear();
            foreach (var port in ports)
            {
                combobox.Items.Add(port);
            }
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox.SelectedItem == null)
            {
                return;
            }

            //Сохранение новой настройки в файл конфигурации
            var newvalue = combobox.SelectedItem.ToString();
            setting.SetValue(newvalue);

            //Смена имени порта, к которому подключен
            //ручной сканер
            DataBridge.Scaner.Load(newvalue);
            DataBridge.Scaner.Start();

            //Записываем в базу данных информацию об изминении
            DataBridge.AlarmLogging.AddMessage($"Изменен порт ручного сканера на : '{newvalue}'", MessageType.Event);
        }
    }
}
