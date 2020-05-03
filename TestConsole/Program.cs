using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OAData;

namespace TestConsole
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Main4(args);
        }
        static void Main1(string[] args)
        {
            Console.WriteLine("Start Test Console.");
            OAData.OADB.Init(@"D:\Home\dev2019\PolarArchiving\TestConsole\");

            //XElement xrecord;
            //IEnumerable<XElement> seq = SearchMarchuk();

            GetMarchuk();
            SearchMarchuk();
            Console.WriteLine("--------");


            //OADB.Load();

            //GetMarchuk();
            //SearchMarchuk();
            //Console.WriteLine("--------");

            OADB.Reload();

            GetMarchuk();
            SearchMarchuk();
            Console.WriteLine("--------");
        }

        private static void GetMarchuk()
        {
            XElement xrecord = OADB.GetItemByIdBasic("syp2001-p-marchuk_a", false);
            if (xrecord != null) Console.WriteLine(xrecord.ToString());
        }

        private static void SearchMarchuk()
        {
            IEnumerable<XElement> seq = OADB.SearchByName("марчук").ToArray();
            foreach (var x in seq)
            {
                Console.WriteLine(x.ToString());
            }
        }
    }
}
