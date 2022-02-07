using SortingStantion.S7Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SortingStantion.TOOL_WINDOWS.windowErrorTransmitionBarcode
{
    /// <summary>
    /// Логика взаимодействия для windowErrorTransmitionBarcode.xaml
    /// </summary>
    public partial class windowErrorTransmitionBarcode : Window
    {
        /// <summary>
        /// Указатель на аварию, по причине
        /// которой появилось окно
        /// </summary>
        S7DiscreteAlarm alarm;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="alarm"></param>
        public windowErrorTransmitionBarcode(S7DiscreteAlarm alarm)
        {
            InitializeComponent();
            this.alarm = alarm;
        }

        /// <summary>
        /// Метод, вызываемый при нажатии на кнопку - ЗАКРЫТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            alarm.Write(false);
            //Закрытие окна
            this.Close();
        }
    }
}
