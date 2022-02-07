using S7Communication;
using S7Communication.Enumerations;
using System;

namespace SortingStantion.S7Extension
{
    public class Item : simaticTagBase
    {
        /// <summary>
        /// Показывает актуальность данных
        /// </summary>
        public bool IsActual
        {
            get
            {
                return isActual;
            }
            set
            {
                isActual = value;
                OnPropertyChanged("IsActual");
            }
        }
        bool isActual = false;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public uint ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged("ID");
            }
        }
        uint id = 0;

        /// <summary>
        /// Штрихкод
        /// </summary>
        public string  Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
                OnPropertyChanged("Code");
            }
        }
        string code = string.Empty;

        /// <summary>
        /// Флаг, указыывающий на необходимость отбраковки
        /// TRUE - отбраковка не требуется
        /// FALSE - требуется отбраковка
        /// </summary>
        public bool NoDeletionRequired
        {
            get
            {
                return noDeletionRequired;
            }
            set
            {
                noDeletionRequired = value;
                OnPropertyChanged("NoDeletionRequired");
            }
        }
        bool noDeletionRequired = false;

        public Item(int DataBlock, int firstByte, SimaticGroup simaticGroup) 
        {
            DBNumber = DataBlock;
            this.DataType = MemmoryArea.DataBlock;
            StartByteAddress = firstByte;
            Lenght = 48;

            simaticGroup.AddTag(this);
        }

        public override void BuildStatus(byte[] bytes, int startbytefromrequest)
        {
            int offset = StartByteAddress - startbytefromrequest;

            //Если данных меньше 4 байтов
            //выходим из функции
            if (bytes == null)
            {
                return;
            }

            if (bytes.Length < offset + Lenght)
            {
                return;
            }

            //Заполнение полей
            IsActual = GetBitState(bytes[0 + offset], 0);

            //Построение статуса из байтов
            ID = (UInt32)(bytes[2 + offset] << 8) | (UInt32)(bytes[3 + offset] << 0);

            var strcode = string.Empty;

            for (int i = 6; i < 46; i++)
            {
                if (bytes[i + offset] == '\0')
                {
                    break;
                }

                strcode += (char)bytes[i + offset];
            }

            if (Code != strcode)
            {
                Code = strcode;
            }
        }

        /// <summary>
        /// Метод для получения состояния бита
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        bool GetBitState(byte Value, byte Bit )
        {
            //Получаем слово
            var bt = Convert.ToByte(Value);

            Byte mask = (Byte)(1 << Bit);
            var temp = bt & mask;
            bool bitstate = temp > 0;
            return bitstate;
        }
    }
}
