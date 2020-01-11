using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Polar.DB;

namespace Phototeka
{
    public class PhototekaRecordFlow
    {
        private int npersons, nphotos, nreflections;
        private Random rnd = new Random();
        private PType tp_Arc;
        private PType tp_Record;
        public PhototekaRecordFlow(int npersons)
        {
            this.npersons = npersons;
            nphotos = npersons * 2;
            nreflections = npersons * 6;
            tp_Arc = new PTypeUnion(
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
            tp_Record = new PTypeRecord(
                new NamedType("about", new PType(PTypeEnumeration.sstring)),
                new NamedType("typ", new PType(PTypeEnumeration.sstring)),
                new NamedType("arcs", new PTypeSequence(tp_Arc))
                );
        }
        // Объектное представление записей соответствует типу:
        // Record = {about: string, typ: string, arcs: [Arc]};
        // Arc =   
        //   field^{prop: string, value: string},
        //   text^{prop: string, value: string, lang: string},
        //   direct^{prop: string, resource: string};
        public IEnumerable<object> GeneratePersons()
        {
            var query = Enumerable.Range(0, npersons).Select(i => npersons - i - 1)
                    .Select(c => new object[] { "p"+c, "http://fogid.net/o/person",
                        new object[] {
                            new object[] { 1, new object[] { "http://fogid.net/o/name", "Пупкин" + c, "ru" }},
                            new object[] { 0, new object[] { "http://fogid.net/o/age", "" + 33 } } }
                    });
            return query;
        }
        public IEnumerable<object> GenerateFotos()
        {
            var query = Enumerable.Range(0, nphotos).Select(i => nphotos - i - 1)
                    .Select(c => new object[] { "f"+c, "http://fogid.net/o/photo-doc",
                        new object[] {
                            new object[] { 1, new object[] { "http://fogid.net/o/name", "Фотка" + c, "ru" }}
                            }
                    });
            return query;
        }
        public IEnumerable<object> GenerateReflections()
        {
            var query = Enumerable.Range(0, nreflections).Select(i => nreflections - i - 1)
                    .Select(c => new object[] { "r"+c, "http://fogid.net/o/reflection",
                        new object[] {
                            new object[] { 2, new object[] { "http://fogid.net/o/reflected", "p" + rnd.Next(npersons) }},
                            new object[] { 2, new object[] { "http://fogid.net/o/in-doc", "f" + rnd.Next(nphotos) }}
                            }
                    });
            return query;
        }
        public IEnumerable<object> GenerateAll()
        {
            return GeneratePersons().Concat(GenerateFotos()).Concat(GenerateReflections());
        }
        public void SaveFlowToSequence(IEnumerable<object> flow, string path)
        {
            var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            UniversalSequenceBase sequ = new UniversalSequenceBase(tp_Record, file);

            sequ.Clear();
            foreach (var ob in flow)
            {
                sequ.AppendElement(ob);
            }
            sequ.Flush();
            file.Close();
        }
        private string LocalName(string name)
        {
            int pos = name.LastIndexOf("/");
            string lname = name.Substring(pos + 1);
            return lname;
        }
        public void SaveFlowToXml(IEnumerable<object> flow, string path)
        {
            XmlDocument xdoc = new XmlDocument();

            XmlElement xdb = xdoc.CreateElement("db", "http://fogis.net/o/");
            XmlAttribute att = xdoc.CreateAttribute("xmlns:rdf");
            att.Value = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            xdb.Attributes.Append(att);
            xdoc.AppendChild(xdb);


            foreach (object[] ob in flow)
            {
                string about = (string)ob[0];
                string typ = (string)ob[1];
                object[] arcs = (object[])ob[2];
                XmlElement el = xdoc.CreateElement(LocalName(typ), "http://fogis.net/o/");
                var about_att = xdoc.CreateAttribute("rdf", "about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                about_att.Value = about;
                el.Attributes.Append(about_att);
                foreach (object[] pair in arcs)
                {
                    int tag = (int)pair[0];
                    object[] values = (object[])pair[1];
                    string prop = (string)values[0];
                    XmlElement sub = xdoc.CreateElement(LocalName(prop), "http://fogis.net/o/");
                    if (tag == 0) //field
                    {
                        sub.InnerText = (string)values[1];
                    }
                    else if (tag == 1) // text
                    {
                        sub.InnerText = (string)values[1];
                        var lang = xdoc.CreateAttribute("xml:lang");
                        lang.Value = "ru";
                        sub.Attributes.Append(lang);
                    }
                    else if (tag == 2) // direct
                    {
                        var resource_att = xdoc.CreateAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                        resource_att.Value = (string)values[1];
                        sub.Attributes.Append(resource_att);
                    }
                    el.AppendChild(sub);
                }
                xdb.AppendChild(el);

            }
            xdoc.Save(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        }
        private XName ToXName(string name)
        {
            int pos = name.LastIndexOf("/");
            string lname = name.Substring(pos + 1);
            return XName.Get(lname, name.Substring(0, pos + 1));
        }
        public void SaveFlowToXElement(IEnumerable<object> flow, Stream stream)
        {
            XElement db = XElement.Parse(
@"<?xml version='1.0'?>
<rdf:RDF xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' >
</rdf:RDF>"
                );
            //XElement db0 = new XElement("db",
            //    new XAttribute(XName.Get("xmlns", "rdf"), "http://www.w3.org/1999/02/22-rdf-syntax-ns#"));

            foreach (object[] ob in flow)
            {
                string about = (string)ob[0];
                string typ = (string)ob[1];
                object[] arcs = (object[])ob[2];
                XElement el = new XElement(ToXName(typ), new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", about));
                foreach (object[] pair in arcs)
                {
                    int tag = (int)pair[0];
                    object[] values = (object[])pair[1];
                    string prop = (string)values[0];
                    XElement sub = new XElement(ToXName(prop));
                    if (tag == 0) //field
                        sub.Add((string)values[1]);
                    else if (tag == 1)
                        sub.Add((string)values[1], new XAttribute("lang", (string)values[2]));
                    else if (tag == 2)
                        sub.Add(new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource", (string)values[1]));
                    el.Add(sub);
                }
                db.Add(el);
            }
            db.Save(stream);
        }

        public void SaveFlowToPolarText(IEnumerable<object> flow, FileStream file)
        {
            TextWriter tw = new StreamWriter(file);
            TextFlow.SerializeFlowToSequenseFormatted(tw, flow, tp_Record, 0);
        }

    }
}