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
        public static void Main2(string[] args)
        {
            Console.WriteLine("Start DeepZoom 2 RDF tranformation");
            string fin = @"D:\Home\FactographProjects\newspaper\meta\newspaper_current.fog";
            string fout = @"D:\Home\FactographProjects\PA\newspaper\meta\newspaper_current.fog";
            XElement xin = XElement.Load(fin);
            // <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" dbid="newspaper_current" uri="iiss://newspaper@iis.nsk.su/meta">
            XElement xout = XElement.Parse(@"<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' dbid='newspaper_current' uri='iiss://newspaper@iis.nsk.su/meta' />");
            
            // Вставим все элементы кроме документов и страниц
            xout.Add(xin.Elements()
                .Select(x =>
                {
                    if (x.Name == "document" || x.Name == "scanned-page") return null;
                    return new XElement(x);
                }));

            // Выделим документы, уложим их в словарь
            Dictionary<string, XElement> docs = new Dictionary<string, XElement>();
            foreach (XElement x in xin.Elements("document"))
            {
                string id = x.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                if (docs.ContainsKey(id))
                {
                    XElement existing_doc = docs[id];
                    if (!existing_doc.Elements("from-date").Any())
                    {
                        docs.Remove(id);
                        docs.Add(id, x);
                    }
                }
                else
                {
                    docs.Add(id, x);
                }
            }

            // Выделим страницы, сгруппируем по документам, уложим их в словарь
            Dictionary<string, XElement[]>  pages = xin.Elements("scanned-page")
                .GroupBy(p => p.Element("scanned-document").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value)
                .ToDictionary(g => g.Key, g => g.ToArray());

            // Добавим переработанные страницы
            var query = docs
                .Select(x =>
                {
                    string id = x.Key;
                    XElement d = x.Value;
                    XElement[] pgs = pages.ContainsKey(id) ? pages[id] : new XElement[0];
                    // Пока количество страниц будет вычисляться каунтером
                    int npgs = pgs.Count();
                    XElement iisstore = d.Element("iisstore");
                    iisstore.SetAttributeValue("documenttype", "application/pdf");
                    iisstore.Add(new XAttribute("npages", npgs));
                    string year = d.Element("from-date") == null ? "999" :d.Element("from-date").Value;
                    string number = d.Element("name").Value.Substring(1, 2);
                    string url = $"http://www.sbras.info/system/files/archive/archive1961-2009/{year}_{number}.pdf";
                    iisstore.Add(new XAttribute("url", url));
                    return new XElement("document",
                        new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", id),
                        new XElement(d.Element("name")),
                        new XElement("from-date", year),
                        new XElement(iisstore),
                        null);
                })
                .OrderBy(x =>x.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value);
            xout.Add(query);

            // вывод зафиксированных результатов
            var qu = docs.Select(x =>
            {
                var url = x.Value.Element("iisstore").Attribute("url").Value.Substring(60);
                return url;
            }).OrderBy(x => x);
            string y = "0000";
            int numb = 1;
            foreach (var q in qu)
            {
                string year = q.Substring(0, 4);
                string number = q.Substring(5, 2);
                //Console.WriteLine(q);
                if (year != y) { Console.WriteLine(q); numb = 1; }
                else { numb++; }
                y = year;
                Console.WriteLine("  " + q.Substring(4, 3) + " " + numb);
            }

            xout.Save(fout);
        }
    }
}
