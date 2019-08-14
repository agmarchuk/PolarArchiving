using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Polar.DB;

namespace TestDataGenerators
{
    public partial class Program
    {
        static void Main()
        {
            Main1();
        }
        static void Main1()
        {
            string root = "";
            Console.WriteLine("Start TestDataGenerators");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int npersons = 10;
            Phototeka.PhototekaRecordFlow rflow = new Phototeka.PhototekaRecordFlow(npersons);
            Console.WriteLine(rflow.GenerateAll().Count());
            sw.Stop();
            Console.WriteLine($"duration={sw.ElapsedMilliseconds}");

            PType tp_Arc = new PTypeUnion(
                new NamedType("field", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("value", new PType(PTypeEnumeration.sstring)))),
                new NamedType("text", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("value", new PType(PTypeEnumeration.sstring)),
                    new NamedType("lang", new PType(PTypeEnumeration.sstring)))),
                new NamedType("direct", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("resource", new PType(PTypeEnumeration.sstring))))
                );
            PType tp_Record = new PTypeRecord(
                new NamedType("about", new PType(PTypeEnumeration.sstring)),
                new NamedType("typ", new PType(PTypeEnumeration.sstring)),
                new NamedType("arcs", new PTypeSequence(tp_Arc))
                );

            // string binpath = root+"phototeka.bin";
            // sw.Restart();
            // rflow.SaveFlowToSequence(rflow.GenerateAll(), binpath);
            // sw.Stop();
            // Console.WriteLine($"duration={sw.ElapsedMilliseconds}");

            // sw.Restart();
            // UniversalSequenceBase sequ = new UniversalSequenceBase(tp_Record,
            //     File.Open(binpath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
            // sequ.Scan((off, obj) => true);
            // sw.Stop();
            // Console.WriteLine($"Scanning... duration={sw.ElapsedMilliseconds}");

            //sw.Restart();
            //rflow.SaveFlowToXml(rflow.GenerateAll(), root + "phototeka.xml");
            //sw.Stop();
            //Console.WriteLine($"duration={sw.ElapsedMilliseconds}");

            string filexmlpath = root + "phototeka.fogx";

            FileStream filexml = File.Open(filexmlpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            sw.Restart();
            rflow.SaveFlowToXElement(rflow.GenerateAll(), filexml);
            sw.Stop();
            Console.WriteLine($"duration={sw.ElapsedMilliseconds}");
            filexml.Close();

            //sw.Restart();root + 
            //XElement db1 = XElement.Load(filexmlpath);
            //sw.Stop();
            //Console.WriteLine($"Scanning... duration={sw.ElapsedMilliseconds}");

            string ptpath = "phototeka.fogp";

            FileStream ptextfile = File.Open(ptpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            sw.Restart();
            rflow.SaveFlowToPolarText(rflow.GenerateAll(), ptextfile);
            sw.Stop();
            Console.WriteLine($"Loading and writing... duration={sw.ElapsedMilliseconds}");
            ptextfile.Close();

            ptextfile = File.Open(ptpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            sw.Restart();
            TextReader tr = new StreamReader(ptextfile);
            int nrecs = TextFlow.DeserializeSequenseToFlow(tr, tp_Record).Count();
            Console.WriteLine($"{nrecs} records read");
            sw.Stop();
            Console.WriteLine($"Scanning... duration={sw.ElapsedMilliseconds}");
        }
    }
}
