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
            if (p == "search")
            {
                model.SearchResults =  new List<object[]>()
                {
                    new object[3],
                    new object[3],
                };
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
