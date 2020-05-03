using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace TestConsole
{
    partial class Program
    {
        public static void Main4(string[] args)
        {
            Console.WriteLine("Start Check Pdfs");
            string dbin = @"D:\Home\FactographProjects\PA\newspaper\meta\newspaper_current.fog";
            string pdf_path = @"D:\Home\";
            string path_out = @"D:\Home\FactographProjects\PA\newspaper\originals\";
            //string fout = @"D:\Home\FactographProjects\PA\newspaper\meta\newspaper_current.fog";
            XElement xin = XElement.Load(dbin);

            //var query = xin.Elements("document");

            foreach (XElement xel in xin.Elements("document"))
            {
                XElement iisstore = xel.Element("iisstore");
                if (iisstore == null) continue;
                string documenttype = iisstore.Attribute("documenttype")?.Value;
                if (documenttype != "scanned/dz") continue;
                string uri = iisstore.Attribute("uri")?.Value;
                string name = xel.Element("name").Value;
            }

        }
    }
}
