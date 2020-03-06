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
            else if (id != null) // Построение портрета
            {
                // Сначала надо выяснить тип сущности, для этого запрашиваем ее без формата
                XElement xrec = OAData.OADB.GetItemByIdBasic(id, false);
                model.Id = xrec.Attribute("id").Value;
                model.Type = xrec.Attribute("type").Value;
                // Заодно вычислим важные поля
                foreach (XElement el in xrec.Elements("field"))
                {
                    string prop = el.Attribute("prop").Value;
                    if (prop == "http://fogid.net/o/name") model.Name = el.Value;
                    else if (prop == "http://fogid.net/o/from-date") model.StartDate = el.Value;
                    else if (prop == "http://fogid.net/o/to-date") model.EndDate = el.Value;
                    else if (prop == "http://fogid.net/o/description") model.Description = el.Value;
                    else if (prop == "http://fogid.net/o/uri") model.Uri = el.Value;
                }

                // Теперь определим формат для этого типа
                if (model.Formats.ContainsKey(model.Type))
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
