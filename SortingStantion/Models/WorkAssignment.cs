using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, описывающий рабочее задание
    /// </summary>
    public class WorkAssignment : INotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        [JsonProperty("id")]
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// Номер GTIN
        /// </summary>
        [JsonProperty("gtin")]
        public string gtin
        {
            get;
            set;
        }

        /// <summary>
        /// Номер производственной серии, до 20 символов
        /// </summary>
        [JsonProperty("lotNo")]
        public string lotNo
        {
            get;
            set;
        }

        /// <summary>
        /// Номер линии для, которой сформировано это задание.
        /// </summary>
        //[JsonProperty("lineNum")]
        //public string lineNum
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Кол-во продуктов в коробе
        /// </summary>
        [JsonProperty("numPacksInBox")]
        public int numPacksInBox
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование продукта
        /// </summary>
        [JsonProperty("productName")]
        public string productName
        {
            get;
            set;
        }

        /// <summary>
        /// Ожидаемое количество продуктов в серии (определяется по заданию на производство серии) 
        /// </summary>
        [JsonProperty("numPacksInSeries")]
        public int numPacksInSeries
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WorkAssignment()
        {

        }

        /// <summary>
        /// Метод для сериализации задания
        /// </summary>
        /// <returns></returns>
        string Serialize()
        {
            //Сериализация
            return System.Text.Json.JsonSerializer.Serialize<WorkAssignment>(this);
        }

        /// <summary>
        /// Метод для сохранения задания в файл
        /// </summary>
        /// <param name="dirName"></param>
        public void Save(string dirName)
        {
            //Если директория не создана
            //создаем её
            if (Directory.Exists(dirName) == false)
            {
                Directory.CreateDirectory(dirName);
            }

            //Получение имени файла
            var filename = $@"{dirName}\{this.ID}.txt";

            //Получение содержимого файла
            var content = Serialize();

            //Получение массива байт
            var bytes = Encoding.UTF8.GetBytes(content);

            //Запись данных в файл
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();

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
