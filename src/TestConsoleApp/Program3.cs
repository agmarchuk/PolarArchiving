using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace TestConsoleApp
{
    partial class Program
    {
        public static void Main3()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            System.Random rnd = new Random();
            string path = "D:/Home/data/Databases/";
            int fnom = 0;
            Func<Stream> GenStream = () => File.Open(path + (fnom++), FileMode.OpenOrCreate);
            Console.WriteLine("Start TestConsoleApp (Main32)");
            TripleRecordStore store = new TripleRecordStore(GenStream, path);

            store.Preload = new string[] {
                "http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
                , "http://fogid.net/o/name"
                , "http://fogid.net/o/age"
                , "http://fogid.net/o/person"
            };


            int npersons = 1_000_000;

            bool toload = true;
            if (toload)
            {
                sw.Restart();
                store.Clear();
                var query = Enumerable.Range(0, npersons).Select(i => npersons - i - 1)
                    .Select(c => new object[] { -c-1, // Это "прямой" код не требующий кодирования через таблицу
                        new object[] { new object[] { 0, 3 } },
                        new object[] { new object[] { 1, "" + c }, new object[] { 2, "" + 33 } }
                    })
                    .ToArray()
                    ;
                store.Load(query);
                store.Build();
                sw.Stop();
                Console.WriteLine($"Load ok. duration={sw.ElapsedMilliseconds}");
            }

            // Проверка
            int code = -(npersons * 2 / 3) - 1;
            var query1 = store.GetRecord(code);
            Console.WriteLine(store.ToTT(query1));

            // Скорость выполнения запросов
            int nprobe = 10000;
            sw.Restart();
            for (int i = 0; i<nprobe; i++)
            {
                int c = -1 - (rnd.Next(npersons));
                var ob = store.GetRecord(code);
            }
            sw.Stop();
            Console.WriteLine($"{nprobe} GetRecord() ok. duration={sw.ElapsedMilliseconds}");

        }
    }
}
