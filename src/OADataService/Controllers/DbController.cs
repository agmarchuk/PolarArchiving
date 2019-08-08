using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

using OAData;

namespace OADataService.Controllers
{
    public class DbController : Controller
    {
        //// POST db/GetById?id=идентификатор&f=...
        //[HttpPost]7
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}
        // Формат результата: <results>...</results>
        [HttpGet]
        public ContentResult Ping()
        {
            return Content("Pong", "text/plain");
        }

        [HttpGet]
        public ContentResult SearchByName(string ss, string tt)
        {
            XElement results = new XElement("results");
            IEnumerable<XElement> query = OAData.OAData.SearchByName(ss);
            if (tt != null) query = query.Where(x => x.Attribute("type")?.Value == tt);
            foreach (XElement result in query) results.Add(result);
            return Content(results.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpGet]
        public ContentResult GetItemByIdBasic(string id, bool? addinverse)
        {
            bool ai = addinverse.HasValue ? addinverse.Value: false;
            //if (addinverse == null) ai = false;
            XElement result = OAData.OAData.GetItemByIdBasic(id, ai);
            if (result == null) result = new XElement("error");
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpPost]
        public ContentResult GetItemById(string id, string format)
        {
            XElement result = OAData.OAData.GetItemById(id, XElement.Parse(format));
            if (result == null) result = new XElement("error");
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpPost]
        public ContentResult PutItem(string item)
        {
            XElement xitem = XElement.Parse(item);
            XElement result = OAData.OAData.PutItem(xitem);
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
    }
}