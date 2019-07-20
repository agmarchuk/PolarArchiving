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
                    XAttribute write_att = lc.Attribute("wtite");
                    string name = cassPath.Split('/', '\\').Last();
                    return new CassInfo() { name = name, path = cassPath,
                        writable = (write_att != null && (write_att.Value == "yes" || write_att.Value == "true")) };
                })
                .ToArray();

            // Формирую список фог-документов
            List<FogInfo> fogs_list = new List<FogInfo>();
            for (int i=0; i< cassettes.Length; i++)
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
                    cassette = cass,
                    pth = pth,
                    owner = atts.owner,
                    writable = cass.writable,
                    prefix = atts.prefix,
                    counter = atts.counter
                });
                // А еще в кассете могут быть другие фог-документы. Они размещаются в originals
                IEnumerable<FileInfo> fgs = (new DirectoryInfo(cass.path + "/originals"))
                    .GetDirectories("????").SelectMany(di =>di.GetFiles("*.fog"));
                // Быстро проглядим документы и поместим информацию в список фогов
                foreach (FileInfo fi in fgs)
                {
                    var attts = ReadFogAttributes(pth);
                    // запишем владельца, уточним признак записи
                    cass.owner = attts.owner;
                    fogs_list.Add(new FogInfo()
                    {
                        cassette = cass,
                        pth = fi.FullName,
                        owner = attts.owner,
                        writable = cass.writable,
                        prefix = attts.prefix,
                        counter = attts.counter
                    });
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
                Console.WriteLine(xrec.ToString());
                
            }
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
        public CassInfo cassette;
        public string pth; // координата файла
        public string owner;
        public string prefix;
        public string counter;
        public bool writable;
    }
}
