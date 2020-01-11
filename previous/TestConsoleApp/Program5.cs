using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

using Polar.TripleStore;


namespace TestConsoleApp
{
    partial class Program
    {
        public static void Main5()
        {
            Console.WriteLine("Start Main5");
            string path = "C:/Home/data/Databases/";

            TripleRecordStoreAdapter adapter = new TripleRecordStoreAdapter();
            adapter.Init("trs:C:/Home/data/Databases/");
            bool toload = true;
            if (toload)
            {
                adapter.StartFillDb(null);
                adapter.LoadFromCassettesExpress(
                    new string[] { @"C:\home\syp\syp_cassettes\SypCassete/meta/SypCassete_current.fog" }, null, null);
                adapter.FinishFillDb(null);
            }

            var query = adapter.SearchByName("марчук");

            foreach (XElement rec in query)
            {
                Console.WriteLine(rec.ToString());
            }

            var per = adapter.GetItemByIdBasic("syp2001-p-marchuk_a", true);
            Console.WriteLine(per.ToString());

            var prs2 = adapter.GetItemById("syp2001-p-marchuk_a", new XElement("record",
                new XAttribute("type", "http://fogid.net/o/person"),
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                null));
            Console.WriteLine(prs2.ToString());
        }
    }
}
