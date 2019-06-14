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
                , "http://fogid.net/o/photo-doc"
                , "http://fogid.net/o/reflection"
                , "http://fogid.net/o/reflected"
                , "http://fogid.net/o/in-doc"
            };


            int npersons = 10_000_000;
            int nphotos = npersons * 2;
            int nreflections = npersons * 6;

            bool toload = false;
            if (toload)
            {
                sw.Restart();
                store.Clear();
                var persons = Enumerable.Range(0, npersons).Select(i => npersons - i - 1)
                    .Select(c => new object[] { -c-1, // Это "прямой" код не требующий кодирования через таблицу
                        new object[] { new object[] { 0, 3 } },
                        new object[] { new object[] { 1, "p" + c }, new object[] { 2, "" + 33 } }
                    });
                var photos = Enumerable.Range(0, nphotos).Select(i => nphotos - i - 1)
                    .Select(c => new object[] { -(c+npersons)-1, // Это "прямой" код не требующий кодирования через таблицу
                        new object[] { new object[] { 0, 4 } },
                        new object[] { new object[] { 1, "f" + c } }
                    });
                var reflections = Enumerable.Range(0, nreflections).Select(i => nreflections - i - 1)
                    .Select(c => new object[] { -(c+3*npersons)-1, // Это "прямой" код не требующий кодирования через таблицу
                        new object[] { new object[] { 0, 5 },
                            new object[] { 6, -1 - (rnd.Next(npersons)) },
                            new object[] { 7, -1 - (rnd.Next(nphotos) + npersons) } },
                        new object[] { }
                    });
                store.Load(persons.Concat(photos).Concat(reflections));
                store.Build();
                sw.Stop();
                Console.WriteLine($"Load ok. duration={sw.ElapsedMilliseconds}");
            }
            else
            {
                sw.Restart();
                store.Refresh();
                sw.Stop();
                Console.WriteLine($"Refresh for Phototeka {npersons} persons. Duration={sw.ElapsedMilliseconds}");
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
                var ob = store.GetRecord(c);
            }
            sw.Stop();
            Console.WriteLine($"{nprobe} GetRecord() ok. duration={sw.ElapsedMilliseconds}");

            // Надо найти все фотографии, в которых отражается персона с выбранным (случайно) кодом. 
            sw.Restart();
            int total = 0;
            nprobe = 1000;
            for (int i = 0; i < nprobe; i++)
            {
                int c = -1 - (rnd.Next(npersons));
                var query2 = store.GetRefers(c).Cast<object[]>()
                    .Select(ob => ((object[])ob[1]).Cast<object[]>()
                        .First(dupl => (int)dupl[0] == 7))
                    .Select(bb => store.GetRecord((int)bb[1]));
                total += query2.Count();
            }
            sw.Stop();
            Console.WriteLine($"{nprobe} persons for photos ok. duration={sw.ElapsedMilliseconds} total={total}");

        }
    }
}
