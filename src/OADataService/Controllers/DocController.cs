using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OADataService.Controllers
{
    public class DocController : Controller
    {
        public IActionResult GetPhoto(string u, string s)
        {
            string filename = @"C:\Users\SSYP\Pictures\Arc.jpg";
            //filename = Turgunda7.SObjects.Engine.localstorage.GetPhotoFileName(u, s) + ".jpg";
            return new FileStreamResult(new FileStream(filename, FileMode.Open, FileAccess.Read), "image/jpeg");
                //(filename, System.IO.FileAccess.Read), "image/jpeg");
        }
        [HttpGet("[controller]/pic/{id}")]
        public IActionResult GetImage(int id)
        {
            //var contentRoot = _env.ContentRootPath + "//Pics";
            //var path = Path.Combine(contentRoot, id + ".png");
            string path = @"C:\Users\SSYP\Pictures\Arc.png";
            //Byte[] b = System.IO.File.ReadAllBytes(path);
            //return File(b, "image/png");
            return PhysicalFile(path, "image/png");
        }
        [HttpGet("[controller]/html/{id}")]
        public IActionResult GetHtml(int id)
        {
            ContentResult cr = new ContentResult() { ContentType = "text/html" };
            cr.Content = "<html><head><meta charset='utf-8'/></head><body><h1>Hello! Привет!</h1></body></html>";
            return cr;
        }
        [HttpGet("[controller]/json/{id}")]
        public ContentResult GetJson(int id)
        {
            return Content("[125]",  "application/json");
        }
    }
}
