using SortingStantion.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using S7Communication;
using System.Windows.Media;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий результат операций
    /// </summary>
    public class Report : INotifyPropertyChanged
    {

        /// <summary>
        /// Флаг, что отчет загружен из файла Task.json
        /// </summary>
        public bool IsLoadFromFile = false;

        /// <summary>
        /// Уникальный идентификатор задания. 
        /// </summary>
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        string _id;

        /// <summary>
        /// Время начала работы с заданием
        /// </summary>
        public string startTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
            }
        }
        string _startTime;

        /// <summary>
        /// Время окончания работы с заданием 
        /// </summary>
        public string endTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value;
            }
        }
        string _endTime;

        /// <summary>
        /// URL адрес для отправки отчетов на  L3
        /// </summary>
        string SrvL3UrlReport;

        /// <summary>
        /// Логин для авторизации на L3
        /// </summary>
        string SrvL3Login;

        /// <summary>
        /// Пароль  для авторизации на L3
        /// </summary>
        string rvL3Password;

        /// <summary>
        /// Массив, содержащий объекты Operator 
        /// описывающие мастера на линии
        /// </summary>
        List<UserAuthorizationHistotyItem> operators = new List<UserAuthorizationHistotyItem>();

        /// <summary>
        /// Акцессор для предоставления упрощенного
        /// доступа к экземпляру последнего оператора
        /// </summary>
        public UserAuthorizationHistotyItem LastOperator
        {
            get
            {
                if (operators.Count == 0)
                {
                    return null;
                }

                //Последний оператор
                return operators[operators.Count - 1];
            }
        }

        /// <summary>
        /// Список всех кодов, находящихся в результате
        /// </summary>
        public ObservableCollection<string> AllCodes
        {
            get;
            set;
        }

        /// <summary>
        /// Список всех номеров продуктов 
        /// </summary>
        public List<string> Codes = new List<string>();

        /// <summary>
        /// Счетчик продуктов
        /// </summary>
        public int ProductCount
        {
            get
            {
                return Codes.Count;
            }
        }

        /// <summary>
        /// Счетчик коробов
        /// </summary>
        public int BoxCount
        {
            get
            {
                if (DataBridge.WorkAssignmentEngine.SelectedWorkAssignment == null)
                {
                    return 0;
                }
                else
                    if (DataBridge.WorkAssignmentEngine.SelectedWorkAssignment.numPacksInBox == 0)
                {
                    return 0;
                }
                return Codes.Count / DataBridge.WorkAssignmentEngine.SelectedWorkAssignment.numPacksInBox;

            }
        }

        /// <summary>
        /// Счетчик отбракованных продуктов
        /// </summary>
        public int DeffectCount
        {
            get
            {
                return defectiveCodes.Count;
            }
        }

        /// <summary>
        /// Счетчик повторов
        /// </summary>
        public int RepeatCount
        {
            get
            {
                int count = 0;
                foreach (var repeat in repeatPacks)
                {
                    count += repeat.quantity;
                }
                return count;
            }
        }

        /// <summary>
        /// Массив отбракованных вручную номеров продуктов 
        /// </summary>
        public List<string> defectiveCodes = new List<string>();

        /// <summary>
        /// Массив, содержащий номера продуктов, прошедших сканер повторно.
        /// </summary>
        public List<RepeatPack> repeatPacks = new List<RepeatPack>();

        /// <summary>
        /// Текущее рабочее задание
        /// </summary>
        WorkAssignment CurrentWorkAssignment
        {
            get
            {
                return DataBridge.WorkAssignmentEngine.SelectedWorkAssignment;
            }
        }

        /// <summary>
        /// Флаг, указывающий на ошибку отправки отчета
        /// </summary>
        public bool SendReportErrorMemmoryFlag;

        /// <summary>
        /// Событие генериремое при отправке отчета либо
        /// ошибки отправки отчета
        /// </summary>
        public event Action<bool> SendReportNotification;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Report()
        {
            //Загрузка настроек из файла
            SrvL3Login = DataBridge.SettingsFile.GetSetting("SrvL3Login").Value;
            rvL3Password = DataBridge.SettingsFile.GetSetting("SrvL3Pass").Value;
            SrvL3UrlReport = DataBridge.SettingsFile.GetSetting("SrvL3UrlReport").Value;

            //Инициализация коллекции всех кодов
            //находящихся в результате
            AllCodes = new ObservableCollection<string>();

            //Подпись на событие по завершению задания
            DataBridge.WorkAssignmentEngine.WorkOrderCompletionNotification += (workAssignment) =>
            {               
               // endTime = DateTime.Now.GetDateTimeFormats()[43];

                //if (LastOperator != null)
                //{
                //    LastOperator.endTime = endTime;
                //}
            };

            //Подпись на событие по авторизации нового пользователя
            DataBridge.MainAccesLevelModel.ChangeUser += (accesslevel, currentuser, archive) =>
            {
                //Если отсутсвует разрешение на архивацию истории авторизации
                //выходим из метода
                if (archive == false)
                {
                    return;
                }

                //Если задание не в работое - запись в историю 
                //регистрации в отчет не заносим
                var inwork = DataBridge.WorkAssignmentEngine.InWork;
                if (inwork == false)
                {
                    return;
                }               

                //Если зарегистрирован не ОПЕРАТОР[0] или не МАСТЕР[1]
                //сведения о регистрации пользователя в отчет не попадают
                if (accesslevel > 1)
                {
                    return;
                }

                //Если оператор не изменился, в историю авторизации
                //мастера не добавляем
                var lastuser = string.Empty;

                //Если указатель на последнего 
                //пользователя не null
                if (LastOperator != null)
                {
                    //Запоминаем ID последнего пользователя
                    lastuser = LastOperator.id;
                }

                //Если последний оператор не
                //поменялся
                if (currentuser.ID == lastuser && IsLoadFromFile == false)
                {
                    return;
                }

                //Сохраняем время завершения работы
                //предыдущего мастера
                if (LastOperator != null)
                {
                    //Если окончание работы мастера не определено
                    if (string.IsNullOrEmpty(LastOperator.endTime) == true)
                    {
                        LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
                    }
                }

                //Сброс флага, указывающего на то
                //что отчет загружен из файла
                IsLoadFromFile = false;

                //Создаем запись истории регистрации мастера
                var historyitem = new UserAuthorizationHistotyItem(currentuser);

                //Добавляем запись в журнал
                operators.Add(historyitem);
            };

            //Загрузка результата выцполнения задания
            //из файла резервной копии
            try
            {
                LoadFromBackupFile();
            }
            catch (System.Net.HttpListenerException ex)
            {
                //Запись в лог
                Logger.AddExeption("Report.cs", ex);
            }

            DataBridge.WorkAssignmentEngine.WorkOrderAcceptanceNotification += (task) =>
            {
                //Обновление счетчиков на UI
                UpdateCounters();
            };
        }

        /// <summary>
        /// метод для добавления пользователя
        /// </summary>
        public void AddAuthorizationUser(UserAuthorizationHistotyItem historyitem)
        {
            this.operators.Add(historyitem);
        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в браке
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsDeffect(string serialnumber)
        {
            foreach (var dc in defectiveCodes)
            {
                if (dc == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод, указывающий на то,
        /// содержится ли код в результате
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool AsAResult(string serialnumber)
        {
            foreach (var code in Codes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод для проверки повторяется ли продукт или нет
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsRepeat(string serialnumber)
        {
            /*
                Проверка кодов в списке кодов
            */
            foreach (var code in Codes)
            {
                if (code == serialnumber)
                {
                    return true;
                }
            }

            /*
                Проверка кодов в списке повторов
            */
            foreach (var repeatPack in repeatPacks)
            {
                if (repeatPack.num == serialnumber)
                {
                    return true;
                }
            }

            //Если не нашли код, возвращаем
            //результат
            return false;
        }

        /// <summary>
        /// Метод для проверки содержится ли
        /// продукт в коллекции повторов
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        public bool IsContentsRepeatCollection(string serialnumber)
        {
            /*
                Проверка кодов в списке повторов
            */
            foreach (var repeatPack in repeatPacks)
            {
                if (repeatPack.num == serialnumber)
                {
                    return true;
                }
            }

            //Если не нашли код, возвращаем
            //результат
            return false;
        }

        /// <summary>
        /// Метод для добавления нового
        /// просканированного короба
        /// </summary>
        public void AddBox(string serialnumber)
        {
            /*
                Добавление продукта в список
                продуктов 
            (на экране???)
            */
            AllCodes.Insert(0, serialnumber.Substring(2, serialnumber.Length - 8));


            /*
                Если кодов больше 6
                удаляем самый ранний
            */
            if (AllCodes.Count > 6)
            {
                AllCodes.RemoveAt(AllCodes.Count - 1);
            }

            /*
                Объявление локальных переменных
            */
            var msg = string.Empty;
            UserMessage messageItem = null;
                        

            /*
                Если код повторяется 
            */
            if (IsRepeat(serialnumber) == true)
            {
                //Добавление повтора в коллекцию
                AddRepeatProduct(serialnumber);

                //Обновление счетчиков на UI
                UpdateCounters();

                //Выход из процедуры
                return;
            }

            /*
                Если код не повторяется 
            */
            Codes.Add(serialnumber);

            //Обновление счетчиков на UI
            UpdateCounters();
        }

        /// <summary>
        /// Метод для добавления кода повтора продукта
        /// </summary>
        public void AddRepeatProduct(string serialnumber)
        {
            //Осуществляем поиск повтора из отчета
            foreach (var repeatPack in repeatPacks)
            {
                //Если уже есть код продукта, который
                //следует добавить, инкрементируем счетчик
                if (repeatPack.num == serialnumber)
                {
                    repeatPack.quantity++;

                    //Обновление счетчиков на UI
                    UpdateCounters();

                    return;
                }
            }

            //Добавляем экземпляр повтора кода
            var repeatPackItem = new RepeatPack()
            {
                num = serialnumber,
                quantity = 1
            };

            repeatPacks.Add(repeatPackItem);

            //Обновление счетчиков на UI
            UpdateCounters();
        }

        /// <summary>
        /// Метод для добавления деффекта в результат
        /// </summary>
        /// <param name="serialnumber"></param>
        public void AddDeffect(string serialnumber)
        {
            var  dcounter = RemoteCode(serialnumber);
            RemoteCodeFromFullResult(serialnumber);
            defectiveCodes.Add(serialnumber);

            //Обновление счетчиков на UI
            UpdateCounters();
        }

        /// <summary>
        /// Метод для удаления кодов из результата
        /// </summary>
        /// <param name="serialnumber"></param>
        int RemoteCode(string serialnumber)
        {
            int count = 0;

            M0: foreach (var code in Codes)
            {
                if (serialnumber == code)
                {
                    count++;

                    Codes.Remove(code);
                    goto M0;
                }
            }

            return count;
        }

        /// <summary>
        /// Метод для удаления кода из полного
        /// результата
        /// </summary>
        /// <param name="serialnumber"></param>
        void RemoteCodeFromFullResult(string serialnumber)
        {
        M0: foreach (var code in AllCodes)
            {
                if (serialnumber.Substring(2, serialnumber.Length - 8) == code)
                {
                    AllCodes.Remove(code);
                    goto M0;
                }
            }
        }

        /// <summary>
        /// Метод для сериализации отчета
        /// </summary>
        /// <returns></returns>
        string Serialize()
        {
            //Если за время работы никто не авторизировался
            //сериализацию результата не производим
            if (LastOperator == null)
            {
                return "";
            }

            //Если время окончания определено, записваем
            //время окончания работы мастера как время завершения задания
            if (string.IsNullOrEmpty(endTime) == false)
            {
                LastOperator.endTime = endTime;
            }

            //Если время завершения последнего мостера не определено
            //записваем текущее время
            if (string.IsNullOrEmpty(LastOperator.endTime) == true)
            {
                LastOperator.endTime = DateTime.Now.GetDateTimeFormats()[43];
            }

            //Создание бэкап файла
            var reportBackupFile = new ReportBackupFile()
            {
                id = DataBridge.WorkAssignmentEngine.ID,
                operators = this.operators,
                startTime = this.startTime,
                endTime =   this.endTime,
                defectiveCodes = this.defectiveCodes,
                Packs = this.Codes,
                repeatPacks = this.repeatPacks
            };

            //Сериализация
            return JsonConvert.SerializeObject(reportBackupFile);
        }

        /// <summary>
        /// Метод для сохранения
        /// результата в файл в определенную папку
        /// </summary>
        public void Save(string dirName)
        {
            if (CurrentWorkAssignment == null)
            {
                return;
            }

            //Получение сериализованного отчета
            var content = Serialize();

            //Если ответ не сериализирован, выходим
            if (string.IsNullOrEmpty(content) == true)
            {
                return;
            }

            //Если директория не создана
            //создаем её
            if (Directory.Exists(dirName) == false)
            {
                Directory.CreateDirectory(dirName);
            }

            //Получение имени файла
            var filename = $@"{dirName}\{DataBridge.WorkAssignmentEngine.ID}.txt";

            //Сохранение файла
            StreamWriter sr = new StreamWriter($@"{filename}");
            sr.Write(content);
            sr.Close();
        }

        /// <summary>
        /// Метод для создания бэкап файла
        /// </summary>
        public void CreateBackupFile()
        {
            this.Save("Report");
        }

        /// <summary>
        /// Метод для загрузки результата незавершенного задания
        /// из файла резервной копии
        /// </summary>
        async void LoadFromBackupFile()
        {
            var files = Directory.GetFiles("Report");

            //Если файлов больше 1 выводим ошибку
            if (files.Length > 1)
            {
                string message = "Ошибка загрузки незавершенного задания. Обратитесь к наладчику";
                ShowMessage(message, DataBridge.myRed);
                return;
            }

            //Если файлы отсутсвуют, пропускаем загрузку файла
            if (files.Length == 0)
            {
                return;
            }

            //Получение файла незавершенного задания
            var file = $@"{files[0]}";

            //Устанавливаем флаг, что отчет загружен из файла
            IsLoadFromFile = true;

            ReportBackupFile reportBackupFile = null;

            //Десериализация задачи из памяти программы
            // чтение данных
            using (StreamReader fs = new StreamReader(file))
            {
                var text = fs.ReadToEnd();
                reportBackupFile = JsonConvert.DeserializeObject<ReportBackupFile>(text);
                fs.Close();

                this.ID = reportBackupFile.id;
                this.operators = reportBackupFile.operators;
                this.repeatPacks = reportBackupFile.repeatPacks;
                this.startTime = reportBackupFile.startTime;
                this.endTime = reportBackupFile.endTime;
                this.defectiveCodes = reportBackupFile.defectiveCodes;
                this.Codes = reportBackupFile.Packs;

                //Обновление счетчиков на UI
                UpdateCounters();
            }
        }

        /// <summary>
        /// метод для отправки отчета
        /// Выполняется в отдельном потоке
        /// </summary>
        public bool SendReport()
        {
            //Отправка отчета на L3
            var result = SendReportToL3();

            //Записываем статус ошибки отправки последнего отчета
            SendReportErrorMemmoryFlag = !result;

            //Уведомление подписчиков
            SendReportNotification?.Invoke(SendReportErrorMemmoryFlag);

            //Возврат результата
            return result;
        }

        /// <summary>
        /// Метод для получения токена
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="tenancyName"></param>
        /// <returns></returns>
        private string GetToken(string url, string username, string password, string tenancyName = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var input = "{\"usernameOrEmailAddress\":\"" + username + "\"," +
                            "\"password\":\"" + password + "\"}";

                if (tenancyName != null)
                {
                    input = input.TrimEnd('}') + "," +
                            "\"tenancyName\":\"" + tenancyName + "\"}";
                }

                streamWriter.Write(input);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            // Crude way
            var entries = response.TrimStart('{').TrimEnd('}').Replace("\"", String.Empty).Split(',');

            foreach (var entry in entries)
            {
                if (entry.Split(':')[0] == "result")
                {
                    return entry.Split(':')[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Метод для отправки результата на L3
        /// </summary>
        bool SendReportToL3()
        {
            var username = SrvL3Login;
            var password = rvL3Password;

            //Получение получателя отчета
            var endPoint = SrvL3UrlReport;

            //Формирование http запроса с отчетом
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);
            httpWebRequest.ContentType = "applicaion/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.PreAuthenticate = true;

            string authorisation = string.Format("{0}:{1}", username,  password);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorisation));
            string header = string.Format("{0} {1}", "Basic", encoded);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = header;


            try
            {
                //Отправка отчета конечному получателю
                using (var sw = httpWebRequest.GetRequestStream())
                {
                    string json = Serialize();

                    //Если ответ не сериализирован, выходим
                    if (string.IsNullOrEmpty(json) == true)
                    {
                        return false;
                    }

                    //Получение массива байт из json
                    byte[] byteArray = Encoding.UTF8.GetBytes(json);

                    //Запись данных в поток вывода
                    sw.Write(byteArray, 0, byteArray.Length);
                }

                //Прием ответа от получателя отчетов
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //Если в ответа содержится 201
                    //возвращаем true
                    if (httpResponse.StatusCode == HttpStatusCode.Created)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                //В случае невозможности отправить
                //отчет на L3 - создаем резервную копию в папке "Report"
                CreateBackupFile();

                //Запись исключения в лог
                Logger.AddExeption("Report.cs", ex);
            }

            return false;

        }

        /// <summary>
        /// Метод для отображения сообщения в зоне информации
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message, Brush color)
        {
            Action action = () =>
            {
                var msgitem = new UserMessage(message, color);
                DataBridge.MSGBOX.Add(msgitem);
            };
            DataBridge.UIDispatcher.Invoke(action);
        }

        /// <summary>
        /// Метод для очистки результата операций
        /// </summary>
        public void ClearResult()
        {
            //Удаление бэкап файла
            DeleteBackupFile();

            //Обнуление результата
            this.ID = string.Empty;
            this.operators = new List<UserAuthorizationHistotyItem>();
            this.repeatPacks = new List<RepeatPack>();
            this.startTime = string.Empty;
            this.endTime = string.Empty;
            this.defectiveCodes = new List<string>();
            this.Codes.Clear();
            AllCodes.Clear();

            //Обновление счетчиков на UI
            UpdateCounters();
        }

        /// <summary>
        /// Метод для удаления бэкап файла
        /// </summary>
        public void DeleteBackupFile()
        {
            //Получаем имя бэкап файла
            var backupfile = $@"Report\{DataBridge.WorkAssignmentEngine.ID}.txt";

            //Если бэкап файл существует - удаляем его
            if (File.Exists(backupfile) == true)
            {
                File.Delete(backupfile);
            }
        }

        /// <summary>
        /// Метод для обновления счетчиков на UI
        /// </summary>
        void UpdateCounters()
        {
            OnPropertyChanged("ProductCount");
            OnPropertyChanged("BoxCount");
            OnPropertyChanged("DeffectCount");
            OnPropertyChanged("RepeatCount");
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
