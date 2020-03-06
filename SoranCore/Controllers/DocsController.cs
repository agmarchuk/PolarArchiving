using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SoranCore.Controllers
{
    public class DocsController : Controller
    {
        [HttpGet("docs/GetPhoto")]
        public IActionResult GetPhoto(string u, string s)
        {
            if (u == null) return NotFound();
            var cass_dir = OAData.OADB.CassDirPath(u);
            if (cass_dir == null) return NotFound();
            string last10 = u.Substring(u.Length - 10);
            string subpath;
            string method = s;
            //if (method == null) subpath = "/originals";
            if (method == "small") subpath = "/documents/small";
            else if (method == "medium") subpath = "/documents/medium";
            else subpath = "/documents/normal"; // (method == "n")
            string path = cass_dir + subpath + last10 + ".jpg";
            return PhysicalFile(path, "image/jpg");
        }
    }
}