using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TestConsole
{
    partial class Program
    {
        public static void Main3(string[] args)
        {
            Console.WriteLine("Start Correct position");
            string pathin = @"D:\Home\FactographProjects\PA_Users\originals\0001\";
            string pathout = @"D:\Home\FactographProjects\PA\PA_Users\originals\0001\";
            string[] fnames = Enumerable.Range(1, 11).Select(i => (i < 10 ? "000" : "00") + i + ".fog")
                .ToArray();
            NumberStyles styles = NumberStyles.Float | NumberStyles.Any;
            IFormatProvider provider = CultureInfo.InvariantCulture;

            foreach (string fname in fnames)
            {
                bool corrected = false;
                XElement xin = XElement.Load(pathin + fname);
                //XElement xout = XElement.Parse(@"<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' dbid='newspaper_current' uri='iiss://newspaper@iis.nsk.su/meta' />");
                var xout = new XElement(xin.Name, xin.Attributes());

                foreach(XElement el in xin.Elements())
                {
                    XElement clone = new XElement(el);
                    if (clone.Name == "reflection")
                    {
                        XElement position = clone.Element("position");
                        if (position != null)
                        {
                            var a = position.Attribute("x").Value;
                            string a1 = a.Substring(0, System.Math.Max(a.IndexOf('.'), a.IndexOf(',')));
                            //var b = Double.Parse(a, styles, provider);
                            //var c = (int)b;
                            int c = Int32.Parse(a1);
                            position.Value = "" + (c + 1);
                            //position.Value = "" + ((int)Double.Parse(position.Attribute("x").Value) + 1);
                        }
                    }
                    xout.Add(clone);
                }

                xout.Save(pathout + fname);
            }
        }
    }
}
