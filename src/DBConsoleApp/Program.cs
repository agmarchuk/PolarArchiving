using System;
using System.Xml.Linq;
using Polar.Cassettes.DocumentStorage;

namespace DBConsoleApp
{
    class Program
    {
        private static string _path;
        private static XElement _config;
        // Доступ к движку только через свойство Engine и только последовательно!!! Но как??? Через методы! В адаптере есть!
        public static DStorage storage = null;
        private static DbAdapter _engine;
        public static DbAdapter engine { get { return _engine; } }
        private static object locker = new object();

        /// <summary>
        /// Приложение должно стать началом сервиса базы данных. При запкске, приложение загружает базу данных и начинает "реагировать" на запросы.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Start DBConsoleApp");
            // Сначала прочитаем конфигуратор и создадим базу данных на основе указанных кассет
            _path = "../../../"; 

            _config = XElement.Load(_path + "config.xml");
            string connectionstring = _config.Element("database").Attribute("connectionstring").Value;
            // Инициируем движок
            storage = new DStorage();
            storage.Init(_config);
            storage.turlog = (string line) =>
            {
                lock (locker)
                {
                    try
                    {
                        var saver = new System.IO.StreamWriter(_path + "logs/turlog.txt", true, System.Text.Encoding.UTF8);
                        saver.WriteLine(DateTime.Now.ToString("s") + " " + line);
                        saver.Close();
                    }
                    catch (Exception)
                    {
                        //LogFile.WriteLine("Err in buildlog writing: " + ex.Message);
                    }
                }
            };
            storage.turlog("DBConsoleApp initiating... path=" + _path);

            _engine = new XmlDbAdapter();
            storage.InitAdapter(_engine);

            // Присоединимся к кассетам через список из конфигуратора (это надо перенести в загрузочную часть) 
            try
            {
                storage.LoadFromCassettesExpress();
            }
            catch (Exception ex)
            {
                storage.turlog("Error while OpenArchive initiating: " + ex.Message);
                return;
            }

            // Проверим работу
            var q = _engine.SearchByName("марчук");
            foreach (var e in q)
            {
                Console.WriteLine(e.ToString());
            }
            // Теперь надо поставить слушателя и начать обрабатывать послания


        }
    }
}
