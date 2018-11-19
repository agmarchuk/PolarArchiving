using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Polar.DB;
using Polar.CellIndexes;

namespace Polar.Cassettes.DocumentStorage
{
    /// <summary>
    /// Простая база данных на основе Xml-построения. 
    /// </summary>
    public class SimpleDbAdapter : DbAdapter
    {
        private Action<string> errors = s => { Console.WriteLine(s); };
        private PType tp_elem = null;
        TableViewImmutable table;
        /// <summary>
        /// Инициирование базы данных
        /// </summary>
        /// <param name="connectionstring">префикс варианта базы данных xml:, больше в connectionstring ничего не существенно </param>
        public override void Init(string connectionstring)
        {
            tp_elem = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.sstring)),
                new NamedType("mT", new PType(PTypeEnumeration.longinteger)),
                new NamedType("xtext", new PType(PTypeEnumeration.sstring)));
            table = new TableViewImmutable("wwwroot/table", tp_elem);

            IndexDynamic<string, IndexHalfkeyImmutable<string>> keyIndex =
                new IndexDynamic<string, IndexHalfkeyImmutable<string>>(false, new IndexHalfkeyImmutable<string>("wwwroot/keyindex"))
                {
                    //KeyProducer = ob => (string)((object[])(((object[])ob)[1])[0]);
                };
        }

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int totalelements = 0;
        // Загрузка базы данных
        public override void StartFillDb(Action<string> turlog)
        {
            //throw new Exception("1334");
            sw.Start();
            table.Clear();
            table.Fill(new object[0]);
        }
        public override void LoadFromCassettesExpress(IEnumerable<string> fogfilearr, Action<string> turlog, Action<string> convertlog)
        {
            foreach (string filename in fogfilearr)
            {
                XElement fog = XElement.Load(filename);
                totalelements += fog.Elements().Count();
                foreach (XElement elem in fog.Elements())
                {
                    string s = elem.ToString();
                    string id = elem.Name.LocalName == "delete" ? elem.Attribute("id").Value : elem.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    XAttribute mTatt = elem.Attribute("mT");
                    long mt = mTatt == null ? 0L : DateTime.Parse(mTatt.Value).ToBinary();
                    if (elem.Name.LocalName == "delete") { }
                    table.AppendElement(new object[] { id, mt, s });
                }
            }
        }
        public override void FinishFillDb(Action<string> turlog)
        {
            //throw new Exception("29485");
            table.Flush();
            sw.Stop();
            Console.WriteLine($"Load ok. duration = {sw.ElapsedMilliseconds} totalelements={totalelements}");
        }

        public override void Save(string filename)
        {
            throw new Exception("23434");
        }

        public override void LoadXFlowUsingRiTable(IEnumerable<XElement> xflow)
        {
            throw new Exception("29484");
        }
        public override IEnumerable<XElement> SearchByName(string searchstring)
        {
            throw new Exception("29486");
        }
        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            throw new Exception("29487");
        }
        public override XElement GetItemById(string id, XElement format)
        {
            throw new Exception("29488");
        }
        public override XElement GetItemByIdSpecial(string id)
        {
            return GetItemByIdBasic(id, true);
        }

        private XElement RemoveRecord(string id)
        {
            throw new Exception("29489");
        }
        public override XElement Delete(string id)
        {
            throw new Exception("29490");
        }

        public override XElement Add(XElement record)
        {
            throw new Exception("29491");
        }

        public override XElement AddUpdate(XElement record)
        {
            throw new Exception("29492");
        }
    }
}
