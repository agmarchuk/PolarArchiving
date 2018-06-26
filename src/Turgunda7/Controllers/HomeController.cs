using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Threading.Tasks;
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
            if (res == null) res = requ.Form[name];
            return res;
        }
    }
    public class HomeController : Controller
    {
        public ActionResult Error(string mess)
        {
            return View("Error", new Turgunda7.Models.ErrorModel() { mess = mess });
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
        [HttpPost]
        public ActionResult Load(string cassetteId, IFormFile file)
        {
            string filename = file.FileName;
            string fpath = @"D:\Home\data\savedfile";
            using (System.IO.FileStream fs = new System.IO.FileStream(fpath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                file.CopyTo(fs);
            }
            return View("Portrait", new Models.PortraitModel(cassetteId));
        }
        [HttpPost("UploadFiles")]
        public IActionResult Post(string id, List<IFormFile> files)
        {
            // Пользователь
            string user = (new Turgunda7.Models.UserModel(Request)).Uuser;
            // Короткое имя кассеты, если есть
            if (!id.EndsWith("_cassetteId")) { return new EmptyResult(); }
            string sid = id.Substring(0, id.Length - "_cassetteId".Length).ToLower();
            string fid = "iiss://" + sid + "@iis.nsk.su";
            // Определяем кассету
            var cassetteExists = SObjects.Engine.localstorage.connection.cassettesInfo.ContainsKey(fid);
            if (!cassetteExists) { return new EmptyResult(); }
            var cassetteInfo = SObjects.Engine.localstorage.connection.cassettesInfo[fid];
            Cassette cassette = cassetteInfo.cassette;
            string dirpath = cassetteInfo.url;
            foreach (var formFile in files)
            {
                // Сохраним документ в промежуточном месте
                string tmpfilepath = dirpath + formFile.FileName;
                using (var stream = new System.IO.FileStream(tmpfilepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    formFile.CopyTo(stream);
                }
                FileInfo fi = new FileInfo(tmpfilepath);

                // Воспользуемся "стандартной" записью файла
                var elements = CassetteExtension.AddFile(cassette, fi, id).ToArray();
                foreach (var item in elements)
                {
                    SObjects.PutItemToDb(item, false, user);
                }
                Console.WriteLine("QQQQQQQQQQQQQQQQQQQQQQQ" + elements.Length);
                // Уничтожение времянки
                System.IO.File.Delete(tmpfilepath);
            }
            cassette.Save();

            return View("Portrait", new Models.PortraitModel(id));
        }
        //[HttpPost("UploadFiles0")]
        //public async System.Threading.Tasks.Task<IActionResult> Post(string id, List<IFormFile> files)
        //{
        //    // Короткое имя кассеты, если есть
        //    if (!id.EndsWith("_cassetteId")) { return new EmptyResult(); }
        //    string sid = id.Substring(0, id.Length - "_cassetteId".Length).ToLower();
        //    string fid = "iiss://" + sid + "@iis.nsk.su";
        //    // Определяем кассету
        //    var cassetteExists = SObjects.Engine.localstorage.connection.cassettesInfo.ContainsKey(fid);
        //    if (!cassetteExists) { return new EmptyResult(); }
        //    var cassetteInfo = SObjects.Engine.localstorage.connection.cassettesInfo[fid];
        //    Cassette cassette = cassetteInfo.cassette;
        //    string dirpath = cassetteInfo.url;

        //    // Определяем важные параметры

        //    //List<Task> tasklist = new List<Task>();
        //    foreach (var formFile in files)
        //    {
        //        var foldernumb = cassette._folderNumber;
        //        var docnumb = cassette._documentNumber;
        //        string ext = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.')).ToLower();

        //        string fpath = dirpath + "originals/" + foldernumb + "/" + docnumb + ext;
        //        using (var stream = new System.IO.FileStream(fpath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        //        {
        //            //MemoryStream mstream = new MemoryStream();
        //            formFile.CopyTo(stream);
        //        }

        //        using (var stream = new System.IO.FileStream(fpath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //        using (var original = FreeImageBitmap.FromStream(stream))
        //        {
        //            stream.Position = 0L;
        //            DateTime datePictureTaken = DateTime.Now;
        //            bool exifok = false;
        //            // Использую ExifLib.Standard
        //            if (ext == ".jpg")
        //            {
        //                try
        //                {
        //                    using (ExifReader reader = new ExifReader(stream))
        //                    {
        //                        // Extract the tag data using the ExifTags enumeration
        //                        if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
        //                                                        out datePictureTaken))
        //                        {
        //                            exifok = true;
        //                        }
        //                    }
        //                }
        //                catch (Exception) { }
        //            }


        //            var fi = new FileInfo(fpath);
        //            var md = fi.CreationTimeUtc;
        //            //original.Save(fpath);
        //            Console.WriteLine($"Width={original.Width} Height={original.Height} ImageFormat={original.ImageFormat} {datePictureTaken} ");
                    
        //            //string previewpath = dirpath + "documents/small/" + foldernumb + "/" + docnumb + ".jpg";
        //            Sizing(original, 150, dirpath + "documents/small/" + foldernumb + "/" + docnumb + ".jpg");
        //            Sizing(original, 600, dirpath + "documents/medium/" + foldernumb + "/" + docnumb + ".jpg");
        //            Sizing(original, 1200, dirpath + "documents/normal/" + foldernumb + "/" + docnumb + ".jpg");
        //            //FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYGOOD |
        //            //FREE_IMAGE_SAVE_FLAGS.JPEG_BASELINE);
        //        }

        //        //tasklist.Add(BuildImageFile(formFile, dirpath + "originals/" + foldernumb + "/" + docnumb + ext));
        //        // Следующий документ
        //        cassette.IncrementDocumentNumber();
        //    }
        //    //Task.WaitAll(tasklist.ToArray());

        //    // process uploaded files
        //    // Don't rely on or trust the FileName property without validation.

        //    //return Ok(new { count = files.Count, size, filePath });
        //    return View("Portrait", new Models.PortraitModel(id));
        //}

        //private static void Sizing(FreeImageBitmap original, int maxbase, string previewpath)
        //{
        //    int x = original.Width, y = original.Height;
        //    double factor = ((double)maxbase) / (x > y ? (double)x : (double)y);
        //    int width = (int)(factor * x);
        //    int height = (int)(factor * y);
        //    var resized = new FreeImageBitmap(original, width, height);
        //    // JPEG_QUALITYGOOD is 75 JPEG.
        //    // JPEG_BASELINE strips metadata (EXIF, etc.)
        //    resized.Save(previewpath);//, FREE_IMAGE_FORMAT.FIF_JPEG,
        //}

        private async Task BuildImageFile(IFormFile formFile, string fpath)
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
            string nid = SObjects.CreateNewItem(searchstring, type, (new Turgunda7.Models.UserModel(Request)).Uuser);
            return RedirectToAction("Portrait", "Home", new { id = nid });
        }
        public ActionResult AddInvRelation(string bid, string prop, string rtype)
        {
            SObjects.AddInvRelation(bid, prop, rtype, (new Turgunda7.Models.UserModel(Request)).Uuser);
            //string nid = StaticObjects.CreateNewItem(searchstring, type, User.Identity.Name);
            return RedirectToAction("Portrait", "Home", new { id = bid });
        }

        //[HttpPost]
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
                                    bool istext = Turgunda7.Models.Common.IsTextField(fd.Attribute("prop").Value);
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
                            HttpContext.Session.SetString("inDocumentId", pvalue);
                            int ind = 0;
                            for (; ind < Turgunda7.Models.RecordModel.nfields; ind++)
                                if (rmodel.GetPValue(ind) == pvalue) break;
                            if (ind < Turgunda7.Models.RecordModel.nfields)
                            {
                                //var s1 = rmodel.GetFValue(ind);
                                //var s2 = rmodel.GetPValue(ind);
                                //var s3 = rmodel.GetTValue(ind);
                                //var s4 = rmodel.GetVValue(ind);

                                HttpContext.Session.SetString("inDocumentName", rmodel.GetVValue(ind));
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

    }
}