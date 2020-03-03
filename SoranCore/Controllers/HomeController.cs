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
                List<object[]> fields_list = new List<object[]>();
                List<object[]> naming_list = new List<object[]>();
                List<object[]> description_list = null;
                List<object[]> doclist_list = new List<object[]>();
                //SGraph.SNode portraitphoto = null;
                List<object[]> firstfaces_list = new List<object[]>();
                List<object[]> participants_list = new List<object[]>();
                List<object[]> orgchilds_list = new List<object[]>();
                List<object[]> titles_list = new List<object[]>();
                List<object[]> works_list = null;
                List<object[]> org_events_list = null;
                List<object[]> incollection_list = null;
                List<object[]> reflections_list = null;
                List<object[]> authors_list = null;
                List<object[]> archive_list = null;
                var xtree = OAData.OADB.GetItemByIdBasic(id, true);
                //model.Name = xtree.Elements("field").Where(f => f.Attribute("prop").Value == "http://fogid.net/o/name").FirstOrDefault()?.Value;
                foreach (XElement el in xtree.Elements("field"))
                {
                    string prop = el.Attribute("prop").Value;
                    if (prop == "http://fogid.net/o/name") model.Name = el.Value;
                    else if (prop == "http://fogid.net/o/from-date") model.StartDate = el.Value;
                    else if (prop == "http://fogid.net/o/to-date") model.EndDate = el.Value;
                    else if (prop == "http://fogid.net/o/description") model.Description = el.Value;
                }
                foreach (XElement el in xtree.Elements("inverse"))
                {
                    string prop = el.Attribute("prop").Value;
                    if (prop == "http://fogid.net/o/reflected")
                    {
                        string source = el.Element("record").Attribute("id").Value;
                        XElement relation = OAData.OADB.GetItemByIdBasic(source, false);

                    }
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
