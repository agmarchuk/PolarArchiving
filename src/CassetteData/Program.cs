using System;
using System.Xml.Linq;

namespace CassetteData
{
    class Program
    {
        static void Main0(string[] args)
        {
            Console.WriteLine("Start CassetteData module");
            XElement xconfig = new XElement("config",
                new XElement("LoadCassette", @"D:\Home\FactographProjects\Cassette_test20180311"),
                null);
            CassetteIntegration ci = new CassetteIntegration(xconfig, false);

            var res = ci.SearchByName("марчук");
            foreach (var r in res)
            {
                Console.WriteLine(r.ToString());
            }
            Console.WriteLine();

            var res2 = ci.GetItemByIdSpecial("syp2001-p-marchuk_a");
            Console.WriteLine(res2.ToString());
            Console.WriteLine();

            var res3 = ci.GetItemByIdBasic("syp2001-p-marchuk_a", true);
            Console.WriteLine(res3.ToString());
            Console.WriteLine();

            XElement format = new XElement("record",
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/participant"),
                    new XElement("record",
                        new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-org"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
                null);
            var res4 = ci.GetItemById("syp2001-p-marchuk_a", format);
            Console.WriteLine(res4.ToString());
            Console.WriteLine();

            //Console.WriteLine("exit?");
            //Console.ReadKey();
        }
    }
}
