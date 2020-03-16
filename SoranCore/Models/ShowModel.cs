using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoranCore.Models
{
    public class Field { public string prop, value, lang; }
    public class Direct { public string prop; public Record rec; }
    public class Inverse { public string prop; public Record[] recs; }
    public class Record
    {
        public string Id { get; internal set; }
        public string Tp { get; internal set; }
        public Field[] fields = null;
        public Direct[] directs = null;
        public Inverse[] inverses = null;
        // Генерация записи по "базовому" (после OAData.OADB.GetItemByIdBasic(id, _)) запросу
        public static Record CreateRecordWithFields(XElement xrec)
        {
            return new Record()
            {
                Id = xrec.Attribute("id").Value,
                Tp = xrec.Attribute("type").Value,
                fields = xrec.Elements("field")
                .Where(f => f.Attribute("prop").Value != "http://fogid.net/u/uri")
                .Select(f =>
                    new Field() { prop = f.Attribute("prop").Value, value = f.Value, lang = f.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value })
                    .ToArray()
            };
        }
        // xrec должен содержать прямые ссылки
        public static Record CreateRecordWithDirects(XElement xrec, string forbidden)
        {
            Record rec = CreateRecordWithFields(xrec);
            rec.directs = xrec.Elements("direct")
                .Where (d => d.Attribute("prop").Value != forbidden)
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
            Record rec = Record.CreateRecordWithDirects(xrec, null);
            var inversegroups = xrec.Elements("inverse")
                .Select(i => new Tuple<string, XElement>(i.Attribute("prop").Value, i.Element("record")))
                .GroupBy(tup => tup.Item1, tup => tup.Item2)
                .Select(g => new Inverse()
                {
                    prop = g.Key,
                    recs = g.Select(e => Record.CreateRecordWithDirects(OAData.OADB.GetItemByIdBasic(e.Attribute("id").Value, true), g.Key)).ToArray()
                })
                .ToArray();
            rec.inverses = inversegroups;
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
