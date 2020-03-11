﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoranCore
{
    public class StaticObjects
    {
        private static string path;
        public static void Init(string pth)
        {
            path = pth;
            OAData.OADB.Init(path);
            //OAData.OADB.Load();
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
    }

}