﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

using Polar.Cassettes;
using Polar.Cassettes.DocumentStorage;

namespace Turgunda7
{
    public class SObjects
    {
        public static XElement accounts = null;
        public static string path = null;
        public static string extappbinpath = null;
        public static string newcassettesdirpath = null;
        public static XElement finfo_configdefault = null;
        public static string appname = "Открытый архив СО РАН";
        //internal static DStorage storage = null;
        //private static DbAdapter engine = null;
        private static CassetteData.CassetteIntegration _engine;
        public static CassetteData.CassetteIntegration Engine { get { return _engine; } }

        public static bool Initiated = true;
        private static System.Text.StringBuilder _errors = new System.Text.StringBuilder();
        public static string Errors { get { return _errors.ToString(); } }
        public static XElement xconfig = null;
        public static void SaveConfig() { xconfig.Save(path + "wwwroot/config.xml"); }
        public static void SaveAccounts() { accounts.Save(path + "wwwroot/accounts.xml"); }

        public static void Init() { Init(path); }
        public static void Init(string pth)
        {
            path = pth + "/";
            try
            {
                if (!System.IO.File.Exists(path + "wwwroot/config.xml"))
                {
                    System.IO.File.Copy(path + "wwwroot/config0.xml", path + "wwwroot/config.xml");
                }
                xconfig = XElement.Load(path + "wwwroot/config.xml");
                // Работа с параметрами конфигурации
                extappbinpath = xconfig.Element("Parameters")?.Attribute("extappbinpath")?.Value;
                if (extappbinpath == null) extappbinpath = path + "wwwroot/extappbinpath/";
                newcassettesdirpath = xconfig.Element("Parameters")?.Attribute("newcassettesdirpath")?.Value;
                if (newcassettesdirpath == null) newcassettesdirpath = path + "wwwroot/newcassettesdirpath/";
                string appn = xconfig.Element("params")?.Attribute("appname")?.Value;
                if (appn != null) appname = appn;

                finfo_configdefault = xconfig.Element("finfo");
                // WARNING! Это опасно передавать параметры через статические переменные класса!!!
                if (finfo_configdefault != null) Cassette.finfo_default = finfo_configdefault;

                //storage = new DStorage();
                //storage.Init(xconfig);
                //engine = new XmlDbAdapter();
                //storage.InitAdapter(engine);
                _engine = new CassetteData.CassetteIntegration(xconfig, false);


                // Загрузка профиля и онтологии
                if (!System.IO.File.Exists(path + "wwwroot/ApplicationProfile.xml"))
                {
                    System.IO.File.Copy(path + "wwwroot/ApplicationProfile0.xml", path + "wwwroot/ApplicationProfile.xml");
                }
                appProfile = XElement.Load(path + "wwwroot/ApplicationProfile.xml");
                XElement ontology = XElement.Load(path + "wwwroot/ontology_iis-v12-doc_ruen.xml");
                TurgundaCommon.ModelCommon.formats = appProfile.Element("formats");
                TurgundaCommon.ModelCommon.LoadOntNamesFromOntology(ontology);
                TurgundaCommon.ModelCommon.LoadInvOntNamesFromOntology(ontology);

            }
            catch (Exception e) { Initiated = false; _errors.Append(e.Message); Log(e.Message); }
                //storage.SaveDb("C:/Home/syp_db.xml");
            try
            {
                accounts = XElement.Load(path + "wwwroot/accounts.xml");
            } catch (Exception)
            {
                accounts = new XElement("accounts");
                AccountsSave();
            }
        }

        public static void AccountsSave()
        {
            accounts.Save(path + "wwwroot/accounts.xml");
        }

        public static bool tolog = false;
        public static void Log(string message)
        {
            if (tolog)
            {
                var f = System.IO.File.AppendText(path + "wwwroot/log.txt");
                f.WriteLine($"{System.DateTime.Now.ToUniversalTime()} {message}");
                f.Close();
            }
        }
        private static XElement appProfile;

        public static void LoadFromCassettesExpress() { throw new NotImplementedException(); }

        public static CassetteInfo GetCassetteInfoById(string id)
        {
            // Короткое имя кассеты, если есть
            if (!id.EndsWith("_cassetteId")) { return null; }
            string sid = id.Substring(0, id.Length - "_cassetteId".Length).ToLower();
            string fid = "iiss://" + sid + "@iis.nsk.su";
            // Определяем кассету
            var cassetteExists = SObjects.Engine.localstorage.connection.cassettesInfo.ContainsKey(fid);
            if (!cassetteExists) { return null; }
            return SObjects.Engine.localstorage.connection.cassettesInfo[fid];
        }

        public static IEnumerable<XElement> SearchByName(string searchstring) { return _engine.SearchByName(searchstring); }
        public static XElement GetItemById(string id, XElement format) { return _engine.GetItemById(id, format); }
        public static XElement GetItemByIdSpecial(string id) { return _engine.GetItemByIdBasic(id, true); }

        // ============== Редактирование базы данных ===============
        public static string AddInvRelation(string eid, string prop, string rtype, string username)
        {
            int pos = rtype.LastIndexOf('/');
            int pos2 = prop.LastIndexOf('/');
            XElement xrecord = PutItemToDb(new XElement(XName.Get(rtype.Substring(pos + 1), rtype.Substring(0, pos + 1)),
                new XElement(XName.Get(prop.Substring(pos2 + 1), prop.Substring(0, pos2 + 1)),
                    new XAttribute(ONames.rdfresource, eid))),
                true, username);
            if (xrecord == null) return null;
            return xrecord.Attribute(ONames.rdfabout).Value;
        }

        // Возвращает идентификатор созданного айтема или null
        public static string CreateNewItem(string name, string rtype, string username)
        {
            int pos = rtype.LastIndexOf('/');
            XElement xrecord = PutItemToDb(new XElement(XName.Get(rtype.Substring(pos + 1), rtype.Substring(0, pos + 1)),
                new XElement("{http://fogid.net/o/}name", new XAttribute(ONames.xmllang, "ru"), name)),
                true, username);

            return xrecord?.Attribute(ONames.rdfabout).Value;
        }
        public static void DeleteItem(string id, string username)
        {
            //engine.DeleteRecord(id);
            PutItemToDb(new XElement("{http://fogid.net/o/}delete", new XAttribute("id", id)),
                false, username);
        }

        public static XElement GetEditFormat(string type, string prop)
        {
            XElement res = appProfile.Element("EditRecords").Elements("record").FirstOrDefault(re => re.Attribute("type").Value == type);
            if (res == null) return null;
            XElement resu = new XElement(res);
            if (prop != null)
            {
                XElement forbidden = resu.Elements("direct").FirstOrDefault(di => di.Attribute("prop").Value == prop);
                if (forbidden != null) forbidden.Remove();
            }
            return resu;
        }
        private static object saveInDb = new object();
        /// <summary>
        /// Базовый метод. Помещает айтем, включая и операторы, в обе базы данных. Если требуется, то формирует идентификатор
        /// </summary>
        /// <param name="item">Айтем в "стандартном" виде. В записи допустимо отсуттсвие rdf:about, тогда tocreate Должен быть true</param>
        /// <param name="tocreateid">нужно ли создавать для записи новый идентификатор</param>
        /// <param name="username">идентификатор пользователя</param>
        /// <returns>скорректированный элемент, помещаенный в базы данных</returns>
        public static XElement PutItemToDb(XElement item, bool tocreateid, string username)
        {
            XElement item_corrected = new XElement(item);
            item_corrected.Add(new XAttribute("owner", username), new XAttribute("mT", DateTime.Now.ToUniversalTime().ToString("u")));
            lock (saveInDb)
            {
                item_corrected = _engine.localstorage.EditCommand(item_corrected);
            }
            //Log("PutItemToDb ok?");
            return item_corrected;
        }
    }
}