using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OpenArchiveClient.Models;

namespace OpenArchiveClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", new IndexModel());
        }
        [HttpPost]
        public IActionResult Search()
        {
            return View();
        }
        public IActionResult ExtSearch()
        {
            return View("ExtSearch", new ExtSearchModel());
        }
        [HttpPost]
        public IActionResult ExtSearch(string fund, string context, string person, string org, string coll, string geo, string fdate, string tdate)
        {
            return View("ExtSearch", new ExtSearchModel(fund, context, person, org, coll, geo, fdate, tdate));
        }
        [HttpGet]
        public IActionResult Portrait(string id)
        {
            DateTime tt0 = DateTime.Now;

            //string id = Request.Query["id"];//.Params["id"];
            if (string.IsNullOrEmpty(id)) { return new RedirectResult("~/Home/Index"); }

            XElement special = SObs.GetItemByIdBasic(id, false);
            if (special == null || special.Attribute("type") == null) { return new RedirectResult("~/Home/Index"); }
            string type = special.Attribute("type").Value;

            if (type == "http://fogid.net/o/person")
            {
                return View("PortraitPerson", new PortraitPersonModel(id));
            }
            else if (type == "http://fogid.net/o/collection")
            {
                return View("PortraitCollection", new PortraitCollectionModel(id));
            }
            else if (type == "http://fogid.net/o/org-sys")
            {
                return View("PortraitOrg", new PortraitOrgModel(id));
            }
            else if (type == "http://fogid.net/o/document" || type == "http://fogid.net/o/photo-doc")
            {
                return View("PortraitDocument", new PortraitDocumentModel(id));
            }
            else if (type == "http://fogid.net/o/city" || type == "http://fogid.net/o/country")
            {
                return View("PortraitGeo", new PortraitGeoModel(id));
            }
            else
            {
                //@RenderPage("PortraitAny.cshtml", new { id = id, type = type })
            }

            return View();
        }


        [HttpGet]
        public IActionResult DocumentImage(string id, string eid)
        {
            return View("DocumentImage", new DocumentImageModel(id, eid));
        }





        public IActionResult About()
        {
            return View("About");
        }
        public IActionResult Participants()
        {
            return View("Participants");
        }

        public IActionResult Contacts()
        {
            return View("Contacts");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
