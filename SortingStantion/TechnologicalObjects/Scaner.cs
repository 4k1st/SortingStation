using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SortingStantion.TechnologicalObjects
{

    public class Scaner : INotifyPropertyChanged
    {
        /// <summary>
        /// Флаг, указывающий на то, что
        /// сканер доступен
        /// </summary>
        bool isAvailable = false;
        public bool IsAvailable
        {
            get 
            {
                return isAvailable;
            }
            set
            {
                isAvailable = value;
                OnPropertyChanged("IsAvailable");
            }
        }

        /// <summary>
        /// Глобальный файл настроек
        /// </summary>
        SettingsFile sf
        {
            get 
            {
                return DataBridge.SettingsFile;
            }
        }

        /// <summary>
        /// Порт, к которому подключен сканер
        /// </summary>
        SerialPort port;

        /// <summary>
        /// Событие, генерируемое при получении
        /// новых данных сканером
        /// </summary>
        public event Action<string> NewDataNotification;
        

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Scaner()
        {
            //Загрузка настроек из файла настроек
            Load();
            
            //Запуск порта
            Start();
        }

        /// <summary>
        /// Метод для загрузки настроек 
        /// порта
        /// </summary>
        public void Load()
        {
            //Если порт был открыт - закрываем его
            if (port != null)
            {
                port.Close();
            }

            //Инициализация порта, к которому
            //подключен сканер
            port = new SerialPort();
            port.PortName = sf.GetValue("SerialPort232Name");
            port.BaudRate = int.Parse(sf.GetValue("SerialPort232BaudRate"));
            port.DataBits = int.Parse(sf.GetValue("SerialPort232DataBits"));
            port.Parity = ToParity(sf.GetValue("SerialPort232StopBits"));
            port.StopBits = ToStopBits(sf.GetValue("SerialPort232StopBits"));
            port.ReadTimeout = 1000;
        }

        /// <summary>
        /// Метод для загрузки настроек 
        /// порта
        /// </summary>
        /// <param name="portname"></param>
        public void Load(string portname)
        {
            //Если порт был открыт - закрываем его
            if (port != null)
            {
                //Отписка от приема сообщений из последовательного порта
                port.DataReceived -= Port_DataReceived;

                //Закрытие порта
                port.Close();
            }

            //Инициализация порта, к которому
            //подключен сканер
            port = new SerialPort();
            port.PortName = portname;
            port.BaudRate = int.Parse(sf.GetValue("SerialPort232BaudRate"));
            port.DataBits = int.Parse(sf.GetValue("SerialPort232DataBits"));
            port.Parity = ToParity(sf.GetValue("SerialPort232StopBits"));
            port.StopBits = ToStopBits(sf.GetValue("SerialPort232StopBits"));
            port.ReadTimeout = 1000;
        }

        /// <summary>
        /// Метод для приведение строки
        /// к типу Parity
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Parity ToParity(string value)
        {

            if (value.ToUpper() == "ODD")
            {
                return Parity.Odd;
            }

            if (value.ToUpper() == "EVEN")
            {
                return Parity.Even;
            }

            if (value.ToUpper() == "MARK")
            {
                return Parity.Mark;
            }

            if (value.ToUpper() == "SPACE")
            {
                return Parity.Space;
            }

            return Parity.None;
        }

        /// <summary>
        /// Метод для приведение строки
        /// к типу StopBits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        StopBits ToStopBits(string value)
        {
            if (value.ToUpper() == "NONE")
            {
                return StopBits.None;
            }

            if (value.ToUpper() == "ONEPOINTFIVE")
            {
                return StopBits.OnePointFive;
            }

            if (value.ToUpper() == "TWO")
            {
                return StopBits.Two;
            }

            return StopBits.One;
        }

        /// <summary>
        /// Метод для включения сканера в работу
        /// </summary>
        public void Start()
        {
            try
            {
                //Открываем порт
                port.Open();

                //Подписываемся на прием сообщения из последовательного порта
                port.DataReceived += Port_DataReceived;

                //Указываем, что сканер недоступен
                IsAvailable = true;
            }
            catch(Exception ex)
            {
                //Выводим сообщение от ошибке
                customMessageBox msg = new customMessageBox("Ошибка инициализации ручного сканера", $"При инициализации последовательного порта, к которому подключен ручной сканер возникло исключение:" +
                                                            $"{ex.Message}. Ручной сканер работать не будет");
                
                //Указываем, что сканер недоступен
                IsAvailable = false;

                //Показываем сообщение
                msg.Show();
            }
        }

        /// <summary>
        /// Событие, вызываемое при 
        /// получении новых данных от сканера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Отписка от события по приему данных 
            //из SerialPort
            port.DataReceived -= Port_DataReceived;

            //Технологическая пауза
           Thread.Sleep(100);

            //Буфер для чтения данных их последовательноо порта
            byte[] buffer = new byte[512];

            //Чтение данных из последовательного порта
            var lenght = port.Read(buffer, 0, buffer.Length);

            string data = Encoding.ASCII.GetString(buffer);

            //Уведомление подписчиков из потока UI
            Action action = () =>
            {
                //Уведомление подписчиков о новых данных
                NewDataNotification?.Invoke(data);
            };
            DataBridge.MainScreen.Dispatcher.Invoke(action);

            //Подписка на событие по приему данных 
            //из SerialPort
            port.DataReceived += Port_DataReceived;
        }

        /// <summary>
        /// Метод для завершения работыт сканера
        /// </summary>
        public void Stop()
        {
            //Если порт не инициализирован,
            //выходим
            if (port == null)
            {
                return;
            }
            
            // Отписка от события по приему данных
            //из SerialPort
            port.DataReceived -= Port_DataReceived;

            //Закрытие порта
            port.Close();

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
