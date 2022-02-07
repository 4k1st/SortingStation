using SortingStantion.Controls;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SortingStantion.Models
{
    /// <summary>
    /// Модель, реализующая файл настроек
    /// </summary>
    public class SettingsFile
    {
        /// <summary>
        /// Xml документ с настройками
        /// </summary>
        public XmlDocument xDoc;

        /// <summary>
        /// Список всех настроек в файле конфигурации
        /// </summary>
        List<Setting> Settings = new List<Setting>();

        /// <summary>
        /// Полное имя (путь) до файла
        /// настроек
        /// </summary>
        string filename;


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="file"></param>
        public SettingsFile(string file)
        {
            //Загрузка файла конфигурации
            Load(file);
            this.filename = file;
        }

        /// <summary>
        /// Метод для загрузки файла конфигурации
        /// </summary>
        /// <param name="file"></param>
        public void Load(string file)
        {
            //Очистка всех настроек
            Settings.Clear();

            //Создаем экземпляр xml докумета
            xDoc = new XmlDocument();

            //Заргузка из файла
            try
            {
                xDoc.Load(file);

                //Получение корневого элементаэ
                XmlNode root = xDoc.ChildNodes[0];

                //Получаем коллекцию всех настроек
                var settingsnodes = root.ChildNodes[0];

                //Инициализация настроек и добавление 
                //их в список настроек
                foreach (XmlNode settingnode in settingsnodes)
                {
                    var setting = new Setting(settingnode, this);
                    Settings.Add(setting);
                }
            }
            catch (Exception ex)
            {
                customMessageBox.Show($"Ошибка чтения файла конфигурации: {file}"); 
                return;
            }
        }

        /// <summary>
        /// Метод для получения настройки
        /// по наименованию
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Setting GetSetting(string name)
        {
            //Ищем нужное свойство по атрибуту Name
            foreach (var setting in Settings)
            {
                if (name == setting.Name)
                {
                    return setting;
                }
            }

            //Возвращаем пустой результат
            return null;
        }

        /// <summary>
        /// Метод для получения значения из XmlNode
        /// по наименованию узла
        /// </summary>
        /// <returns></returns>
        public string GetValue(string name)
        {
            //Ищем нужное свойство по атрибуту Name
            foreach (var setting in Settings)
            {
                if (name == setting.Name)
                {
                    return setting.Value;
                }
            }

            //Возвращаем пустой результат
            return string.Empty;
        }

        /// <summary>
        /// Метод для получения настроек 
        /// из файла конфигурации
        /// </summary>
        /// <returns></returns>
        public Setting [] GetSettingsList()
        {
            return Settings.ToArray();
        }

        /// <summary>
        /// Метод для сохранения данных в файл
        /// </summary>
        public void Save()
        {
            xDoc.Save(this.filename);
        }
    }
}
