using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Fogid.Cassettes;
using Polar.Cassettes;
using FreeImageAPI;
using ExifLib;

namespace Turgunda7.Controllers
{
    public static class RequestExt
    {
        public static string Params(this Microsoft.AspNetCore.Http.HttpRequest requ, string name)
        {
            string res = requ.Query[name];
            if (res == null && requ.HasFormContentType) res = requ.Form[name];
            return res;
        }
    }
    public class HomeController : Controller
    {
        public ActionResult Error(string mess)
        {
            return View("Error", new Turgunda7.Models.ErrorModel() { mess = mess });
        }
        public ActionResult Info(string mess)
        {
            return View("Info", new Turgunda7.Models.InfoModel() { mess = mess });
        }
        public ActionResult LoadDB()
        {
            Turgunda7.SObjects.Init();
            return Redirect("~/Home/Index");
        }

        public ActionResult Index()
        {
            if (!Turgunda7.SObjects.Initiated) { return Redirect("~/Home/Error?mess=" +
                System.Web.HttpUtility.UrlEncode(Turgunda7.SObjects.Errors)); }
            string nm = Request.Query["name"];
            if (nm == null) nm = "Фонды";
            if ( nm != null)
            {
                XElement my_element = SObjects.Engine.SearchByName(nm).FirstOrDefault(x => 
                    x.Elements("field").Any(f => f.Attribute("prop").Value == "http://fogid.net/o/name" && f.Value == nm));
                if (my_element != null) return Redirect("~/Home/Portrait?id=" + my_element.Attribute("id").Value);
            }
            return View();
        }
        public ActionResult Search(string searchstring, string type)
        {
            string t = string.IsNullOrEmpty(type) ? null : type;
            var model = new Turgunda7.Models.SearchModel(searchstring, t);
            return View(model);
        }
        public ActionResult Portrait(string id)
        {
            if (id == null) return null;
            Models.PortraitModel pmodel = new Models.PortraitModel(id);
            if (!pmodel.ok) return View("Index");
            return View(pmodel);
        }
        public ActionResult PortraitSpecial(string id)
        {
            Models.PortraitSpecialModel pmodel = new Models.PortraitSpecialModel(id);
            return View(pmodel);
        }

        // ==================== Загрузка данных =====================
        //[HttpPost]
        //public ActionResult Load(string cassetteId, IFormFile file)
        //{  // Этот метод был пробный и сейчас не используется
        //    string filename = file.FileName;
        //    string fpath = @"D:\Home\data\savedfile";
        //    using (System.IO.FileStream fs = new System.IO.FileStream(fpath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        //    {
        //        file.CopyTo(fs);
        //    }
        //    return View("Portrait", new Models.PortraitModel(cassetteId));
        //}
        [HttpPost("UploadFiles")]
        public IActionResult Post(string id, List<IFormFile> files)
        {
            // Пользователь
            var umodel = new Turgunda7.Models.UserModel(Request);
            string user = umodel.Uuser;
            // Нужно проверить прова
            if (!umodel.IsInRole("admin")) return Error("Файлы может добавлять только администратор"); 
                
            var cassetteInfo = SObjects.GetCassetteInfoById(id);
            Cassette cassette = cassetteInfo.cassette;
            if (cassetteInfo == null) return Error("Developers error 287344");
            if (cassetteInfo.cassette.Owner != new Turgunda7.Models.UserModel(Request).Uuser) return Error("Вам не разрешено добавлять файлы в эту кассету"); //new EmptyResult();
            string dirpath = cassetteInfo.url;
            // Ищем подколлекцию upload и если нет - создаем
            var c_item = SObjects.Engine.GetItemById(id,
                new XElement("record",
                    new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                        new XElement("record", new XAttribute("type", "http://fogid.net/o/collection-member"),
                            new XElement("direct", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                                new XElement("record", new XAttribute("type", "http://fogid.net/o/collection"),
                                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/name"))))))));
            var up = c_item.Elements("inverse")
                .Where(i => i.Attribute("prop").Value == "http://fogid.net/o/in-collection")
                .Select(i => i.Element("record")?.Element("direct"))
                .Where(d => d != null)
                .Select(d => d.Element("record"))
                .FirstOrDefault(r => r.Attribute("type").Value == "http://fogid.net/o/collection" &&
                    r.Elements("field").Select(f => f.Value).FirstOrDefault() == "upload");
            string upload_id;
            if (up == null)
            {
                // коллекция
                string cid = cassette.GenerateNewId();
                XElement el1 = new XElement("{http://fogid.net/o/}collection", new XAttribute(ONames.rdfabout, cid),
                    new XElement("{http://fogid.net/o/}name", "upload"));
                // членство в коллекции
                string mid = cassette.GenerateNewId();
                XElement el2 = new XElement("{http://fogid.net/o/}collection-member", new XAttribute(ONames.rdfabout, mid),
                    new XElement("{http://fogid.net/o/}in-collection", new XAttribute(ONames.rdfresource, id)),
                    new XElement("{http://fogid.net/o/}collection-item", new XAttribute(ONames.rdfresource, cid)));

                cassette.db.Add(el1);
                SObjects.PutItemToDb(el1, false, user);
                cassette.db.Add(el2);
                SObjects.PutItemToDb(el2, false, user);

                upload_id = cid;
            }
            else upload_id = up.Attribute("id").Value;

            bool dbupdated = false;
            foreach (var formFile in files)
            {
                // Сохраним документ в промежуточном месте
                string fullname = formFile.FileName;
                int pos = fullname.LastIndexOfAny(new char[] { '/', '\\' });
                string localname = pos == -1 ? fullname : fullname.Substring(pos + 1);  
                string tmpfilepath = dirpath + localname;
                using (var stream = new System.IO.FileStream(tmpfilepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    formFile.CopyTo(stream);
                }
                FileInfo fi = new FileInfo(tmpfilepath);

                CassetteExtension.App_Bin_Path = "C:\\home\\bin\\";
                // Воспользуемся "стандартной" записью файла
                //var elements = CassetteExtension.AddFile1(cassette, fi, id).ToArray();
                var elements = CassetteExtension.AddFile1(cassette, fi, upload_id);
                foreach (var item in elements)
                {
                    var item_corrected = Polar.Cassettes.DocumentStorage.DbAdapter.ConvertXElement(item);
                    SObjects.PutItemToDb(item_corrected, false, user);
                    cassette.db.Add(item_corrected);
                    dbupdated = true;
                }
                //Console.WriteLine("QQQQQQQQQQQQQQQQQQQQQQQ" + elements.Length);
                // Уничтожение времянки
                System.IO.File.Delete(tmpfilepath);
            }
            if (dbupdated) cassette.Save();

            return View("Portrait", new Models.PortraitModel(upload_id));
        }


        private void BuildImageFile(IFormFile formFile, string fpath)
        {
            using (var stream = new System.IO.FileStream(fpath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                MemoryStream mstream = new MemoryStream();
                formFile.CopyToAsync(mstream);
                using (var original = FreeImageBitmap.FromStream(mstream))
                {
                    original.Save(fpath);
                }
            }
        }
        private async Task BuildImageFile0(IFormFile formFile, string fpath)
        {
            using (var stream = new System.IO.FileStream(fpath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                await formFile.CopyToAsync(stream);
            }
        }

        //
        // ==================== Редактирование данных =====================
        //
        public ActionResult NewRecord(string searchstring, string type)
        {
            if (type == null) type = "http://fogid.net/o/person";
            string nid = null;
            var umodel = new Turgunda7.Models.UserModel(Request);

            if (type == "http://fogid.net/o/cassette")
            {
                if (!umodel.IsInRole("admin")) return Error("Кассеты может создавать только администратор");
                if (string.IsNullOrEmpty(searchstring)) return Error("Имя кассеты не должно быть пустым");
                Cassette ncass = Cassette.Create(SObjects.newcassettesdirpath + searchstring, (new Turgunda7.Models.UserModel(Request)).Uuser);
                SObjects.xconfig.Add(
                    new XElement("LoadCassette", 
                        new XAttribute("write", "yes"), 
                        SObjects.newcassettesdirpath + ncass.Name));
                SObjects.SaveConfig();
                SObjects.Init();
                nid = ncass.Name + "_cassetteId";
            }
            else
            {
                nid = SObjects.CreateNewItem(searchstring, type, umodel.Uuser);
            }
            if (nid == null) return Error("Не создан элемент, возможно, у Вас нет полномочий");
            //return RedirectToAction("Portrait", "Home", new { id = nid });
            return Redirect("~/Home/Portrait?id=" + nid);
        }
        [HttpGet]
        public IActionResult ConnectUser(string userlogin, string id)
        {
            var umodel = new Turgunda7.Models.UserModel(Request);
            if (!umodel.IsInRole("admin")) return Error("Кассеты может создавать только администратор");
            Cassette cassette = SObjects.GetCassetteInfoById(id).cassette;
            var elements = cassette.AddNewRdf(userlogin);
            foreach (var item in elements)
            {
                var item_corrected = Polar.Cassettes.DocumentStorage.DbAdapter.ConvertXElement(item);
                SObjects.PutItemToDb(item_corrected, false, (new Turgunda7.Models.UserModel(Request)).Uuser);
                cassette.db.Add(item_corrected);
            }
            cassette.Save();
            SObjects.Init();
            //return View("Info", $"Пользователь {userlogin} подключен к кассете {cass.Name}");
            return Redirect("~/Home/Index");
        }
        public ActionResult AddInvRelation(string bid, string prop, string rtype)
        {
            SObjects.AddInvRelation(bid, prop, rtype, (new Turgunda7.Models.UserModel(Request)).Uuser);
            //string nid = StaticObjects.CreateNewItem(searchstring, type, User.Identity.Name);
            //return RedirectToAction("Portrait", "Home", new { id = bid });
            return Redirect("~/Home/Portrait?id=" + bid);

        }

        [HttpGet]
        public PartialViewResult EditForm(Turgunda7.Models.RecordModel rmodel)
        {
            //string chk = Request.Params["chk"]; // проверка ОДНОЙ введенной связи
            //string ok = Request.Params["ok"]; // фиксация изменений, запоминание в БД
            //string canc = Request.Params["canc"]; // Отмена редактирования
            string chk = Request.Params("chk"); // проверка ОДНОЙ введенной связи
            string ok = Request.Params("ok"); // фиксация изменений, запоминание в БД
            string canc = Request.Params("canc"); // Отмена редактирования

            //string p_4 = Request["p_4"];
            //string p_4_ = Request.QueryString["p_4"];

            if (rmodel.firsttime)
            { // формирование формы редактирования
                rmodel.firsttime = false;
                bool replacemode = true;
                if (rmodel.eid == "create888")
                {
                    string eid = SObjects.AddInvRelation(rmodel.bid, rmodel.iprop, rmodel.etype,
                        (new Turgunda7.Models.UserModel(Request)).Uuser);
                    rmodel.eid = eid;
                    replacemode = false;
                }
                rmodel.LoadFromDb();
                rmodel.MakeLocalRecord();
                //XElement[] arr = rmodel.GetHeaderFlow().ToArray();
                //rmodel.MakeXResult();
                if (!replacemode) ViewData["insertnewrelation"] = rmodel.eid;
            }
            else if (canc != null)
            {
                rmodel.LoadFromDb();
                rmodel.MakeLocalRecord();
                return PartialView("EditFormFinal", rmodel);
            }
            else
            { // Собирание данных из реквеста
                XElement format = rmodel.CalculateFormat();
                XElement[] hrows = rmodel.GetHeaderFlow().ToArray();
                if (chk != null)
                {   // Проверка. Находим первый ряд такой, что: а) это прямое отношение, б) набран текст и есть тип, в) нет ссылки. 
                    // Делаем поисковый запрос через SearchModel. Результаты SearchResult[] помещаем в ViewData под именем searchresults,
                    // а в ViewData["searchindex"] поместим индекс
                    var pair = hrows.Select((hr, ind) => new { hr, ind })
                        .FirstOrDefault(hrind =>
                        {
                            var hr = hrind.hr;
                            if (hr.Name != "d") return false;
                            var ind = hrind.ind;
                            if (string.IsNullOrEmpty(rmodel.GetFValue(ind)) || string.IsNullOrEmpty(rmodel.GetTValue(ind))) return false;
                            if (!string.IsNullOrEmpty(rmodel.GetPValue(ind))) return false;
                            return true;
                        });
                    if (pair != null)
                    {
                        int ind = pair.ind;
                        // Ничего проверять не буду
                        Turgunda7.Models.SearchModel sm = new Models.SearchModel(rmodel.GetFValue(ind), rmodel.GetTValue(ind));
                        ViewData["searchresults"] = sm.Results;
                        ViewData["searchindex"] = ind;
                    }
                }
                else if (ok != null)
                { // Запоминание
                    // Соберем получившуюся запись
                    XElement record = new XElement(ONames.GetXName(rmodel.etype),
                        new XAttribute(ONames.rdfabout, rmodel.eid),
                        hrows.Select((fd, ind) => new { fd, ind })
                        .Select(fdind =>
                        {
                            XElement fd = fdind.fd;
                            int ind = fdind.ind;
                            XName xprop = ONames.GetXName(fd.Attribute("prop").Value);
                            if (fd.Name == "f")
                            {
                                string value = rmodel.GetFValue(ind);
                                if (true || !string.IsNullOrEmpty(value)) // Эта проверка мешает(ла) уничтожению полей
                                {
                                    // Надо определить еще нужен ли язык и какой
                                    bool istext = TurgundaCommon.ModelCommon.IsTextField(fd.Attribute("prop").Value);
                                    return new XElement(xprop, rmodel.GetFValue(ind),
                                        istext ? new XAttribute(ONames.xmllang, rmodel.lang) : null);
                                }
                            }
                            else if (fd.Name == "d")
                            {
                                string pvalue = rmodel.GetPValue(ind);
                                if (!string.IsNullOrEmpty(pvalue))
                                    return new XElement(xprop,
                                    new XAttribute(ONames.rdfresource, rmodel.GetPValue(ind)));
                            }
                            return (XElement)null;
                        }));
                    // Пошлем эту запись на изменение
                    SObjects.PutItemToDb(record, false, (new Turgunda7.Models.UserModel(Request)).Uuser);
                    // Если эта запись является записью типа "DocumentPart", то фиксируем две величины:
                    // ссылку inDocument и идентификатор, имеющийся "за" этой ссылкой
                    if (record.Name.LocalName == "DocumentPart" && record.Name.NamespaceName == "http://fogid.net/o/")
                    {
                        var resource_el = record.Element("{http://fogid.net/o/}inDocument");
                        if (resource_el != null)
                        {
                            string pvalue = resource_el.Attribute(ONames.rdfresource).Value;
                            //HttpContext.Session.SetString("inDocumentId", pvalue);
                            int ind = 0;
                            for (; ind < Turgunda7.Models.RecordModel.nfields; ind++)
                                if (rmodel.GetPValue(ind) == pvalue) break;
                            if (ind < Turgunda7.Models.RecordModel.nfields)
                            {
                                //var s1 = rmodel.GetFValue(ind);
                                //var s2 = rmodel.GetPValue(ind);
                                //var s3 = rmodel.GetTValue(ind);
                                //var s4 = rmodel.GetVValue(ind);

                                //HttpContext.Session.SetString("inDocumentName", rmodel.GetVValue(ind));
                            }
                        }
                    }

                    return PartialView("EditFormFinal", rmodel);
                }
                else if (rmodel.command != null && rmodel.command == "SetVariant")
                { // Выбор варианта значения для связывания
                    string[] parts = rmodel.exchange.Split('|');
                    int ind = Int32.Parse(parts[0]);
                    string p_id = parts[1];
                    string p_name = System.Web.HttpUtility.UrlDecode(parts[2]);
                    rmodel.SetPValue(ind, p_id);
                    rmodel.SetVValue(ind, p_name);
                    rmodel.CalculateFormat();
                }
                else if (rmodel.command != null && rmodel.command == "SetVariantNew")
                { // Связывание с новым значением
                    string[] parts = rmodel.exchange.Split('|');
                    int ind = Int32.Parse(parts[0]);
                    string p_type = parts[1];
                    string p_name = System.Web.HttpUtility.UrlDecode(parts[2]);
                    string nid = SObjects.CreateNewItem(p_name, p_type, (new Turgunda7.Models.UserModel(Request)).Uuser);
                    rmodel.SetPValue(ind, nid);
                    rmodel.SetVValue(ind, p_name);
                    rmodel.CalculateFormat();
                }
                else
                { // Остальное

                }
            }
            return PartialView("EditForm", rmodel);
        }
        //[HttpPost]
        public PartialViewResult SetVariant(Turgunda7.Models.RecordModel rmodel)
        {
            //Turgunda2.Models.RecordModel rmodel = new Models.RecordModel();
            //return PartialView("EditForm", rmodel);
            return EditForm(rmodel);
        }
        public PartialViewResult DeleteRow(string eid)
        {
            SObjects.DeleteItem(eid, (new Turgunda7.Models.UserModel(Request)).Uuser);
            return PartialView();
        }

        // ========================= Конфигурирование ========================
        public IActionResult UserConfiguration()
        {
            var usermodel = new Models.UserModel(Request);
            var pars = Request.Query.ToArray();
            if (pars.Length > 0)
            {
                string acti = null;
                foreach (var p in pars)
                {
                    string pkey = p.Key;
                    if (pkey == "radio") acti = p.Value;
                }
                if (acti != null)
                {
                    if (usermodel.SetActiveCassette(acti)) usermodel.Save();
                }
            }
            List<Models.Conf> list = new List<Models.Conf>();
            foreach (var c in SObjects.Engine.localstorage.connection.GetFogFiles1())
            {
                list.Add(new Models.Conf() { Cass = c.cassette, Editable = c.isEditable, Owner = c.owner,
                    Active = usermodel.ActiveCassette() });
                
            }

            //Turgunda7.Models.ConfigurationModel cmodel = new Models.ConfigurationModel(fogs);
            return View("UserConfiguration", new Models.UserConfigurationModel() { Confs = list.ToArray() });
        }
        public IActionResult SystemConfiguration()
        {
            var pars = Request.Query.ToArray();
            XElement[] loadcassetteelements = null;
            // i-ый элемент соответствует i-му параметру
            bool[] deletes = null;
            bool[] towrites = null;
            string addcassette = null;
            bool awrite = false;
            foreach (var p in pars)
            {
                if (loadcassetteelements == null)
                {
                    loadcassetteelements = SObjects.xconfig.Elements("LoadCassette").ToArray();
                    deletes = new bool[loadcassetteelements.Length];
                    towrites = new bool[loadcassetteelements.Length];
                }
                var name = p.Key;
                var val = p.Value.FirstOrDefault();
                char c = name[0];
                if (char.IsDigit(c))
                { // Открепление кассеты
                    int nom = Int32.Parse(name);
                    //Console.WriteLine($"============================== deleting cass {nom}");
                    deletes[nom] = true;
                }
                else if (c == 'w')
                {
                    int nom = Int32.Parse(name.Substring(1));
                    //Console.WriteLine($"============================== changing write status cass {nom}");
                    towrites[nom] = true;
                }
                else if (name == "addition")
                {
                    addcassette = val;
                }
                else if (name == "awrite")
                {
                    awrite = true;
                }
            }
            bool changed = false;
            if (loadcassetteelements != null)
            { // зафиксировать результат
                for (int i=0; i<loadcassetteelements.Length; i++)
                {
                    // коррекция атрибута write
                    XElement el = loadcassetteelements[i];
                    bool towrite = towrites[i];
                    string wr_value = towrite ? "yes" : "no";
                    XAttribute wr_att = el.Attribute("write");
                    if (wr_att == null) { el.Add(new XAttribute("write", wr_value)); changed = true; }
                    else { changed = wr_att.Value != wr_value; wr_att.Value = wr_value; }
                    // уничтожение, если заказано
                    if (deletes[i]) { el.Remove(); changed = true; }
                }
            }
            if (addcassette != null)
            {
                if (Directory.Exists(addcassette) && System.IO.File.Exists(addcassette + "/cassette.finfo"))
                {
                    SObjects.xconfig.Add(
                        new XElement("LoadCassette", 
                            new XAttribute("write", awrite ? "yes" : "no"),
                            addcassette));
                    changed = true;
                }
            }
            if (changed)
            {
                SObjects.SaveConfig();
                SObjects.Init();
            }

            return View("SystemConfiguration");
        }

        // ====================== Экспресс-вариант портрета ========================
        public IActionResult P(string id)
        {
            string tilda = HttpContext.Request.PathBase;
            ContentResult cr = new ContentResult() { ContentType = "text/html" };
            XElement html = new XElement("html",
                new XElement("head",
                    new XElement("meta", new XAttribute("charset", "utf-8")),
                    new XElement("link", new XAttribute("rel", "stylesheet"), new XAttribute("href", tilda+"/css/site.css")),
                    new XElement("script", new XAttribute("type", "text/javascript"), " ")),
                new XElement("body", 
                    new XElement("table", new XAttribute("width", "100%"), new XAttribute("border", "0"), 
                        new XAttribute("style", "margin-top:10px;"),
                        new XElement("tbody",
                            new XElement("tr", new XAttribute("valign", "top"),
                                new XElement("td", new XAttribute("rowspan", "2"),
                                    new XElement("div", new XAttribute("style", "width:120px;"), new XAttribute("align", "center"),
                                        new XElement("a", new XAttribute("href", tilda + "/Home/Index"),
                                            new XElement("img", new XAttribute("src", tilda + "/images/logo1.jpg"))))),
                                new XElement("td",
                                    new XElement("div",
                                        new XElement("a", new XAttribute("href", tilda + "/Home/index"), "Начало"))),
                                new XElement("td", new XAttribute("width", "100%"),
                                    new XElement("h2", new XAttribute("style", "color:#5c87b2;padding: 0 0 0 0;") ,"Универсальный редактор")),
                                new XElement("td",
                                    new XElement("div", new XAttribute("style", "width:100px; text-align:right;"), 
                                        new XElement("a", new XAttribute("href", tilda + "/Home/index"), "ред"))),
                                null),
                            new XElement("tr", new XAttribute("valign", "bottom"),
                                new XElement("td", new XAttribute("colspan", "3"),
                                    new XElement("div", new XAttribute("style", "vertical-align:bottom;"),
                                        new XElement("form", new XAttribute("method", "get"), new XAttribute("action", tilda+"/Home/Srch"),
                                            new XAttribute("style", "margin:0;"),
                                            new XElement("input", new XAttribute("type", "text"), new XAttribute("name", "searchstring"), 
                                                new XAttribute("style", "width:300px;")),
                                            new XElement("select", new XAttribute("name", "choosetype"),
                                                new XElement("option", " "),
                                                new XElement("option", new XAttribute("value", "cheburashka"), "Чебурашка")),
                                            new XElement("input", new XAttribute("type", "submit"), new XAttribute("value", "искать"),
                                                new XAttribute("style", "font-size:small;"))))))
                                                ))));
            cr.Content = "<!DOCTYPE html>\n" + html.ToString(); // (SaveOptions.DisableFormatting);
            return cr;
        }

    }
}