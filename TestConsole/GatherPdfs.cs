using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TestConsole
{
    partial class Program
    {
        public static void Main4(string[] args)
        {
            Console.WriteLine("Start Gather Pdfs");
            string pdf_path = @"D:\FactographProjects\NVS\";
            //string path_out = @"D:\FactographProjects\newspaper_\originals\";
            //string fout = @"D:\Home\FactographProjects\PA\newspaper\meta\newspaper_current.fog";

            Dictionary<string, string> did = new Dictionary<string, string>();
            Dictionary<string, string> dfi = new Dictionary<string, string>();
            Dictionary<string, string> dur = new Dictionary<string, string>();


            DirectoryInfo di = new DirectoryInfo(pdf_path);
            FileInfo[] fls = di.GetDirectories()
                .SelectMany(d1 => d1.GetDirectories())
                .SelectMany(d2 => d2.GetFiles("*.pdf"))
                .ToArray()
                ;
            Console.WriteLine($"nfiles={fls.Count()}");

            string[] dbins = 
            {
                @"D:\FactographProjects\newspaper\meta\newspaper_current.fog",
                @"D:\FactographProjects\newspaper adds\meta\newspaper adds_current.fog"
            };

            string[] specialnames = 
            {"спецвыпуск", "приложение №1", "приложение№2", "спецвыпуск15ноября", "спецвыпуск22ноября",
            "депутатский курьер_05",
            "депутатский курьер_06",
            "депутатский курьер_08",
            "депутатский курьер_09",
            "депутатский курьер_10",
            "депутатский курьер_11",
            "депутатский курьер_12",
            };
            string[] specialfnames = 
            {"СВ)", "приложение-1", "приложение-2", "спецвыпуск_15_ноября", "спецвыпуск_22_ноября",
            "ДК_05)",
            "ДК_06)",
            "ДК_08)",
            "ДК_09)",
            "ДК_10)",
            "ДК_11)",
            "ДК_12)",
            };

            foreach (string dbin in dbins)
            {
                XElement xin = XElement.Load(dbin);
                int idoc = 0;
                foreach (XElement xel in xin.Elements("document"))
                {
                    string id = xel.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    XElement iisstore = xel.Element("iisstore");
                    if (iisstore == null) continue;
                    string documenttype = iisstore.Attribute("documenttype")?.Value;
                    if (documenttype != "scanned/dz") continue;
                    idoc++;
                    string uri = iisstore.Attribute("uri")?.Value;
                    string name = xel.Element("name").Value;

                    // Попытка выявить специальное имя
                    int pos = 0;
                    for (; pos < specialnames.Length; pos++) {if (specialnames[pos]==name) break;}
                    
                    if (pos < specialnames.Length) name = specialfnames[pos]; 
                    else if (name.Length == 1) name = "0" + name;
                    else if (name.Length == 3 && name[0] == '0') name = name.Substring(1);
                    else if (name.IndexOf('-') > 0)
                    {
                        //name = name.Substring(1);
                        if (name.IndexOf('-') == 3) 
                        {
                            string[] parts = name.Split('-');
                            string first = parts[0];
                            if (first.Length == 3) first = first.Substring(1);
                            string second = parts[1];
                            if (second.Length == 1) second = "0" + second;
                            name = first + "-" + second;
                        }
                    }
                    //else if (name.StartsWith("депутатский курьер_")) name = name.Substring("депутатский курьер_".Length);

                    string fromdate = xel.Element("from-date")?.Value;
                    //bool toprint = true;


                    // строю имя файла
                    string fname = fromdate + "_" + name + ".pdf";
                    // ищу файл
                    var query = fls.Where(f => f.Name == fname)
                        .Count();
                    if (query != 1)
                    {
                        Console.WriteLine($"No FILE: {fname} => {fromdate} {name}");
                    }

                    // Заполняю словари
                    did.Add(id, fname);

                    if (!dfi.ContainsKey(fname))
                    {
                        dfi.Add(fname, id);
                    }
                    else
                    {
                        Console.WriteLine($"Err: double fname {fname} for id={id} stored {dfi[fname]}");
                    }
                    
                    dur.Add(uri, id);
                }
                Console.WriteLine($"ndocs={idoc}");
            }
            Console.WriteLine($"{did.Count} did elements");
            Console.WriteLine($"{dfi.Count} dfi elements");
        }
    }
}
