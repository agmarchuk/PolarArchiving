using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TestConsoleApp");
            string path = "./";
            Turgunda6.SObjects.Init(path);
            var q = Turgunda6.SObjects.SearchByName("марчук");
            foreach (var e in q)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
