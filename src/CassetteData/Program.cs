using System;
using System.Xml.Linq;

namespace CassetteData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start CassetteData module");
            CassetteIntegration ci = new CassetteIntegration(new XElement("config"), false);

            Console.WriteLine("exit?");
            Console.ReadKey();
        }
    }
}
