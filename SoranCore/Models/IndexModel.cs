using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;


namespace SoranCore.Models
{
    public class IndexModel
    {
        private string sdirection = "person";
        public string TabDirection { get { return sdirection; } set { sdirection = value; } }
        // Поиск дает две строки: id и name
        private IEnumerable<object[]> _sresults = null;
        public IEnumerable<object[]> SearchResults { get { return _sresults; } set { _sresults = value; } }
        // Портрет состоит из идентификатора, типа, множества полей 
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
        public string Uri { get; set; }

        // Главное поле
        public XElement XRecord { get; set; }

        // Множество форматов
        private static Dictionary<string, XElement> _formats = null;
        public Dictionary<string, XElement> Formats { get { return _formats; } set { _formats = value; } }

        // Конструктор иногда вычисляет
        public IndexModel()
        {
            if (_formats == null)
            {
                XElement xformats = new XElement("formats", 
                    new XElement("record", new XAttribute("type", "http://fogid.net/o/person"), 
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                            new XElement("record",
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/has-title"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "degree")),
                            null),
                        null),
                    null));

                _formats = xformats.Elements("record").ToDictionary<XElement, string, XElement>(x => x.Attribute("type").Value, x => x);
            }
        }

    }
}
