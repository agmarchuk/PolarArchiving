using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using OAData.Adapters;

namespace OAData
{
    public class OADB
    {
        private static CassInfo[] cassettes = null;
        private static FogInfo[] fogs = null;
        public static DAdapter adapter = null;

        public static string look = "";

        private static string path;
        private static XElement _xconfig = null;
        public static XElement XConfig { get { return _xconfig; } }

        public static void Init(string pth)
        {
            path = pth;
            Init();
        }
        public static void Init()
        { 
            XElement xconfig = XElement.Load(path + "config.xml");
            _xconfig = xconfig;
            // Кассеты перечислены через элементы LoadCassette. Имена кассет в файловой системе должны сравниваться по lower case
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
            // Прямое попадание в список фогов из строчек конфигуратора
            foreach (var lf in xconfig.Elements("LoadFog"))
            {
                string fogname = lf.Value;
                int lastpoint = fogname.LastIndexOf('.');
                if (lastpoint == -1) throw new Exception("Err in fog file name construction");
                string ext = fogname.Substring(lastpoint).ToLower();
                bool writable = (lf.Attribute("writable")?.Value == "true" || lf.Attribute("write")?.Value == "yes") ?
                    true : false;
                var atts = ReadFogAttributes(fogname);
                fogs_list.Add(new FogInfo()
                {
                    vid = ext,
                    pth = fogname,
                    writable = writable && atts.prefix != null && atts.counter != null,
                    owner = atts.owner,
                    prefix = atts.prefix,
                    counter = atts.counter == null ? -1 : Int32.Parse(atts.counter)
                });
                
            }
            // Сбор фогов из кассет
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
                    fogx = null,
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
                        fogx = null,
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
                string pre = connectionstring.Substring(0, connectionstring.IndexOf(':'));
                if (pre == "trs")
                {
                    adapter = new TripleRecordStoreAdapter();
                }
                else if (pre == "xml")
                {
                    adapter = new XmlDbAdapter();
                }
                adapter.Init(connectionstring);
                bool toload = false;
                if (toload)
                {
                    adapter.StartFillDb(null);
                    //adapter.LoadFromCassettesExpress(fogs.Select(fo => fo.pth),
                    //    null, null);
                    adapter.FillDb(fogs, null);
                    adapter.FinishFillDb(null);
                }
                else
                {
                    
                }
                if (pre == "xml") Load();

                // Логфайл элементов Put()
                //putlogfilename = connectionstring.Substring(connectionstring.IndexOf(':') + 1) + "logfile_put.txt";
                putlogfilename = path + "logfile_put.txt";
            }
        }
        public static void Load()
        {
            adapter.StartFillDb(null);
            //adapter.LoadFromCassettesExpress(fogs.Select(fo => fo.pth),
            //    null, null);
            adapter.FillDb(fogs, null);
            adapter.FinishFillDb(null);
        }
        public static void Reload()
        {
            Close();
            Init();
            Load();
        }
        private static string putlogfilename = null;
        public static void Close()
        {
            adapter.Close();
        }
        public static string CassDirPath(string uri)
        {
            if (!uri.StartsWith("iiss://")) throw new Exception("Err: 22233");
            int pos = uri.IndexOf('@', 7);
            if (pos < 8) throw new Exception("Err: 22234");
            return cassettes.FirstOrDefault(c => c.name == uri.Substring(7, pos - 7))?.path;
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

        /// <summary>
        /// Делает портрет айтема, годный для простого преобразования в html-страницу или ее часть.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static XElement GetBasicPortrait(string id)
        {
            XElement tree = GetItemByIdBasic(id, true);
            return new XElement("record", new XAttribute(tree.Attribute("id")), new XAttribute(tree.Attribute("type")),
                tree.Elements().Where(el => el.Name == "field" || el.Name == "direct")
                .Select(el =>
                {
                    if (el.Name == "field") return new XElement(el);
                    string prop = el.Attribute("prop").Value;
                    string target = el.Element("record").Attribute("id").Value;
                    XElement tr = GetItemByIdBasic(target, false);
                    return new XElement("direct", new XAttribute("prop", prop),
                        new XElement("record",
                            new XAttribute(tr.Attribute("id")),
                            new XAttribute(tr.Attribute("type")),
                            tr.Elements()));
                }),
                null);
        }

        public static XElement GetItemById(string id, XElement format)
        {
            return adapter.GetItemById(id, format);
        }
        public static IEnumerable<XElement> GetAll()
        {
            return adapter.GetAll();
        }
        public static XElement UpdateItem(XElement item)
        {
            string id = item.Attribute(ONames.rdfabout)?.Value;
            if (id == null)
            {  // точно не апдэйт
                return PutItem(item);
            }
            else
            { // возможно update
                XElement old = adapter.GetItemByIdBasic(id, false);
                if (old == null) return PutItem(item);
                // добавляем старые, которых нет
                XElement nitem = new XElement(item);
                // новые свойства. TODO: Языковые варианты опущены!
                string[] props = nitem.Elements().Select(el => el.Name.LocalName).ToArray();
                nitem.Add(old.Elements()
                .Select(el =>
                {
                    string prop = el.Attribute("prop").Value;
                    int pos = prop.LastIndexOf('/');
                    XName subel_name = XName.Get(prop.Substring(pos + 1), prop.Substring(0, pos));
                    if (props.Contains(prop.Substring(pos + 1))) return null;
                    XElement subel = new XElement(subel_name);
                    if (el.Name == "field") subel.Add(el.Value);
                    else if (el.Name == "direct") subel.Add(new XAttribute(ONames.rdfresource, el.Element("record").Attribute("id").Value));
                    return subel;
                }));
                return PutItem(nitem);
            }
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
            if (fi.fogx == null) fi.fogx = XElement.Load(fi.pth);


            // читаем или формируем идентификатор
            string id = item.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about")?.Value;
            XElement element = null; // запись с пришедшим идентификатором
            if (id == null)
            {
                XAttribute counter_att = fi.fogx.Attribute("counter");
                int counter = Int32.Parse(counter_att.Value);
                id = fi.fogx.Attribute("prefix").Value + counter;
                // внедряем
                item.Add(new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", id));
                counter_att.Value = "" + (counter + 1);
            }
            else
            {
                element = fi.fogx.Elements().FirstOrDefault(el => 
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
            fi.fogx.Add(nitem);

            // Сохраняем файл
            fi.fogx.Save(fi.pth);

            // Сохраняем в базе данных
            adapter.PutItem(nitem);

            // Сохраняем в логе
            using (Stream log = File.Open(putlogfilename, FileMode.Append, FileAccess.Write))
            {
                TextWriter tw = new StreamWriter(log, System.Text.Encoding.UTF8);
                tw.WriteLine(nitem.ToString());
                tw.Close();
            }

            return new XElement(nitem);
        }


        private static (string owner, string prefix, string counter)  ReadFogAttributes(string pth)
        {
            // Нужно для чтиния в кодировке windows-1251. Нужен также Nuget System.Text.Encoding.CodePages
            var v = System.Text.CodePagesEncodingProvider.Instance;
            System.Text.Encoding.RegisterProvider(v);

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
            //Console.WriteLine($"ReadFogAttributes({pth}) : {owner} {prefix} {counter} ");
            return (owner, prefix, counter);
        }
    }

}
