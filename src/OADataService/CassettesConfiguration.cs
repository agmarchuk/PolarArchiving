using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OADataService
{
    public class CassettesConfiguration
    {
        private CassInfo[] cassettes;
        private FogInfo[] fogs;

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
            // В каждой кассете есть фог-элемент meta/имякассеты_current.fog, в нем есть владелец и может быть запрет на запись в виде
            // отсутствия атрибутов prefix или counter. Также там есть uri кассеты, надо проверить.
            for (int i=0; i< cassettes.Length; i++)
            {
                CassInfo cass = cassettes[i];
                string pth = cass.path + "/meta/" + cass.name + ".fog";
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
            }
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
