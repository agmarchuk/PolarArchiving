using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Polar.TripleStore;
using Polar.Cassettes.DocumentStorage;

namespace OADataService
{
    public class CassettesConfiguration
    {
        public static string CassDirPath(string uri)
        {
            if (!uri.StartsWith("iiss://")) throw new Exception("Err: 22233");
            int pos = uri.IndexOf('@', 7);
            if (pos < 8) throw new Exception("Err: 22234");
            return cassettes.FirstOrDefault(c => c.name == uri.Substring(7, pos - 7))?.path;
        }
        private static CassInfo[] cassettes;
        private static FogInfo[] fogs;
        private static DbAdapter adapter = null;

        public static string look = "";

        private XElement xconfig = null;
        public CassettesConfiguration(XElement xconfig)
        {
            this.xconfig = xconfig;
            // Кассеты перечислены через элементы LoadCassette. Имена кассе в файловой системе должны сравниваться по lower case
            cassettes = xconfig.Elements("LoadCassette")
                .Select(lc =>
                {
                    string cassPath = lc.Value;
                    XAttribute write_att = lc.Attribute("write");
                    string name = cassPath.Split('/', '\\').Last();
                    return new CassInfo() { name = name, path = cassPath,
                        writable = (write_att != null && (write_att.Value == "yes" || write_att.Value == "true")) };
                })
                .ToArray();

            // Формирую список фог-документов
            List<FogInfo> fogs_list = new List<FogInfo>();
            for (int i = 0; i < cassettes.Length; i++)
            {
                // В каждой кассете есть фог-элемент meta/имякассеты_current.fog, в нем есть владелец и может быть запрет на запись в виде
                // отсутствия атрибутов prefix или counter. Также там есть uri кассеты, надо проверить.
                CassInfo cass = cassettes[i];
                string pth = cass.path + "/meta/" + cass.name + "_current.fog";
                var atts = ReadFogAttributes(pth);
                // запишем владельца, уточним признак записи
                cass.owner = atts.owner;
                if (atts.prefix == null || atts.counter == null) cass.writable = false;
                fogs_list.Add(new FogInfo()
                {
                    //cassette = cass,
                    pth = pth,
                    fog = null,
                    owner = atts.owner,
                    writable = true //cass.writable,
                    //prefix = atts.prefix,
                    //counter = atts.counter
                });
                // А еще в кассете могут быть другие фог-документы. Они размещаются в originals
                IEnumerable<FileInfo> fgs = (new DirectoryInfo(cass.path + "/originals"))
                    .GetDirectories("????").SelectMany(di => di.GetFiles("*.fog"));
                // Быстро проглядим документы и поместим информацию в список фогов
                foreach (FileInfo fi in fgs)
                {
                    var attts = ReadFogAttributes(fi.FullName);

                    // запишем владельца, уточним признак записи
                    cass.owner = attts.owner;
                    fogs_list.Add(new FogInfo()
                    {
                        //cassette = cass,
                        pth = fi.FullName,
                        fog = null,
                        owner = attts.owner,
                        //writable = cass.writable,
                        //prefix = attts.prefix,
                        //counter = attts.counter
                        writable = cass.writable && attts.prefix != null && attts.counter != null
                    }); ;
                }

            }
            // На выходе я определил, что будет массив
            fogs = fogs_list.ToArray();
            fogs_list = null;

            // Подключение к базе данных, если задано
            string connectionstring = xconfig.Element("database")?.Attribute("connectionstring")?.Value;
            if (connectionstring != null)
            {
                adapter = new TripleRecordStoreAdapter();
                adapter.Init(connectionstring);
                bool toload = true;
                if (toload)
                {
                    adapter.StartFillDb(null);
                    adapter.LoadFromCassettesExpress(fogs.Select(fo => fo.pth),
                        null, null);
                    adapter.FinishFillDb(null);
                }
            }
        }
        public void Test()
        { 
            // Испытаем
            var query = adapter.SearchByName("марчук");
            XElement xseq = new XElement("sequ");
            foreach (XElement rec in query)
            {
                //Console.WriteLine(rec.ToString());
                xseq.Add(rec);
            }
            look = xseq.ToString();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(look);
            XElement xrec = adapter.GetItemByIdBasic("syp2001-p-marchuk_a", true);
            if (xrec != null) Console.WriteLine(xrec.ToString());

            XElement x = XElement.Parse(@"
<person xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' owner='tester'
>
<name xml:lang='ru'>Пупкин Первый</name>
</person>
    ");
            //var result = PutItem(x);
            //Console.WriteLine(result.ToString());

            foreach (XElement rec in SearchByName("Аааков"))
            {
                Console.WriteLine(rec.ToString());
            }

            //string person_id = "syp2001-p-marchuk_a";
            string person_id = "Cassette_test20180311_tester_636565276468061094_1067";
            XElement format = new XElement("record",
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")));
            var xperson = GetItemById(person_id, format);
            Console.WriteLine(xperson.ToString());
        }
        public static IEnumerable<XElement> SearchByName(string ss)
        {
            return adapter.SearchByName(ss);
        }
        public static XElement GetItemByIdBasic(string id, bool addinverse)
        {
            return adapter.GetItemByIdBasic(id, addinverse);
        }
        public static XElement GetItemById(string id, XElement format)
        {
            return adapter.GetItemById(id, format);
        }
        public static XElement PutItem(XElement item)
        {
            //XElement result = null;
            string owner = item.Attribute("owner")?.Value;
            
            // Запись возможна только если есть код владельца
            if (owner == null) return new XElement("error", "no owner attribute");

            // Проверим и изменим отметку времени
            string mT = DateTime.Now.ToUniversalTime().ToString("u");
            XAttribute mT_att = item.Attribute("mT");
            if (mT_att == null) item.Add(new XAttribute("mT", mT));
            else  mT_att.Value = mT;
            
            
            // Ищем подходящий фог-документ
            FogInfo fi = fogs.FirstOrDefault(f => f.owner == owner && f.writable);
            
            // Если нет подходящего - запись не производится
            if (fi == null) return new XElement("error", "no writable fog for request");

            // Если фог не загружен, то загрузить его
            if (fi.fog == null) fi.fog = XElement.Load(fi.pth);


            // читаем или формируем идентификатор
            string id = item.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about")?.Value;
            XElement element = null; // запись с пришедшим идентификатором
            if (id == null)
            {
                XAttribute counter_att = fi.fog.Attribute("counter");
                int counter = Int32.Parse(counter_att.Value);
                id = fi.fog.Attribute("prefix").Value + counter;
                // внедряем
                item.Add(new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", id));
                counter_att.Value = "" + (counter + 1);
            }
            else
            {
                element = fi.fog.Elements().FirstOrDefault(el => 
                    el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about")?.Value == id);
            }

            // Изымаем из пришедшего элемента владельца и фиксируем его в фоге
            XAttribute owner_att = item.Attribute("owner");
            owner_att.Remove();
            if (element != null)
            {
                element.Remove();
            }
            XElement nitem = new XElement(item);
            fi.fog.Add(nitem);

            // Сохраняем файл
            fi.fog.Save(fi.pth);

            // Сохраняем в базе данных
            adapter.PutItem(nitem);

            return new XElement(nitem);
        }


        private (string owner, string prefix, string counter)  ReadFogAttributes(string pth)
        {
            string owner = null;
            string prefix = null;
            string counter = null;
            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(pth, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name != "rdf:RDF") throw new Exception($"Err: Name={reader.Name}");
                        owner = reader.GetAttribute("owner");
                        prefix = reader.GetAttribute("prefix");
                        counter = reader.GetAttribute("counter");
                        break;
                    }
                }
            }
            Console.WriteLine($"ReadFogAttributes({pth}) : {owner} {prefix} {counter} ");
            return (owner, prefix, counter);
        }
    }
    internal class CassInfo
    {
        public string name;
        public string path;
        public string owner;
        public bool writable = false;
    }
    internal class FogInfo
    {
        //public CassInfo cassette;
        public string pth; // координата файла
        public XElement fog = null;
        public string owner;
        //public string prefix;
        //public int counter; // -1 - отсутствует
        public bool writable;
    }
}
