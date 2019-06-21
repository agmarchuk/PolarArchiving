using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TestConsoleApp
{
    partial class Program
    {
        public static void Main()
        {
            Main2();
        }
        static void Main1()
        {
            //var v = CodePagesEncodingProvider.Instance;
            //Encoding.RegisterProvider(v);
            ////var enc = System.Text.Encoding.GetEncoding("windows-1251");
            ////TextReader tr = new StreamReader(new System.IO.FileStream(
            ////    @"E:\FactographProjects\PA_cassettes\PA_database201012\originals\0001\0001.fog", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read),
            ////    enc);
            ////XmlReader reader = XmlReader.Create(new System.IO.FileStream(
            ////    @"E:\FactographProjects\PA_cassettes\PA_database201012\originals\0001\0001.fog", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read),
            ////    new XmlReaderSettings() {  });
            ////XElement doc = XElement.Load(reader, LoadOptions.None);
            //XElement doc = XElement.Load(@"E:\FactographProjects\PA_cassettes\PA_database201012\originals\0001\0001.fog");
            //Console.WriteLine(doc.ToString());
            //return;

            Console.WriteLine("Start TestConsoleApp");
            string path = "./";
            SObjects.Init(path);
            var q = SObjects.SearchByName("марчук");
            foreach (var e in q)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadKey();
        }
    }
}
