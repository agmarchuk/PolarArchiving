using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SoranCore.Models;

namespace SoranCore.Controllers
{
    public class HomeController : Controller
    {
        //static string home_id = "w20070417_7_1744"; // "pahomepage";
        //static string soran_id = "w20070417_3_6186";
        //static string database_id = "w20070417_4_1010";
        //static string about_id = "w20070417_3_4658";
        //static string sitecollection = "w20070417_4_1027";
        //static string photocollections = "w20070417_4_1012";
        //static string usefullLinks_id = "usefullLinks";
        //static string newspaper_id = "newspaper_cassetteId";

        public IActionResult Index()
        {
            var model = new IndexModel();

            string sdir = HttpContext.Request.Query["sdirection"].FirstOrDefault();
            if (sdir == null)
            {
                sdir = HttpContext.Session.GetString("sdirection");
                if (sdir == null) sdir = "person"; 
            }
            if (sdir != null) model.TabDirection = sdir;
            HttpContext.Session.SetString("sdirection", sdir);

            string p = HttpContext.Request.Query["p"].FirstOrDefault();
            string id = HttpContext.Request.Query["id"].FirstOrDefault();
            if (p == null && id == null) id = "w20070417_7_1744";
            XElement xrec = null;
            if (p == "search")
            {
                string searchstring = HttpContext.Request.Query["searchstring"].FirstOrDefault();
                IEnumerable<XElement> query = OAData.OADB.SearchByName(searchstring);
                var list = new List<object[]>();
                foreach (XElement el in query)
                {
                    string t = el.Attribute("type").Value;
                    string name = el.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name")?.Value;
                    if (t == "http://fogid.net/o/person")
                    {
                        list.Add(new object[] { el.Attribute("id").Value, name });
                    }
                }
                model.SearchResults = list;
            }
            else if (id != null && (xrec = OAData.OADB.GetItemByIdBasic(id, false)) != null) // Построение портрета
            {
                // Сначала надо выяснить тип сущности, для этого запрашиваем ее без формата
                model.Id = xrec.Attribute("id").Value;
                model.Type = xrec.Attribute("type").Value;
                // Заодно вычислим важные поля
                //foreach (XElement el in xrec.Elements("field"))
                //{
                //    string prop = el.Attribute("prop").Value;
                //    if (prop == "http://fogid.net/o/name") model.Name = el.Value;
                //    else if (prop == "http://fogid.net/o/from-date") model.StartDate = el.Value;
                //    else if (prop == "http://fogid.net/o/to-date") model.EndDate = el.Value;
                //    else if (prop == "http://fogid.net/o/description") model.Description = el.Value;
                //    else if (prop == "http://fogid.net/o/uri") model.Uri = el.Value;
                //}
                model.Name = StaticObjects.GetField(xrec, "http://fogid.net/o/name");
                model.StartDate = StaticObjects.GetField(xrec, "http://fogid.net/o/from-date");
                model.EndDate = StaticObjects.GetField(xrec, "http://fogid.net/o/to-date");
                model.Description = StaticObjects.GetField(xrec, "http://fogid.net/o/description");
                model.Uri = StaticObjects.GetField(xrec, "http://fogid.net/o/uri");
                model.Docmetainfo = StaticObjects.GetField(xrec, "http://fogid.net/o/docmetainfo");

                // Теперь определим формат для этого типа
                if (model.Type == "http://fogid.net/o/photo-doc")
                {
                    model.XRecord = OAData.OADB.GetItemById(id, model.Formats["http://fogid.net/o/document"]);
                }
                else if (model.Formats.ContainsKey(model.Type))
                {
                    XElement format = model.Formats[model.Type];
                    model.XRecord = OAData.OADB.GetItemById(id, format);
                }
                else
                {
                    model.XRecord = xrec;
                }
            }

            return View(model);
        }

        public IActionResult Show()
        {
            string id = HttpContext.Request.Query["id"].FirstOrDefault();
            if (id == null) return Redirect("Index");
            ShowModel model = new ShowModel(id);
            if (model.Rec == null) return Redirect("~/Home/Index");
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
