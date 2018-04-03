using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenArchiveMVC.Models;

namespace OpenArchiveMVC.Controllers
{
    public class DocsController : Controller
    {
        [HttpGet]
        public IActionResult GetPhoto(string s, string u)
        {
            string filename = OpenArchive.StaticObjects.storage.GetPhotoFileName(u, s) + ".jpg";
            return new PhysicalFileResult(filename, "image/jpeg");
        }
    }
}
