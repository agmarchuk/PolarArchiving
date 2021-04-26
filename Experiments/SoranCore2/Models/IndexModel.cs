using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;


namespace SoranCore2.Models
{
    public class IndexModel
    {
        // Для отладки:
        public string look = "";
        public string ir = null; // база массива отражений документов через отражения
        public string ic = null; // база массива отражений документов через членство в коллекциях. Бывает либо то, либо другое

        private string sdirection = "person";
        public int page_length = 40;
        private int _pg = 0;
        public int Pg {  get { return _pg;  } set { _pg = value; } }
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
        public string Docmetainfo { get; set; }

        // Главное поле
        public XElement XRecord { get; set; }

        // ============= То, что попало в вьюер, а теперь извлечено
        public Tuple<string, string>[] titles = null;
        public string portraitphotouri = null;
        public XElement[] reflected_reflections = null;
        public XElement[] work = null;
        public XElement[] parties = null;
        public XElement[] firstfaces = null;
        public XElement[] allparticipants = null;
        public Tuple<string, string, string>[] naming = null;
        public XElement[] indocreflected = null;
        public XElement[] incollectionmember = null;

        public Func<string, string> ImgSrc = (string tp) => tp == "http://fogid.net/o/document" ? "~/img/document_s.jpg" :
            (tp == "http://fogid.net/o/photo-doc" ? "~/Docs/GetPhoto?s=small&u=" :
            (tp == "http://fogid.net/o/video" ? "~/img/video_s.jpg" :
            (tp == "http://fogid.net/o/audio" ? "~/img/audio_s.jpg" :
            (tp == "http://fogid.net/o/org-sys" ? "~/img/org_s.jpg" :
            (tp == "http://fogid.net/o/collection" ? "~/img/collection_s.jpg" : "~/img/document_s.jpg")))));

        public string[] docidarr = null; // массив идентификаторов документов в reflected_reflections 
        public void BuildPortrait()
        {
            reflected_reflections = this.XRecord.Elements("inverse")
               .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/reflected")
               .Select(i => i.Element("record"))
               .ToArray();

            if (reflected_reflections.Length > 0)
            {
                docidarr = reflected_reflections.Select(r => r.Element("direct")?.Element("record")?.Attribute("id")?.Value).ToArray();
            }

            portraitphotouri = reflected_reflections
                .Where(re => re.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/ground")?.Value == "portrait")
                .Select(re => re.Element("direct")?.Element("record"))
                .Where(r => r?.Attribute("type").Value == "http://fogid.net/o/photo-doc")
                .FirstOrDefault()
                ?.Elements("field")
                .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/uri")?.Value;

            titles = this.XRecord.Elements("inverse")
                .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/has-title")
                .Select(i => i.Element("record"))
                .Select<XElement, Tuple<string, string>>(r => Tuple.Create<string, string>(
                    r.Elements("field").FirstOrDefault(f => f.Attribute("prop")?.Value == "http://fogid.net/o/degree")?.Value,
                    r.Elements("field").FirstOrDefault(f => f.Attribute("prop")?.Value == "http://fogid.net/o/from-date")?.Value
                    ))
                .OrderBy(t => t.Item2)
                .ToArray();

            XElement[] participant_participation = this.XRecord.Elements("inverse")
               .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/participant")
               .Select(i => i.Element("record"))
               .ToArray();
            work = participant_participation
               .Where(r => r.Element("direct")?.Element("record")?.Elements("field")
                    .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/org-classification")?.Value == "organization")
               .ToArray();
            parties = participant_participation
               .Where(r => r.Element("direct")?.Element("record")?.Elements("field")
                    .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/org-classification")?.Value != "organization")
               .ToArray();


            naming = this.XRecord.Elements("inverse")
                .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/referred-sys")
                .Select(i => i.Element("record"))
                .Select<XElement, Tuple<string, string, string>>(r => Tuple.Create<string, string, string>(
                    r.Elements("field").FirstOrDefault(f => f.Attribute("prop")?.Value == "http://fogid.net/o/alias")?.Value,
                    r.Elements("field").FirstOrDefault(f => f.Attribute("prop")?.Value == "http://fogid.net/o/from-date")?.Value,
                    r.Elements("field").FirstOrDefault(f => f.Attribute("prop")?.Value == "http://fogid.net/o/to-date")?.Value
                    ))
                .OrderBy(t => t.Item2)
                .ToArray();
            var inorg_participation = this.XRecord.Elements("inverse")
               .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/in-org")
               .Select(i => i.Element("record"))
               .ToArray();
            firstfaces = inorg_participation
                .Where(r => StaticObjects.GetField(r, "http://fogid.net/o/role-classification") == "director")
                .ToArray();
            allparticipants = inorg_participation
                .Where(r => StaticObjects.GetField(r, "http://fogid.net/o/role-classification") != "director")
                .ToArray();
            inorg_participation = this.XRecord.Elements("inverse")
                .Select(i => i.Element("record"))
                .ToArray();
            indocreflected = this.XRecord.Elements("inverse")
                .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/in-doc")
                .Select(i => i.Element("record"))
                .ToArray();
            incollectionmember = this.XRecord.Elements("inverse")
                .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/in-collection")
                .Select(i => i.Element("record")?.Element("direct")?.Element("record"))
                .Where(r => r != null)
                .ToArray();
        }

        // =============

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
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                            new XElement("record",
                                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")))), 
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/participant"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role-classification")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-org"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/org-classification")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/has-title"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/degree")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            null)),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/referred-sys"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/alias")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                                null)),
                        null),
                    new XElement("record", new XAttribute("type", "http://fogid.net/o/org-sys"),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-org"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role-classification")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/participant"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/referred-sys"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/alias")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                                null)),
                        null),
                    new XElement("record", new XAttribute("type", "http://fogid.net/o/document"),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/reflected"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/position")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))),
                                null)),
                        null),
                    new XElement("record", new XAttribute("type", "http://fogid.net/o/collection"),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                                        null)))),
                        null),
                    null);

                _formats = xformats.Elements("record").ToDictionary<XElement, string, XElement>(x => x.Attribute("type").Value, x => x);
            }
        }

    }
}
