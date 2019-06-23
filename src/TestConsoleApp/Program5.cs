using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Polar.TripleStore;


namespace TestConsoleApp
{
    partial class Program
    {
        public static void Main5()
        {
            Console.WriteLine("Start Main5");
            string path = "D:/Home/data/Databases1/";

            TripleRecordStoreAdapter adapter = new TripleRecordStoreAdapter();
            adapter.Init("trs:D:/Home/data/Databases1/");
            adapter.StartFillDb(null);
            adapter.LoadFromCassettesExpress(new string[] { @"D:\Home\FactographProjects\syp_cassettes\SypCassete/meta/SypCassete_current.fog" }, null, null);
            adapter.FinishFillDb(null);


        }
    }
}
