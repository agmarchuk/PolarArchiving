using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OAData;

namespace Soran1957Try20210328
{
    public class DataSource
    {
        private static string path;
        public static string look = "novalue";
        public static XElement xlook = new XElement("div", "no xml element");
        public static Dictionary<string, XElement> formatsDictionary;
        public static void Connect(string pth)
        {
            path = pth;
            OAData.OADB.Init(path);
            formatsDictionary = xformats.Elements("record").ToDictionary<XElement, string, XElement>(x => x.Attribute("type").Value, x => x);

        }
        public static string GetField(XElement rec, string prop)
        {
            //string lang = null;
            string res = null;
            foreach (XElement f in rec.Elements("field"))
            {
                string p = f.Attribute("prop").Value;
                if (p != prop) continue;
                XAttribute xlang = f.Attribute("{http://www.w3.org/XML/1998/namespace}lang");
                res = f.Value;
                if (xlang?.Value == "ru") { break; }
            }
            return res;
        }
        private static string[] months = new[] { "янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек" };
        public static string DatePrinted(string date)
        {
            if (date == null) return null;
            string[] split = date.Split('-');
            string str = split[0];
            if (split.Length > 1)
            {
                int month;
                if (Int32.TryParse(split[1], out month) && month > 0 && month <= 12)
                {
                    str += months[month - 1];
                    if (split.Length > 2) str += split[2].Substring(0, 2);
                }
            }
            return str;
        }

        private static XElement xformats = new XElement("formats",
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

    }
}
