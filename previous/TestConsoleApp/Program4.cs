using System;
using System.Collections.Generic;
using System.Text;

namespace TestConsoleApp
{
    partial class Program
    {
        public static void Main4()
        {
            Console.WriteLine("Start Main4");
            string path = "../../../";
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
