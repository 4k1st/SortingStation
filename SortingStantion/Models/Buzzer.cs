using S7Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingStantion.Models
{
    /// <summary>
    /// Модель, реализующая звуковой извещатель
    /// </summary>
    public class Buzzer
    {
        #region SIMATIC СУЩНОСТИ 

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        SimaticClient server
        {
            get
            {
                return DataBridge.S7Server;
            }
        }

        /// <summary>
        /// Указатель на экземпляр ПЛК
        /// </summary>
        SimaticDevice device
        {
            get
            {
                return server.Devices[0];
            }
        }

        /// <summary>
        /// Указатель на группу, где хранятся все тэгиК
        /// </summary>
        SimaticGroup group
        {
            get
            {
                return device.Groups[0];
            }
        }

        #endregion

        /// <summary>
        /// Тэг, отвечающий за Принять - завершить задание
        /// </summary>
        S7_Boolean BUZZER_TAG
        {
            get;
            set;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Buzzer()
        {
            BUZZER_TAG = (S7_Boolean)device.GetTagByAddress("DB1.DBX86.0");
        }

        /// <summary>
        /// Метод для включения звукового извещателя
        /// </summary>
        public void On()
        {
            BUZZER_TAG.Write(true);
        }
    }
}
