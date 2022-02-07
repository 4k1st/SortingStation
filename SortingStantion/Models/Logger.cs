using System;
using System.IO;

namespace SortingStantion.Models
{
    /// <summary>
    /// Статичный класс для регистрации 
    /// исключений
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Метод для логирования исключения
        /// </summary>
        public static void AddExeption(string objectname, Exception exception)
        {
            string dir = string.Format(@"ExeptionsLog\{0:D4}_{1:D2}", DateTime.Now.Year, DateTime.Now.Month);

            //Если папка архивов не создана, создадим её
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            string filename = string.Format(@"{0}\{1:D4}_{2:D2}_{3:D2}.log", dir, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            //Если файл не создан, создадим его
            if (File.Exists(filename) == false)
            {
                CreateFile(filename);
            }

            //Пишем в файл
            string data = "Exeption\t" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss\t") + "objectname=" + objectname + "\t" + exception.Message;
            StreamWriter sw = new StreamWriter(filename, true);
            sw.WriteLine(data);
            sw.Close();

        }

        /// <summary>
        /// Метод для создания нового файла
        /// </summary>
        /// <param name="filename"></param>
        static void CreateFile(string filename)
        {
            //Запись текста в файл
            FileStream fs = new FileStream(filename, FileMode.CreateNew);
            fs.Close();
        }
    }
}
