using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс для обновление времени
    /// на главном экране
    /// </summary>
    public class TimeUpdater : INotifyPropertyChanged
    {
        /// <summary>
        /// Время, отбражаемое на главном экране
        /// </summary>
        string currentTime;
        public string CurrentTime
        {
            get
            {
                return currentTime;
            }
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public TimeUpdater()
        {

            //Запуск потока для отображения времени
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    //currentTime = DateTime.Now.ToString("HH mm");
                    //OnPropertyChanged("CurrentTime");
                    //Thread.Sleep(1000);
                    currentTime = DateTime.Now.ToString("HH:mm:ss  dd.MM.yyyy");
                    OnPropertyChanged("CurrentTime");
                    Thread.Sleep(1000);
                }
            });

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
