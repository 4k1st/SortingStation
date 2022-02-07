using S7Communication;
using SortingStantion.Controls;
using SortingStantion.S7Extension;
using SortingStantion.TOOL_WINDOWS.windowErrorTransmitionBarcode;
using SortingStantion.ToolsWindows.windowOverDeffectCounter;
using SortingStantion.ToolsWindows.windowProductsAreTooCloseToEachOther;
using SortingStantion.ToolsWindows.windowPusherError;
using System;
using System.Windows.Threading;

namespace SortingStantion.Models
{
    public class AlarmsEngine
    {

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

        /// <summary>
        /// Деблокировка
        /// если имеются критичные ошибки Interlock = false
        /// если отсутсвуют критичные ошибки Interlock = true
        /// </summary>
        public bool Interlock
        {
            get
            {
                //Если все ошибки не активны - возвращаем TRUE
                //В случае наличая хотяч бы одной ошибки - FALSE
                return interlock;
            }
            set
            {
                //Если текущее состояние деблокировки изменилось - генирируем событие
                //и уведомляем подписчиков
                var currentinterlock = al_1.Value == false && al_2.Value == false && al_3.Value == false && new_1.Value == false;
                if (currentinterlock != interlock)
                {
                    //Получение текущего состояния деблокировки
                    interlock = al_1.Value == false && al_2.Value == false && al_3.Value == false && new_1.Value == false;

                    //Уведомление подписчиков
                    ChangeInterlockState?.Invoke();
                }
                interlock = currentinterlock;
            }
        }
        bool interlock = true;

        /// <summary>
        /// Событие, генерируемое при изменении
        /// состояния деблокировки
        /// </summary>
        public event Action ChangeInterlockState;

        /// <summary>
        /// Тэ для сброса ошибок
        /// </summary>
        public S7_Boolean ResetAlarmsTag;

        /// <summary>
        /// Ошибка 1 - Посторонний продукт (GTIN не совпадает с заданием)
        /// </summary>
        public S7DiscreteAlarm al_1;

        /// <summary>
        /// Ошибка 2 - Посторонний код (код не является СИ)
        /// </summary>
        public S7DiscreteAlarm al_2;

        /// <summary>
        /// Ошибка 3 - Номер продукта числится в браке
        /// </summary>
        public S7DiscreteAlarm al_3;

        /// <summary>
        /// Ошибка 4 - Повтор кода продукта
        /// </summary>
        public S7DiscreteAlarm al_4;

        /// <summary>
        /// Ошибка 5 - Получение кода от сканера при остановке конвейера
        /// </summary>
        public S7DiscreteAlarm al_5;

        /// <summary>
        /// Ошибка 6 - Ошибка отбраковщика (продукт не отбраковался)
        /// </summary>
        public S7DiscreteAlarm al_6;

        /// <summary>
        /// Ошибка 7 - Массовый брак
        /// </summary>
        public S7DiscreteAlarm al_7;

        /// <summary>
        /// Ошибка 8 - Ошибка отбраковщика
        /// </summary>
        public S7DiscreteAlarm al_8;

        /// <summary>
        /// Ошибка 9 - Продукт слишком близко друг другу в зоне сканера
        /// </summary>
        public S7DiscreteAlarm al_9;

        /// <summary>
        /// Ошибка 10 - Продукт слишком близко друг другу в зоне отбраковщика
        /// </summary>
        public S7DiscreteAlarm al_10;

        /// <summary>
        /// Ошибка 13 - Ошибка ИБП UPS
        /// </summary>
        public S7DiscreteAlarm al_13;

        /// <summary>
        /// Ошибка 12 - Ошибка коммуникации с автоматическим сканером
        /// </summary>
        public S7DiscreteAlarm new_1;

        /// <summary>
        /// Количесво изделий,
        /// отбракованых вручную
        /// </summary>
        public S7_DWord QUANTITY_PRODUCTS_MANUAL_REJECTED;

        /// <summary>
        /// Счетчик брака
        /// </summary>
        public S7_DWord DEFFECT_PRODUCTS_COUNTER;


        /// <summary>
        /// Сообщение о том, что соединение с ПЛК потеряно
        /// </summary>
        Controls.UserMessage msgLostConnection;

        /// <summary>
        /// Окно ошибки "МАССОВЫЙ БРАК"
        /// </summary>
        windowOverDeffectCounter windowOverDeffectCounter;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public AlarmsEngine()
        {
            //Инициализация тэга по которому осуществляется сброс
            //всех сообщений
            ResetAlarmsTag = (S7_Boolean)device.GetTagByAddress("DB1.DBX132.1");

            //Тэг, хранящий количество изделий, отбраковыных вручную
            QUANTITY_PRODUCTS_MANUAL_REJECTED = (S7_DWord)device.GetTagByAddress("DB1.DBD28-DWORD");

            //Тэг, хранящий счетчик подряд отбракованых продуктов
            DEFFECT_PRODUCTS_COUNTER = (S7_DWord)device.GetTagByAddress("DB1.DBD32-DWORD");

            /*
                Неисправность фотодатчика FS1
            */
            al_1 = new S7DiscreteAlarm("Отсутствует связь с датчиком сканера, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.0", group);
            al_1.ChangeValue += (oldstate, currentstate) => { Interlock = false; };
            al_1.ShowMessage = true;
            al_1.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS1 (перед сканером)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                var msg = new UserMessage("Отсутствует связь с датчиком сканера, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", DataBridge.myRed);
                DataBridge.MSGBOX.Add(msg);

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };



            /*
                Неисправность фотодатчика FS2
            */
            al_2 = new S7DiscreteAlarm("Отсутствует связь с датчиком отбраковщика, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.1", group);
            al_2.ChangeValue += (oldstate, currentstate) => { Interlock = false; };
            al_2.ShowMessage = true;
            al_2.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS2 (перед отбраковщиком)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //var msg = new UserMessage("Отсутствует связь с датчиком отбраковщика, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };

            /*
                Неисправность фотодатчика FS3
            */
            al_3 = new S7DiscreteAlarm("Отсутствует связь с датчиком контроля отбраковки, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX12.2", group);
            al_3.ChangeValue += (oldstate, currentstate) => { Interlock = false; };
            al_3.ShowMessage = true;
            al_3.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность фотодатчика FS3 (перед отбраковщиком)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //var msg = new UserMessage("Отсутствует связь с датчиком контроля отбраковки, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };


            /*
               Отсутствие связи с автоматическим сканером
           */
            new_1 = new S7DiscreteAlarm("Отсутствует связь с автоматическим сканером, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", "DB6.DBX13.5", group);
            new_1.ChangeValue += (oldstate, currentstate) => { Interlock = false; };
            new_1.ShowMessage = true;
            new_1.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Неисправность коммуникации с автоматическим сканером", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //var msg = new UserMessage("Отсутствует связь с датчиком сканера, уберите все продукты в зоне работы комплекса и обратитесь к наладчику.", DataBridge.myRed);
                //DataBridge.MSGBOX.Add(msg);

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };

            /*
                Получение кода от сканера при остановке конвейера
            */
            //al_5 = new S7DiscreteAlarm("Получение кода от сканера при остановке конвейера", "DB6.DBX12.4", group);
            //al_5.MessageAction = () =>
            //{

            //    //Запись сообщения в базу данных
            //    DataBridge.AlarmLogging.AddMessage("Получение кода от сканера при остановке конвейера", MessageType.Alarm);
            //};

            /*
                Ошибка отбраковщика (продукт не отбраковался)
            */
            al_6 = new S7DiscreteAlarm("Ошибка отбраковщика (продукт не отбраковался)", "DB6.DBX12.5", group);
            al_6.MessageAction = () =>
            {
                //Сброс ошибки
                al_6.Write(false);

                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Ошибка отбраковщика (продукт не отбраковался)", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод окна ошибки
                windowPusherError windowPusherError = new windowPusherError(al_6);
                windowPusherError.Show();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };

            /*
                Массовый брак
            */
            al_7 = new S7DiscreteAlarm("Массовый брак", "DB6.DBX12.6", group);
            al_7.MessageAction = () =>
            {
                //Сброс ошибки
                al_7.Write(false);

                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Массовый брак", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Вывод окна ошибки
                windowOverDeffectCounter = new windowOverDeffectCounter(DEFFECT_PRODUCTS_COUNTER, al_7);
                windowOverDeffectCounter.Show();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };

            /*
                Ошибка отправки продукта на ПК
            */
            al_8 = new S7DiscreteAlarm("Один из продуктов не поступил в ПК. Линия остановлена", "DB6.DBX12.7", group);
            al_8.MessageAction = () =>
            {
                //Сброс ошибки
                al_8.Write(false);

                //Остановка конвейера
                //DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Ошибка перемещения продукта из ПЛК в ПК", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //customMessageBox cmb = new customMessageBox("Ошибка внутренних связей", "Найдите с помощью операции Справка продукты, не попавшие в результат, и удалите их с конвейера либо добавьте в результат с помощью операции Добавить");
                //cmb.Show();

                var wetb = new windowErrorTransmitionBarcode(al_8);
                wetb.Show();

                //Очистка коллекции продуктов, расположенных
                //между сканером и отбраковщиком
                DataBridge.BoxEngine.ClearCollection();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();
            };

            /*
                Продукт слишком близко к предыдущему, удалите его с конвейера
            */
            al_9 = new S7DiscreteAlarm("Продукт в зоне сканера слишком близко к предыдущему, удалите его с конвейера", "DB6.DBX13.0", group);
            al_9.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Продукт слишком близко к предыдущему", MessageType.Alarm);

                //Сброс ошибки
                al_9.Write(false);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();


                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод окна ошибки
                windowProductsAreTooCloseToEachOther windowProductsAreTooCloseToEachOther = new windowProductsAreTooCloseToEachOther(message: "Продукт на фотодатчике 1 слишком близко к предыдущему. Удалите все продукты с линии между датчиками 1 и 2 и проверьте их коды операцией Справка.");
                windowProductsAreTooCloseToEachOther.ShowDialog();

            };

            /*
                Продукт слишком близко к предыдущему, удалите его с конвейера
            */
            al_10 = new S7DiscreteAlarm("Продукт в зоне отбраковщика слишком близко к предыдущему, удалите его с конвейера", "DB6.DBX13.1", group);
            al_10.MessageAction = () =>
            {
                //Остановка конвейера
                DataBridge.Conveyor.Stop();

                //Подача звукового сигнала
                DataBridge.Buzzer.On();

                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Продукт в зоне отбраковщика слишком близко к предыдущему", MessageType.Alarm);

                //Сброс ошибки
                al_10.Write(false);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();

                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Вывод окна ошибки
                windowProductsAreTooCloseToEachOther windowProductsAreTooCloseToEachOther = new windowProductsAreTooCloseToEachOther(message: "Продукт на фотодатчике 2 слишком близко к предыдущему. Удалите все продукты с линии между датчиками 1 и 2 и проверьте их коды операцией Справка.");
                windowProductsAreTooCloseToEachOther.ShowDialog();
            };


            /*
                Ошибка питания UPS (ИБП)
            */
            al_13 = new S7DiscreteAlarm("Потеря питания! Комплекс будет отключен", "DB6.DBX13.4", group);
            al_13.MessageAction = () =>
            {
                //Запись сообщения в базу данных
                DataBridge.AlarmLogging.AddMessage("Ошибка питания ИБП", MessageType.Alarm);

                //Переход на главный экран
                DataBridge.ScreenEngine.GoToMainWindow();


                //Извещение подписчиков о возникновлении
                //новой аварии
                DataBridge.NewAlarmNotificationMetod();

                //Создание окна
                SortingStantion.frameUSPFault.frameUSPFault fups = new SortingStantion.frameUSPFault.frameUSPFault();
                fups.Owner = DataBridge.MainScreen;
                fups.ShowDialog();
            };

            /*
                Ошибка потреи связи с устройством
            */
            device.LostConnection += () =>
            {
                Action action = () =>
                {
                    //Подписка на событие по восстановлению связи
                    //для очистки коллекции продуктов перед отбракощиком
                    device.GotConnection += ClearCollectionCallBack;

                    //Запись сообщения в базу данных
                    DataBridge.AlarmLogging.AddMessage("Линия остановлена. Обрыв связи с контроллером. Удалите все продукты с конвейера между первым и последним датчиком", MessageType.Alarm);

                    msgLostConnection = new Controls.UserMessage($"Линия остановлена. Обрыв связи с контроллером. Удалите все продукты с конвейера между первым и последним датчиком", DataBridge.myRed);
                    DataBridge.MSGBOX.Add(msgLostConnection);

                    //Извещение подписчиков о возникновлении
                    //новой аварии
                    DataBridge.NewAlarmNotificationMetod();

                    //Переход на главный экран
                    DataBridge.ScreenEngine.GoToMainWindow();

                };
                DataBridge.UIDispatcher?.Invoke(action);
            };

            /*
                Ошибка потреи связи с устройством
            */
            device.GotConnection += () =>
            {
                Action action = () =>
                {
                    //Запись сообщения в базу данных
                    DataBridge.AlarmLogging.AddMessage("Восстановление связи с ПЛК", MessageType.Alarm);

                    DataBridge.MSGBOX.Remove(msgLostConnection);


                    //Извещение подписчиков о возникновлении
                    //новой аварии
                    DataBridge.NewAlarmNotificationMetod();

                    //Переход на главный экран
                    DataBridge.ScreenEngine.GoToMainWindow();

                    
                };
                DataBridge.UIDispatcher?.Invoke(action);

            };

            //Имитация ошибок
            //DispatcherTimer Timer = new DispatcherTimer();
            //Timer.Interval = new TimeSpan(0,0,3);
            //Timer.Tick += (s, e) =>
            //{
            //    new_1.Invoke();
            //    Timer.Stop();
            //};
            //Timer.Start();
        }

        /// <summary>
        /// Очистка коллекции при восстановлении связи
        /// </summary>
        void ClearCollectionCallBack()
        {
            //Очистка коллекции продуктов, расположенных
            //между сканером и отбраковщиком
            DataBridge.BoxEngine.ClearCollection();

            //отписка от события
            device.GotConnection -= ClearCollectionCallBack;
        }
    }
}
