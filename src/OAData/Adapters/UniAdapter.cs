using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Polar.DB;
using Polar.Universal;

namespace OAData.Adapters
{
    public class UniAdapter : DAdapter
    {
        // Адаптер состоит из последовательности записей, дополнительного индекса и последовательности триплетов объектных свойств 
        private PType tp_prop;
        private PType tp_rec;
        private PType tp_triple;
        private USequence records;
        private SVectorIndex names;
        private SVectorIndex words;

        private Func<object, bool> Isnull;

        // Констуктор инициализирует то, что можно и не изменяется
        public UniAdapter()
        {
            tp_prop = new PTypeUnion(
                new NamedType("novariant", new PType(PTypeEnumeration.none)),
                new NamedType("field", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("value", new PType(PTypeEnumeration.sstring)),
                    new NamedType("lang", new PType(PTypeEnumeration.sstring)))),
                new NamedType("objprop", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("link", new PType(PTypeEnumeration.sstring)))),
                new NamedType("invlink", new PTypeRecord(
                    new NamedType("prop", new PType(PTypeEnumeration.sstring)),
                    new NamedType("sourcelink", new PType(PTypeEnumeration.sstring))))
                );
            tp_rec = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.sstring)),
                new NamedType("tp", new PType(PTypeEnumeration.sstring)),
                new NamedType("props", new PTypeSequence(tp_prop)));
            tp_triple = new PTypeRecord(
                new NamedType("subj", new PType(PTypeEnumeration.sstring)),
                new NamedType("pred", new PType(PTypeEnumeration.sstring)),
                new NamedType("obj", new PType(PTypeEnumeration.sstring)));
            Isnull = ob => (string)((object[])ob)[1] == "delete"; // в поле tp находится "delete" //TODO: проверить, что delete без пространств имен 
        }

        // Главный инициализатор. Используем connectionstring 
        private string dbfolder;
        private int file_no = 0;
        public override void Init(string connectionstring)
        {
            if (connectionstring != null && connectionstring.StartsWith("uni:"))
            {
                dbfolder = connectionstring.Substring("uni:".Length);
            }
            if (File.Exists(dbfolder + "0.bin")) nodatabase = false;
            Func<Stream> GenStream = () =>
                new FileStream(dbfolder + (file_no++) + ".bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            // Полключение к базе данных
            Func<object, int> hashId = obj => Hashfunctions.HashRot13(((string)(((object[])obj)[0])));
            Comparer<object> comp_direct = Comparer<object>.Create(new Comparison<object>((object a, object b) =>
            {
                string val1 = (string)((object[])a)[0];
                string val2 = (string)((object[])b)[0];
                return string.Compare(val1, val2, StringComparison.OrdinalIgnoreCase);
            }));

            // Создаем последовательность R-записей
            records = new USequence(tp_rec, GenStream,
                rec => (string)((object[])rec)[1] == "deleted",
                rec => (string)((object[])rec)[0],
                str => Hashfunctions.HashRot13((string)str));
            Func<object, IEnumerable<string>> skey = obj =>
            {
                object[] props = (object[])((object[])obj)[2];
                var query = props.Where(p => (int)((object[])p)[0] == 1)
                    .Select(p => ((object[])p)[1])
                    .Cast<object[]>()
                    .Where(f => (string)f[0] == "http://fogid.net/o/name")
                    .Select(f => (string)f[1]).ToArray();
                return query;
            };
            names = new SVectorIndex(GenStream, records, skey);
            //additional_names = new SVectorDynaIndex(records, skey);
            records.uindexes = new IUIndex[]
            {
                names
            };
        }


        public override void StartFillDb(Action<string> turlog)
        {
            records.Clear();
            // Индексы чистятся внутри
        }
        public override void FinishFillDb(Action<string> turlog)
        {
            records.Build(); 
            GC.Collect();
        }

        // Загрузка потока x-элементов
        internal class OTriple { public string subj, pred, resource; }
        internal class InverseLink { public string pred, source; }
        public override void LoadXFlow(IEnumerable<XElement> xflow, Dictionary<string, string> orig_ids)
        {
            // Соберем граф через 2 сканирования
            // Проход 1: вычисление обратных объектных свойств
            Dictionary<string, InverseLink[]> inverseProps = xflow.SelectMany(record =>
            {
                string id = record.Attribute(ONames.rdfabout).Value;
                if (id == "syp2001-p-marchuk_a") { }
                // Корректируем идентификатор
                if (orig_ids.TryGetValue(id, out string idd)) id = idd;
                var directProps = record.Elements().Where(el => el.Attribute(ONames.rdfresource) != null)
                    .Select(el =>
                    {
                        string subj = id;
                        string pred = el.Name.NamespaceName + el.Name.LocalName;
                        string resource = el.Attribute(ONames.rdfresource).Value;
                        if (orig_ids.TryGetValue(resource, out string res)) if (res != null) resource = res;
                        return new OTriple { subj = subj, pred = pred, resource = resource };
                    });
                return directProps;
            }).GroupBy(triple => triple.resource, triple => new InverseLink { pred = triple.pred, source = triple.subj })
            .Select(g => new { key = g.Key, ilinks = g.Select(lnk => lnk) })
            .ToDictionary(pair => pair.key, pair => pair.ilinks.ToArray());
            //Dictionary<string, InverseLink[]> inverseProps = new Dictionary<string, InverseLink[]>();

            //var test_rec = inverseProps["syp2001-p-marchuk_a"];

            // Проход 2: вычисление графа 
            IEnumerable<object> flow = xflow.Select(record =>
            {
                string id = record.Attribute(ONames.rdfabout).Value;
                // Корректируем идентификатор
                if (orig_ids.TryGetValue(id, out string idd)) id = idd;

                string rec_type = ONames.fog + record.Name.LocalName;
                object[] orecord = new object[] {
                    id,
                    rec_type,
                    record.Elements()
                        .Select(el =>
                        {
                            string prop = el.Name.NamespaceName + el.Name.LocalName;
                            if (el.Attribute(ONames.rdfresource) != null)
                            {
                                string resource = el.Attribute(ONames.rdfresource).Value;
                                if (orig_ids.TryGetValue(resource, out string res)) if (res != null) resource = res;
                                return new object[] { 2, new object[] { prop, resource } };
                            }
                            else
                            {
                                var xlang = el.Attribute("{http://www.w3.org/XML/1998/namespace}lang");
                                string lang = xlang == null ? "" : xlang.Value;
                                return new object[] { 1, new object[] { prop, el.Value, lang } };
                            }
                        })
                        .Concat(inverseProps.ContainsKey(id) ?
                            inverseProps[id].Select(ip => new object[] { 3, new object[] { ip.pred, ip.source } }) :
                            Enumerable.Empty<InverseLink[]>())
                        .ToArray()
                    };
                return orecord;
            });
            records.Load(flow);
            GC.Collect();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<XElement> SearchByName(string searchstring)
        {
            var qu = records.GetAllByLike(0, searchstring).ToArray();
            return qu.Select(r => ORecToXRec((object[])r, false));
        }

        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            object rec = GetRecord(id);

            if (rec == null || Isnull(rec)) return null;
            XElement xres = ORecToXRec((object[])rec, addinverse);

            return xres;
        }
        private object GetRecord(string id)
        {
            object rec;
            rec = records.GetByKey(id);
            return rec;
        }
        private XElement ORecToXRec(object[] ob, bool addinverse)
        {
            return new XElement("record",
                new XAttribute("id", ob[0]), new XAttribute("type", ob[1]),
                ((object[])ob[2]).Cast<object[]>().Select(uni =>
                {
                    if ((int)uni[0] == 1)
                    {
                        object[] p = (object[])uni[1];
                        XAttribute langatt = string.IsNullOrEmpty((string)p[2]) ? null : new XAttribute(ONames.xmllang, p[2]);
                        return new XElement("field", new XAttribute("prop", p[0]), langatt,
                            p[1]);
                    }
                    else if ((int)uni[0] == 2)
                    {
                        object[] p = (object[])uni[1];
                        return new XElement("direct", new XAttribute("prop", p[0]),
                            new XElement("record", new XAttribute("id", p[1])));
                    }
                    else if ((int)uni[0] == 3)
                    {
                        object[] p = (object[])uni[1];
                        return new XElement("inverse", new XAttribute("prop", p[0]),
                            new XElement("record", new XAttribute("id", p[1])));
                    }
                    return null;
                }));
        }

        public override XElement GetItemById(string id, XElement format)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<XElement> GetAll()
        {
            throw new NotImplementedException();
        }

        public override XElement Delete(string id)
        {
            throw new NotImplementedException();
        }

        public override XElement PutItem(XElement record)
        {
            throw new NotImplementedException();
        }
    }
}
