using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenArchiveClient
{
    public class SObs : DataSource
    {
        private static bool _initiated = false;
        public static bool Initiated { get { return _initiated; } }
        private static string _path;
        private static XElement _config;

        public static void Init() { Init(_path); }
        public static void Init(string path)
        {
            _path = path;
            if (_path[_path.Length - 1] != '/' && _path[_path.Length - 1] != '\\') _path = _path + "/";
            string configpath = _path + "wwwroot/config.xml";
            if (!System.IO.File.Exists(configpath))
            {
                System.IO.File.Copy(_path + "wwwroot/config0.xml", configpath);
            }
            _config = XElement.Load(configpath);

            OAData.OADB.Init(_path + "wwwroot/");

            if (OAData.OADB.adapter != null) CalculateConstants();
            _initiated = true;
        }

        public static string funds_id = null;
        public static XElement xtree;
        public static void CalculateConstants()
        {
            string funds_name = "Фонды";
            XElement rec = SObs.SearchByName(funds_name)
                .FirstOrDefault(r =>
                {
                    if (r.Attribute("type").Value != "http://fogid.net/o/collection") return false;
                    var na = r.Elements("field")
                        .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name" && f.Value == funds_name);
                    if (na == null) return false;
                    return true;
                });
            if (rec != null)
            {
                funds_id = rec.Attribute("id").Value;
                //Сделаем дерево. Будем вкладывать коллекции и документы в коллекции более высокого уровня

                try
                {
                    xtree = XElement.Load(_path + "wwwroot/xtree.xml");
                }
                catch (Exception)
                {
                    xtree = new XElement(rec);
                    // Какая-то ошибка
                    //xtree.Add(CollectChilds(rec.Attribute("id").Value));
                    //xtree.Save(_path + "wwwroot/xtree.xml");
                }
            }
        }


        private static XElement formattoparentcollection =
            new XElement("record",
                new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                    new XElement("record",
                        new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                            new XElement("record", new XAttribute("type", "http://fogid.net/o/collection"),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))));
        private static XElement formattochildfromcollection =
            new XElement("record",
                new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                    new XElement("record",
                        new XElement("direct", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))));
        public static IEnumerable<XElement> GetDocsFromCollection(string id)
        {
            XElement rec = xtree.DescendantsAndSelf("record")
                .FirstOrDefault(x => x.Attribute("id").Value == id);
            if (rec != null)
            {
                return rec.DescendantsAndSelf("record")
                    .Select(r => new XElement("record", new XAttribute("id", r.Attribute("id").Value)));
            }
            return Enumerable.Empty<XElement>();
        }
        private static IEnumerable<XElement> CollectChilds(string id)
        {
            XElement xt = SObs.GetItemById(id, formattochildfromcollection);
            var query = xt.Elements("inverse")
                .Where(xi => xi.Attribute("prop").Value == "http://fogid.net/o/in-collection")
                .Select(xi => xi.Element("record").Element("direct").Element("record"));
            return query
                .Select(ch =>
                {
                    if (ch.Attribute("type").Value == "http://fogid.net/o/document")
                    {
                        return new XElement(ch);
                    }
                    else if (ch.Attribute("type").Value == "http://fogid.net/o/collection")
                    {
                        XElement xx = new XElement(ch);
                        xx.Add(CollectChilds(ch.Attribute("id").Value));
                        return xx;
                    }
                    else return null;
                });
        }
        public static IEnumerable<string> CollectAllChildDocumentIds(string id)
        {
            XElement xt = SObs.GetItemById(id, formattochildfromcollection);
            var query = xt.Elements("inverse")
                .Where(xi => xi.Attribute("prop").Value == "http://fogid.net/o/in-collection")
                .SelectMany(xi =>
                {
                    XElement child = xi.Element("record")?.Element("direct")?.Element("record");
                    if (child == null) { return Enumerable.Empty<string>(); }
                    if (child.Attribute("type").Value == "http://fogid.net/o/collection") return CollectAllChildDocumentIds(child.Attribute("id").Value);
                    else if (child.Attribute("type").Value == "http://fogid.net/o/document") return new string[] { child.Attribute("id").Value };
                    else return Enumerable.Empty<string>();
                });
            return query;
        }


        // Статические методы выявления узлов и полей
        public static string GetDates(XElement item)
        {
            var fd_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/from-date");
            var td_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/to-date");
            string res = (fd_el == null ? "" : fd_el.Value) + (td_el == null || string.IsNullOrEmpty(td_el.Value) ? "" : "—" + td_el.Value);
            return res;
        }
        public static string GetField(XElement item, string prop)
        {
            XElement el = null;
            try
            {
                el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == prop);
            }
            catch (Exception)
            {
                Console.WriteLine("Exception");
            }
            return el == null ? "" : el.Value;
        }
        public static IEnumerable<XElement> GetCollectionPath(string id)
        {
            //return _getCollectionPath(id);
            System.Collections.Generic.Stack<XElement> stack = new Stack<XElement>();
            XElement node = GetItemByIdBasic(id, false);
            if (node == null) return Enumerable.Empty<XElement>();
            stack.Push(node);
            bool ok = GetCP(id, stack);
            if (!ok) return Enumerable.Empty<XElement>();
            int n = stack.Count();
            // Уберем первый и последний
            var query = stack.Skip(1).Take(n - 2).ToArray();
            return query;
        }
        // В стеке накоплены элементы пути, следующие за id. Последним является узел с id
        private static bool GetCP(string id, Stack<XElement> stack)
        {
            if (id == funds_id) return true;
            XElement tree = GetItemById(id, formattoparentcollection);
            if (tree == null) return false;
            foreach (var n1 in tree.Elements("inverse"))
            {
                var n2 = n1.Element("record"); if (n2 == null) return false;
                var n3 = n2.Element("direct"); if (n3 == null) return false;
                var node = n3.Element("record"); if (node == null) return false;
                string nid = node.Attribute("id").Value;
                stack.Push(node);
                bool ok = GetCP(nid, stack);
                if (ok) return true;
                stack.Pop();
            }
            return false;
        }

    }
}
