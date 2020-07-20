using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlazorMvc.Data
{
    public class DService
    {
        public static void Init(string pth)
        {
            string path = pth;
            OAData.Ontology.Init(path + "ontology_iis-v12-doc_ruen.xml");
            OAData.OADB.Init(path);
            //OAData.OADB.Load();
        }
        public static IEnumerable<string[]> Search(string searchstring)
        {
            var query = OAData.OADB.SearchByName(searchstring)
                .Select(x => new string[] { x.Attribute("type").Value, x.Attribute("id").Value, GetField(x, "http://fogid.net/o/name") })
                ;
            return query;
        }
        public static XElement GetItemByIdBasic(string id, bool addinverse)
        {
            return OAData.OADB.GetItemByIdBasic(id, addinverse);
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

    }

}
