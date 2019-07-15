using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OADataService.Controllers
{
    public class DocsController : Controller
    {
        // Кострукция uri: iiss://cassette_name@iis.nsk.su/0001/DDDD/DDDD
        [HttpGet("docs/photo")]
        public IActionResult GetImage(string uri, string method)
        {
            var cass_dir = CassettesConfiguration.CassDirPath(uri);
            if (cass_dir == null) return NotFound();
            string last10 = uri.Substring(uri.Length - 10);
            string subpath;
            if (method == null) subpath = "/originals";
            else if (method == "s") subpath = "/documents/small";
            else if (method == "m") subpath = "/documents/medium";
            else subpath = "/documents/normal"; // (method == "n")
            string path = cass_dir + subpath + last10 + ".jpg";
            //Byte[] b = System.IO.File.ReadAllBytes(path);
            //return File(b, "image/png");
            return PhysicalFile(path, "image/jpg");
        }

        public IActionResult GetPhoto(string u, string s)
        {
            string filename = @"C:\Users\SSYP\Pictures\Arc.jpg";
            //filename = Turgunda7.SObjects.Engine.localstorage.GetPhotoFileName(u, s) + ".jpg";
            return new FileStreamResult(new FileStream(filename, FileMode.Open, FileAccess.Read), "image/jpeg");
                //(filename, System.IO.FileAccess.Read), "image/jpeg");
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
        [HttpGet("[controller]/xml/{id}")]
        public ContentResult GetXml(int id)
        {
            return Content("<roo><el1>text 1. текст 1</el1><el2 att='attribute 1'></el2></roo>", "text/xml");
        }
        [HttpGet("[controller]/pdf/{id}")]
        public FileStreamResult GetPdf(int id)
        {
            string path = @"D:\Home\work\Пономарев_отчет-2017 (1).pdf";
            return File(System.IO.File.Open(path, FileMode.Open, FileAccess.Read), "application/pdf");
        }

        public FileStreamResult Download(string filename)
        {
            string path = @"C:\home\syp\syp2019\docAsp.net.pdf";
            return File(System.IO.File.Open(path, FileMode.Open, FileAccess.Read), "application/octet-stream");
        }
        public FileContentResult GetFile(string filename)
        {
            string path = @"C:\home\syp\syp2019\docAsp.net.pdf";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", "look2.bin");
        }

    }
}
