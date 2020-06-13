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

            Func<string, string> Fromfile = (fn) => fls.First(f => f.Name == fn).FullName;

            string[] dbins = 
            {
                @"D:\FactographProjects\newspaper\meta\newspaper_current.fog",
                //@"D:\FactographProjects\newspaper adds\meta\newspaper adds_current.fog"
            };
            string cassette_out = @"D:\FactographProjects\PA_newspapers\newspaper\";
            string dbout = cassette_out + @"meta\newspaper_current.fog";

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

            XElement xout = null;
            foreach (string dbin in dbins)
            {
                XElement xin = XElement.Load(dbin);
                if (xout == null) xout = new XElement(xin.Name, xin.Attributes());
                int idoc = 0;
                bool collectionNVSadded = false;
                foreach (XElement xel in xin.Elements())
                {
                    string id = xel.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    if (xel.Name == "document")
                    {
                        // Не используем дополнительный фог-документ
                        if (id == "newspaper_0014_0780") continue;
                        XElement iisstore = xel.Element("iisstore");
                        if (iisstore == null) continue;
                        string documenttype = iisstore.Attribute("documenttype")?.Value;
                        if (documenttype != "scanned/dz") continue;
                        idoc++;
                        string uri = iisstore.Attribute("uri")?.Value;
                        XElement name_el = xel.Element("name");
                        string name = name_el.Value;
                        string suffix = name;

                        // Попытка выявить специальное имя
                        int pos = 0;
                        for (; pos < specialnames.Length; pos++) {if (specialnames[pos]==name) break;}
                        
                        if (pos < specialnames.Length) 
                        {
                            suffix = specialfnames[pos]; 
                        }
                        else
                        {
                            if (name.Length == 1) name = "0" + name;
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
                            suffix = name;
                        }

                        string fromdate = xel.Element("from-date")?.Value;

                        // строю имя файла
                        string fname = fromdate + "_" + suffix + ".pdf";
                        // ищу файл

                        var query = fls.Where(f => f.Name == fname)
                            .Count();
                        if (query != 1)
                        {
                            Console.WriteLine($"No FILE: {fname} => {fromdate} {suffix}");
                        }
                        string ff = Fromfile(fname);
                        Console.WriteLine($"FROM FILE: {ff}");

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
                        // Меняю имя в элементе
                        name_el.Value = name;
                        // Переписываю в базу данных
                        xout.Add(xel);

                        // ================ Работа с pdf файлами =============
                        // Теперь у меня в fname - имя файла, а в uri - место и новое имя файла
                        string[] uri_parts = uri.Split('/');
                        string folder_nom = uri_parts[uri_parts.Length - 2];
                        string file_nom =  uri_parts[uri_parts.Length - 1];
                        string nfname = cassette_out + "originals/" + folder_nom + "/" + file_nom  + ".pdf";
                        // Проверка на наличие
                        if (!File.Exists(nfname)) 
                        {
                            if (!Directory.Exists(cassette_out + "originals/" + folder_nom))
                            {
                                Directory.CreateDirectory(cassette_out + "originals/" + folder_nom);
                            }
                            string fff = Fromfile(fname);
                            File.Copy(fff, nfname);
                        }
                        //Console.WriteLine($"file: {fname} uri: {uri} nfname: {nfname}");

                        // ====================== OK ======================
                    }
                    else if (xel.Name == "collection-member")
                    {
                        // меняем <in-collection rdf:resource="newspaper_cassetteId" />
                        // на <in-collection rdf:resource="newspaperNVScollection" />
                        XElement incollection = xel.Element("in-collection");
                        if (incollection != null 
                            && incollection.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource")?.Value == "newspaper_cassetteId")
                        {
                            if (!collectionNVSadded)
                            {
                                xout.Add(
                                    new XElement("collection",
                                        new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", "newspaperNVScollection"),
                                        new XElement("name", "Газета: За науку в Сибири/Наука в Сибири")),
                                    new XElement("collection-member",
                                        new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", "nnewpaper_2249284"),
                                        new XElement("in-collection", 
                                            new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource", "newspaper_cassetteId")),
                                        new XElement("collection-item", 
                                            new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource", "newspaperNVScollection"))
                                        )
                                );
                                collectionNVSadded = true;
                            }
                            XAttribute att = incollection.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource");
                            att.Value = "newspaperNVScollection";
                        }
                        xout.Add(xel);
                    }
                    else if (xel.Name == "scanned-page")
                    {
                        // Не записываем
                    }
                    else
                    {
                        // Просто переписываем
                        xout.Add(xel);
                    }
                }
                Console.WriteLine($"ndocs={idoc}");
            }
            Console.WriteLine($"{did.Count} did elements");
            Console.WriteLine($"{dfi.Count} dfi elements");
            Console.WriteLine($"{dur.Count} dur elements");

            xout.Save(dbout);
        }
    }
}
