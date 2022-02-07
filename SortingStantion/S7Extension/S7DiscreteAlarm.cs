using S7Communication;
using SortingStantion.Controls;
using System;

namespace SortingStantion.S7Extension
{
    /// <summary>
    /// Класс, описывающий дисткетное сообщение
    /// </summary>
    public class S7DiscreteAlarm : S7_Boolean
    {
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        public bool ShowMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Метод, вызываемый при возникновлении аварии
        /// </summary>
        public Action MessageAction;

        /// <summary>
        /// Элемент сообщения об ошибке
        /// </summary>
        UserMessage msg;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="simaticGroup"></param>

        public S7DiscreteAlarm(string message, string address, SimaticGroup simaticGroup) : base(message, address, simaticGroup)
        {
            this.Message = message;
            this.ChangeValue += S7DiscreteAlarm_ChangeValue;

        }

        bool oldstatus = false;
        bool newstatus = false;

        /// <summary>
        /// Метод, вызываемый при изминении
        /// значения базового тэга
        /// </summary>
        /// <param name="value"></param>
        private void S7DiscreteAlarm_ChangeValue(object oldvalue, object newvalue)
        {

            Action action = () =>
            {
                newstatus = ToBool(newvalue);

                //Если новое значение не bool - выходим
                if (oldstatus == newstatus)
                {
                    return;
                }

                oldstatus = newstatus;


                //Если новое значение true, добавляем сообщение
                if (newstatus == true)
                {
                    //Вывод сообщения, если есть разрешение
                    if (ShowMessage == true)
                    {
                        msg = new UserMessage(Message, MSGTYPE.ERROR);
                        DataBridge.MSGBOX.Add(msg);
                    }

                    //Вывзов делегата
                    MessageAction?.Invoke();
                }

                //Если новое значение false, удаляем сообщение
                if (newstatus == false)
                {
                    DataBridge.MSGBOX.Remove(msg);
                    return;
                }
            };

            DataBridge.UIDispatcher.BeginInvoke(action);

            
        }

        bool ToBool(object obj)
        {
            if (obj is bool == false)
            {
                return false;
            }

            return bool.Parse(obj.ToString());
        }

        /// <summary>
        /// Метод для вызова делегата
        /// </summary>
        public void Invoke()
        {
            //Вывод сообщения, если есть разрешение
            if (ShowMessage == true)
            {
                msg = new UserMessage(Message, MSGTYPE.ERROR);
                DataBridge.MSGBOX.Add(msg);
            }

            //Вывзов делегата
            MessageAction?.Invoke();
        }
    }
}
