using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Turgunda7.Controllers
{
    public class ServController : Controller
    {
        public ActionResult GetPhoto(string u, string s)
        {
            string filename = "/question.jpg";
            filename = SObjects.Engine.storage.GetPhotoFileName(u, s) + ".jpg";
            //return new FilePathResult(filename, "image/jpeg");
            return new PhysicalFileResult(filename, "image/jpeg");

        }

        [HttpGet]
        public ActionResult SearchByName(string ss)
        {
            string name = ss;
            XElement res = new XElement("sequense", SObjects.Engine.SearchByName(name));
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr; 
        }
        [HttpGet]
        public ActionResult GetItemByIdSpecial(string id)
        {
            XElement res = SObjects.Engine.GetItemByIdSpecial(id);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }
        [HttpGet]
        public ActionResult GetItemByIdBasic(string id, bool addinverse)
        {
            XElement res = SObjects.Engine.GetItemByIdBasic(id, addinverse);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }
        [HttpGet]
        public ActionResult Delete(string id)
        {
            XElement res = SObjects.Engine.Delete(id);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }
        [HttpPost]
        public ActionResult GetItemById(string id, XElement format)
        {
            XElement res = SObjects.Engine.GetItemById(id, format);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }
        [HttpPost]
        public ActionResult Add(XElement record)
        {
            XElement res = SObjects.Engine.Add(record);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }
        [HttpPost]
        public ActionResult AddUpdate(XElement record)
        {
            XElement res = SObjects.Engine.AddUpdate(record);
            var cr = new ContentResult();
            cr.ContentType = "text/xml";
            cr.Content = res.ToString();
            return cr;
        }

    }
}
