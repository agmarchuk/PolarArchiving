using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Blazor3.Models
{
    public class Pair { public string prop; }
    public class Field : Pair {public string value, lang; }
    public class Direct : Pair { public Record rec; }
    public class Inverse { public string prop; public Record[] recs; }
    public class Record
    {
        public string Id { get; internal set; }
        public string Tp { get; internal set; }
        public Pair[] fields_directs = null;
        public Inverse[] inverses = null;
        
        public static Record GetRecordMinimal(string id)
        {
            XElement xrec = OAData.OADB.GetItemByIdBasic(id, false);
            Record record = new Record() { Id = xrec.Attribute("id").Value, Tp = xrec.Attribute("type").Value };
            Field fname = xrec.Elements("field")
                .Where(f => f.Attribute("prop").Value == "http://fogid.net/o/name")
                .Select(f => new Field() 
                {
                    prop = f.Attribute("prop").Value,
                    value = f.Value,
                    lang = f.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value
                })
                .FirstOrDefault();
            if (fname != null) record.fields_directs = new Pair[1] { fname };
            return record;
        }
        public static Record GetRecord(string id, string forbidden)
        {
            XElement xrec = OAData.OADB.GetItemByIdBasic(id, true);
            if (xrec == null) return null;
            return new Record()
            {
                Id = xrec.Attribute("id").Value,
                Tp = xrec.Attribute("type").Value,
                fields_directs = xrec.Elements()
                .Where(x => x.Name == "field" || x.Name == "direct")
                .Select<XElement, Pair>((XElement fd) =>
                {
                    if (fd.Name == "field")
                    {
                        return new Field()
                        {
                            prop = fd.Attribute("prop").Value,
                            value = fd.Value,
                            lang = fd.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value
                        };
                    }
                    else if (fd.Name == "direct" && fd.Attribute("prop").Value != forbidden)
                    {
                        return new Direct()
                        {
                            prop = fd.Attribute("prop").Value,
                            rec = GetRecordMinimal(fd.Element("record").Attribute("id").Value)
                        };
                    }
                    else return null;
                }
                )};
            }
        }
        // xrec должен содержать прямые ссылки
        public static Record CreateRecordWithDirects(XElement xrec, string forbidden)
        {
            if (xrec == null) return null;
            Record rec = CreateRecordWithFields(xrec);
            rec.directs = xrec.Elements("direct")
                .Where(d => d.Attribute("prop").Value != forbidden)
                .Select(d =>
                {
                    string target = d.Element("record").Attribute("id").Value;
                    return new Direct()
                    {
                        prop = d.Attribute("prop").Value,
                        rec = Record.CreateRecordWithFields(OAData.OADB.GetItemByIdBasic(target, false))
                    };
                }).ToArray();
            return rec;
        }
        // xrec должен содержать обратные ссылки
        public static Record CreateRecordWithInverse(XElement xrec)
        {
            if (xrec == null) return null;
            Record rec = Record.CreateRecordWithDirects(xrec, null);
            var q1 = xrec.Elements("inverse")
                .Select(i => new Tuple<string, XElement>(i.Attribute("prop").Value, i.Element("record")))
                .ToArray();
            var q2 = q1
                .GroupBy(tup => tup.Item1, tup => tup.Item2)
                .ToArray();
            var q3 = q2
                .Select(g => new Inverse()
                {
                    prop = g.Key,
                    recs = g.Select(e => Record.CreateRecordWithDirects(OAData.OADB.GetItemByIdBasic(e.Attribute("id").Value, true), g.Key)).ToArray()
                })
                .ToArray();
            rec.inverses = q3; //inversegroups;
            return rec;
        }
    }
    public class ShowModel
    {
        public Record Rec { get; private set; }

        public ShowModel(string id)
        {
            XElement xtree = OAData.OADB.GetItemByIdBasic(id, true);
            if (xtree == null) return;
            Rec = Record.CreateRecordWithInverse(xtree);
        }
    }
}
