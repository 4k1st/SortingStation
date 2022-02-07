using S7Communication;
using SortingStantion.Controls;
using SortingStantion.Models;
using System;
using SortingStantion.ToolsWindows.windowGtinFault;
using SortingStantion.ToolsWindows.windowExtraneousBarcode;
using SortingStantion.ToolsWindows.windowProductIsDeffect;
using SortingStantion.ToolsWindows.windowRepeatProduct;
using System.Windows.Input;
using SortingStantion.Utilites;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace SortingStantion.TechnologicalObjects
{
    /// <summary>
    /// Объяект, осуществляющий работу с продуктами, их учет
    /// сравнение для отбраковки
    /// </summary>
    public class ProductsEngine
    {
        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        public SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        public SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        /// <summary>
        /// Сигнал для перемещения данных
        /// просканированного изделия в коллекцию
        /// </summary>
        //S7_Boolean READ_CMD;

        /// <summary>
        /// Тэг для очистки очереди в ПЛК
        /// </summary>
        S7_Boolean CLEAR_ITEMS_COLLECTION_CMD;

        /// <summary>
        /// Результат сканирования
        /// </summary>
        //S7_String SCAN_DATA;

        /// <summary>
        /// Тэг готовности ПК к приему штрих-кода от ПЛК
        /// </summary>
        public S7_Boolean ReadyForTransfer
        {
            get;
            set;
        }


        /// <summary>
        /// Тэг - разрешить повтор кода продукта
        /// </summary>
        S7_Boolean REPEAT_ENABLE;

        /// <summary>
        /// Количество отсканированных, но не выпущенных
        /// продуктов
        /// </summary>
        public S7_DWord ProductCollectionLenght;

        /// <summary>
        /// Счетчик повтора кодов
        /// </summary>
        S7_DWord RepeatProductCounter;

        /// <summary>
        /// Объект, осуществляющий разбор телеграммы
        /// сканированного штрихкода
        /// </summary>
        DataSpliter spliter = new DataSpliter();

        /// <summary>
        /// Последний код, принятый от ПЛК
        /// </summary>
        string LastBarcode;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ProductsEngine()
        {
            //Инициализация сигналов от сканера
            REPEAT_ENABLE = (S7_Boolean)device.GetTagByAddress("DB1.DBX134.0");

            //var fastdevice = DataBridge.S7Server.Devices[1];
            //var fastgroup = fastdevice.Groups[0];

            ////Команда для считывания кода сканера
            //READ_CMD = new S7_Boolean("", "DB1.DBX378.0", fastgroup);

            //Данные из сканера
            //SCAN_DATA = new S7_String("", "DB1.DBD506-STR100", fastgroup);

            //Тэг, указывающий о готовности ПК приянтия данных от ПЛК
            ReadyForTransfer = new S7_Boolean("", "DB1.DBX98.6", group);

            //Тэг для очистки коллекции изделий
            CLEAR_ITEMS_COLLECTION_CMD = (S7_Boolean)device.GetTagByAddress("DB1.DBX98.2");

            //Количество отсканированных но не выпущенных объектов
            ProductCollectionLenght = (S7_DWord)device.GetTagByAddress("DB5.DBD0-DWORD");

            //Счетчик повторов
            RepeatProductCounter = (S7_DWord)device.GetTagByAddress("DB1.DBD36-DWORD");

            //Подписываемся на событие по изминению
            //тэга READ_CMD  и осуществляем вызов
            //метода в потоке UI
            //fastdevice.DataUpdated += () =>
            //{
            //    if (READ_CMD.Value == false)
            //    {
            //        return;
            //    }

            //    SCAN_DATA_DataUpdated(null);
            //};

            //READ_CMD.DataUpdated += (ov) =>
            //{
            //    if (READ_CMD.Value == false)
            //    {
            //        return;
            //    }

            //    SCAN_DATA_DataUpdated(null);

            //    ////В случае, если ппроисходит 
            //    ////сброс тэга - код не выполняем
            //    //SCAN_DATA.DataUpdated += SCAN_DATA_DataUpdated;
            //};

            //При первом скане очищаем коллекцию
            //продуктов в очереди ПЛК
            device.FirstScan += () =>
            {
                //Очистка коллекции продуктов в очереди ПЛК
                CLEAR_ITEMS_COLLECTION_CMD.Write(true);
            };

            //Запуск приема кодов по UDP
            Thread backgroundTask_UDP_listener = new Thread(UDP_Listener);
            backgroundTask_UDP_listener.IsBackground = true;
            backgroundTask_UDP_listener.Start();

            //Запуск приема кодов по TCP
            Thread backgroundTask_TCP_listener = new Thread(TCP_Listener);
            backgroundTask_TCP_listener.IsBackground = true;
            backgroundTask_TCP_listener.Start();


        }

        /// <summary>
        /// ID продукта, полученный от ПЛК
        /// </summary>
        UInt32 RecvID = 0;

        /// <summary>
        /// Метод для принятия кодов
        /// </summary>
        public void UDP_Listener()
        {
            // Создаем UdpClient для чтения входящих данных
            UdpClient receivingUdpClient = new UdpClient(2000);

            //Получение адреса ПЛК из файла настроек
            var ipsetting = DataBridge.SettingsFile.GetSetting("PlcIp").Value;
            var ip = IPAddress.Parse(ipsetting);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(ip, 102);



            try
            { 
                while (true)
                {

                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    //Уведомление ПЛК о получении данных 
                    SendASK();

                    // Преобразуем и отображаем данные
                    var data = Encoding.UTF8.GetString(receiveBytes);
                    var barcode = data.Substring(42);
                    RecvID = GetID(data); 


                    Action action = () =>
                    {
                        if (LastBarcode != data)
                        {
                            NEW_BARCODE_NOTIFICATION(barcode);
                        }
                    };
                    DataBridge.MainScreen?.Dispatcher.Invoke(action);


                    LastBarcode = data;


                }
            }
            catch (Exception ex)
            {
                Logger.AddExeption("ProductsEngine.cs, Method=UDP_Listener", ex);
            }
        }

        /// <summary>
        /// Отправка байта подтверждения в ПЛК
        /// </summary>
        private void SendASK()
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient(2002);

            //Получение адреса ПЛК из файла настроек
            var ipsetting = DataBridge.SettingsFile.GetSetting("PlcIp").Value;
            var ip = IPAddress.Parse(ipsetting);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(ip, 2002);

            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = BitConverter.GetBytes(RecvID);

                //Переворачивание байтов, для симатика
                byte[] rotareBytes = new byte[]
                {
                    bytes[3],
                    bytes[2],
                    bytes[1],
                    bytes[0]
                };

                // Отправляем данные
                sender.Send(rotareBytes, rotareBytes.Length, RemoteIpEndPoint);
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }

        /// <summary>
        /// Получение ID сообщения
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        UInt32 GetID(string input)
        {
            var numbers = Regex.Split(input, @"\D+").Where(s => s != "").ToArray();
            return UInt32.Parse(numbers[0]);
        }

        public void TCP_Listener()
        {
            TcpListener tcpListener = null;

            try
            {
                //Инициализация прослушивателя TCP
                tcpListener = new TcpListener(IPAddress.Any, 2000);

                //Запуск прослушивателя TCP
                tcpListener.Start();

                //Цикл
                while (true)
                {
                    //Получение подключившегося клиента
                    TcpClient client = tcpListener.AcceptTcpClient();

                    //Получение потока вывода
                    NetworkStream ns = client.GetStream();
                    if (client.ReceiveBufferSize > 0)
                    {
                        //Чтение данных из потока вывода
                        var bytes = new byte[client.ReceiveBufferSize];
                        var lenght = ns.Read(bytes, 0, client.ReceiveBufferSize);

                        var data = Encoding.Default.GetString(bytes);
                        //data = ToASCII(data);

                        var barcode = data.Substring(39);

                        //Обработка данных
                        Action action = () =>
                        {
                            NEW_BARCODE_NOTIFICATION(barcode);
                        };
                        DataBridge.MainScreen?.Dispatcher.Invoke(action);
                    }

                    //Завершение сессии с клиентом
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.AddExeption("ProductsEngine.cs, Method=TCP_Listener", ex);
            }
        }

        /// <summary>
        /// Если английские символы
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool IsASCII(char symbol)
        {
            if (symbol > 255)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Метод для получения из стрики 
        /// символов ASCII
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string ToASCII(string input)
        {
            string result = string.Empty;
            var symbols = input.ToCharArray();

            foreach (var symbol in symbols)
            {
                var ascii = IsASCII(symbol);
                if (ascii == true)
                {
                    result += symbol;
                }
            }

            return result;
        }


        /// <summary>
        /// Метод, вызываемый при обновлении массива данных
        /// от сканера
        /// </summary>
        /// <param name="obj"></param>
        private void SCAN_DATA_DataUpdated(object obj)
        {
            //В случае, если ппроисходит 
            //сброс тэга - код не выполняем
            Action action = () =>
            {
                BARCODESCANER_CHANGEVALUE(null, null);
                //SCAN_DATA.DataUpdated -= SCAN_DATA_DataUpdated;
            };
            DataBridge.MainScreen.Dispatcher.Invoke(action);
        }

        private void NEW_BARCODE_NOTIFICATION(string barcode)
        {

            var bcode = barcode;
            bcode = bcode.Replace("\0", "");

            //Получение статуса линии
            //с учетом таймера, учитывающего время
            //остановки линии
            var lineistop = false;

            lineistop = (bool)DataBridge.Conveyor.IsStopFromTimerTag.Value;

            //Получение GTIN из задания
            var task_gtin = DataBridge.WorkAssignmentEngine.GTIN;

            //Разбор телеграммы из ПЛК
            //var data = SCAN_DATA.StatusText;
            spliter.Split(ref bcode);

            //Получение GTIN и SerialNumber и криптохвоста из разобранного
            //штрихкода, полученного из ПЛК
            //искуственно добавляем криптохвост к серийнику через пробел

            var scaner_serialnumber = String.Concat("21", spliter.SerialNumber, "\u001d93", spliter.Crypto);
            //var scaner_serialnumber = spliter.SerialNumber;
            var scaner_gtin = spliter.GTIN;




            /*
                Если сигнатура задачи не совпадает
                с той, что указана в задании
            */
            if (spliter.IsValid == false)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"От автоматического сканера получен посторонний код: {spliter.SourseData}  (код не является СИ)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний код (не является КМ)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowExtraneousBarcode(msg);
                windowExtraneousBarcode.Show();

                //Выход из метода
                return;
            }

            /*
                Если GTIN из сканера и задачи
                не совпадают - формируем ошибку
            */
            if (scaner_gtin != task_gtin)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Посторонний продукт (GTIN не совпадает с заданием)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний продукт (GTIN не совпадает с заданием)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowGtinFault(scaner_gtin, scaner_serialnumber, msg);
                windowExtraneousBarcode.Show();

                //Выход из метода
                return;
            }

            /*
                Если продукт числится в браке
            */
            if (DataBridge.Report.IsDeffect(scaner_serialnumber) == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Номер продукта {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} числится в браке", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в окно информации
                string message = $"Номер продукта {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} числится в браке";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowProductIsDeffect = new windowProductIsDeffect(scaner_serialnumber, msg);
                windowProductIsDeffect.Show();

                //Выход из метода
                return;
            }


            /*
                Повтор кода запрещен
            */
            //Получение статуса тэга
            //ПОВТОР КОДА
            //остановки линии
            var RepeatEnable = REPEAT_ENABLE.Value;

            //Флаг, указывающий на то, является ли код повтором
            var IsRepeat = DataBridge.Report.IsRepeat(scaner_serialnumber);


            if (RepeatEnable == false && IsRepeat == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Продукт GTIN {scaner_gtin} номер {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} считан повторно", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в окно информации
                string message = $"Продукт номер {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} считан повторно.";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Добавление повтора в отчет
                DataBridge.Report.AddRepeatProduct(scaner_serialnumber);

                //Вызов окна
                var windowRepeatProduct = new windowRepeatProduct(scaner_gtin, scaner_serialnumber, msg);
                windowRepeatProduct.Show();

                //Выход из метода
                return;
            }

            /*
               Если повтор - увеличиваем счетчик на единицу
           */
            //if (IsRepeat == true)
            //{
            //    uint value = 0;
            //    var result = uint.TryParse(RepeatProductCounter.Status.ToString(), out value);
            //    if (result == true)
            //    {
            //        value++;
            //        RepeatProductCounter.Write(value);
            //    }
            //}


            /*
                Если проверка прошла успешно добавляем 
                продукт в результат
            */

            //Добавляем просканированное изделие
            //в коллекцию изделий результата
            DataBridge.Report.AddBox(scaner_serialnumber);

        }

        /// <summary>
        /// Событие, вызываемое при изменении статуса GOODREAD или NOREAD
        /// </summary>
        /// <param name="svalue"></param>
        private void BARCODESCANER_CHANGEVALUE(object oldvalue, object newvalue)
        {
            //Стираем флаг READ_CMD
            //для того, чтоб процедура отработала один раз
            //READ_CMD.Write(false);

            //установка флага готовности принятия результата
            ReadyForTransfer.Write(true);


            //Получение статуса линии
            //с учетом таймера, учитывающего время
            //остановки линии
            var lineistop = false;

            lineistop = (bool)DataBridge.Conveyor.IsStopFromTimerTag.Value;

            /*
                Если линия не в работе (определяется по таймеру остановки в TIA) 
            */
            //if (lineistop == true)
            //{
               
            //    //Подача звукового сигнала
            //    DataBridge.Buzzer.On();

            //    //Запись сообщения в базу данных
            //    DataBridge.AlarmLogging.AddMessage($"Получен штрихкод при остановленной линии", MessageType.Alarm);

            //    //Вывод сообщения в зоне информации
            //    string message = $"Конвейер не запущен, полученный код не будет записан в результат";
            //    var msg = new UserMessage(message, DataBridge.myRed);
            //    DataBridge.MSGBOX.Add(msg);

            //    //Вызов окна
            //    customMessageBox mb = new customMessageBox("Ошибка", "Подтвердите удаление продукта с конвейера!");
            //    mb.Owner = DataBridge.MainScreen;
            //    mb.ShowDialog();


            //    //Выход из метода
            //    return;

            //}

            //Получение GTIN из задания
            var task_gtin = DataBridge.WorkAssignmentEngine.GTIN;

            //Разбор телеграммы из ПЛК
            //var data = SCAN_DATA.StatusText;
            //spliter.Split(ref data);

            //Получение GTIN и SerialNumber и криптохвоста из разобранного
            //штрихкода, полученного из ПЛК
            //искуственно добавляем криптохвост к серийнику через пробел

            var scaner_serialnumber = String.Concat("21", spliter.SerialNumber, "\u001d93", spliter.Crypto);
            //var scaner_serialnumber = spliter.SerialNumber;
            var scaner_gtin = spliter.GTIN;



            /*
                Если сигнатура задачи не совпадает
                с той, что указана в задании
            */
            if (spliter.IsValid == false)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"От автоматического сканера получен посторонний код: {spliter.SourseData}  (код не является СИ)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний код (не является КМ)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowExtraneousBarcode(msg);
                windowExtraneousBarcode.Show();

                //Выход из метода
                return;
            }

            /*
                Если GTIN из сканера и задачи
                не совпадают - формируем ошибку
            */
            if (scaner_gtin != task_gtin)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Посторонний продукт (GTIN не совпадает с заданием)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в зоне информации
                string message = $"Посторонний продукт (GTIN не совпадает с заданием)";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowExtraneousBarcode = new windowGtinFault(scaner_gtin, scaner_serialnumber, msg);
                windowExtraneousBarcode.Show();

                //Выход из метода
                return;
            }

            /*
                Если продукт числится в браке
            */
            if (DataBridge.Report.IsDeffect(scaner_serialnumber) == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Номер продукта {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} числится в браке", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в окно информации
                string message = $"Номер продукта {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} числится в браке";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Вызов окна
                var windowProductIsDeffect = new windowProductIsDeffect(scaner_serialnumber, msg);
                windowProductIsDeffect.Show();

                //Выход из метода
                return;
            }


            /*
                Повтор кода запрещен
            */
            //Получение статуса тэга
            //ПОВТОР КОДА
            //остановки линии
            var RepeatEnable = REPEAT_ENABLE.Value;           

            //Флаг, указывающий на то, является ли код повтором
            var IsRepeat = DataBridge.Report.IsRepeat(scaner_serialnumber);


            if (RepeatEnable == false && IsRepeat == true)
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage($"Продукт GTIN {scaner_gtin} номер {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} считан повторно", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод сообщения в окно информации
                string message = $"Продукт номер {scaner_serialnumber.Substring(2, scaner_serialnumber.Length - 8)} считан повторно.";
                var msg = new UserMessage(message, DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Добавление повтора в отчет
                DataBridge.Report.AddRepeatProduct(scaner_serialnumber);

                //Вызов окна
                var windowRepeatProduct = new windowRepeatProduct(scaner_gtin, scaner_serialnumber, msg);
                windowRepeatProduct.Show();
                               
                //Выход из метода
                return;
            }

            /*
               Если повтор - увеличиваем счетчик на единицу
           */
            //if (IsRepeat == true)
            //{
            //    uint value = 0;
            //    var result = uint.TryParse(RepeatProductCounter.Status.ToString(), out value);
            //    if (result == true)
            //    {
            //        value++;
            //        RepeatProductCounter.Write(value);
            //    }
            //}
            

            /*
                Если проверка прошла успешно добавляем 
                продукт в результат
            */

            //Добавляем просканированное изделие
            //в коллекцию изделий результата
            DataBridge.Report.AddBox(scaner_serialnumber);



        }

        /// <summary>
        /// Метод для очистки коллекции продукции
        /// находящейся между сканером
        /// и отбраковщиком
        /// </summary>
        public void ClearCollection()
        {
            CLEAR_ITEMS_COLLECTION_CMD.Write(true);
        }

        /// <summary>
        /// Команда для очистки коллекции в ПЛК
        /// </summary>
        public ICommand ClearCollectionCMD
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ClearCollection();
                },
                (obj) => (true));
            }
        }

    }
}
