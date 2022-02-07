using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий модель
    /// поля данных
    /// </summary>
    public class DataField
    {
        /// <summary>
        /// Флаг, указывающий на валидность
        /// принятых данных
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Индекс
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Префикс поля
        /// </summary>
        public string Preficks
        {
            get;
            set;
        }

        /// <summary>
        /// Длина поля
        /// </summary>
        public int Length
        {
            get;
            set;
        }

        /// <summary>
        /// Данные, хранящиеся в поле
        /// </summary>
        public string Data
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="preficks"></param>
        /// <param name="length"></param>
        public DataField(string preficks, string length)
        {
            Preficks = preficks;
            Length = int.Parse(length);
        }

        /// <summary>
        /// Метод, осуществляющий проверку
        /// поля на соответствие
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Check(ref string data)
        {

            //Инициализация строки, в которой будет хранится результат
            Data = string.Empty;

            //Инициализация строки-символа
            //который необходимо удалить из данных 
            string rs = string.Empty;
            rs += '\u001d';

            //Строка данных в верхнем регистре
            var DataUp = data.ToUpper().Replace(rs, "");

            //Префикс в верхнем регистре
            var PreficksUp = Preficks.ToUpper();

            //Если начало строки данных
            //является префиксом
            bool ispreficks = DataUp.IndexOf(PreficksUp) == 0;

            //Если первые символы не префикс, возвращаем false
            if (ispreficks == false)
            {
                return false;
            }

            //Удаляем префикс из входящей строки
            CompareAndClearPreficks(ref data);

            //Получаем значение 
            for (int i = 0; i < Length; i++)
            {
                //Если данные закончены
                //возвращаем false
                if (data.Length == 0)
                {
                    return false;
                }    

                Data += data[0];
                data = data.Remove(0, 1);
            }

            //Возврат положительного результата
            return true;
        }

        /// <summary>
        /// Метод для сравнения префикса и удаления его
        /// из основной строки
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool CompareAndClearPreficks(ref string data)
        {
            //Длина префикса
            var lenght = Preficks.Length;

            //Префикс, который  будет набран при удалении
            //его из основной строки
            var preficks = string.Empty;

            for (int i = 0; i < lenght; i++)
            {
                preficks += data[0];
                data = data.Remove(0, 1);
            }

            //Возвращаем результат сравнения префиксов
            return preficks == Preficks;
        }


    }
}
