using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoranCore2.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class DbController : Controller
    {
        [HttpGet]
        public ContentResult Ping()
        {
            return Content("Pong", "text/plain");
        }
        [HttpGet]
        public ContentResult Load()
        {
            OAData.OADB.Load();
            return Content("Loaded", "text/plain");
        }
        [HttpGet]
        public ContentResult Reload()
        {
            OAData.OADB.Reload();
            return Content("Reloaded", "text/plain");
        }
        [HttpGet]
        public ContentResult GetConfig()
        {
            XElement result = OAData.OADB.XConfig;
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }

        [HttpGet]
        public ContentResult SearchByName(string ss, string tt)
        {
            XElement results = new XElement("results");
            IEnumerable<XElement> query = OAData.OADB.SearchByName(ss);
            if (tt != null) query = query.Where(x => x.Attribute("type")?.Value == tt);
            foreach (XElement result in query) results.Add(result);
            return Content(results.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpGet]
        public ContentResult GetItemByIdBasic(string id, string addinverse)
        {

            bool ai = (addinverse == "on" || addinverse == "true") ? true : false;
            XElement result = OAData.OADB.GetItemByIdBasic(id, ai);
            if (result == null) result = new XElement("error");
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpPost]
        public ContentResult GetItemById(string id, string format)
        {
            XElement result = OAData.OADB.GetItemById(id, XElement.Parse(format));
            if (result == null) result = new XElement("error");
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpPost]
        public ContentResult PutItem(string item)
        {
            XElement xitem = XElement.Parse(item);
            XElement result = OAData.OADB.PutItem(xitem);
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }
        [HttpPost]
        public ContentResult UpdateItem(string item)
        {
            XElement xitem = XElement.Parse(item);
            XElement result = OAData.OADB.UpdateItem(xitem);
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Обработка на запрос портрета персоны
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ContentResult GetPortrait(string id)
        {
            XElement result = OAData.OADB.GetItemById(id, fPortrait);
            if (result == null) result = new XElement("error");
            return Content(result.ToString(), "text/xml", System.Text.Encoding.UTF8);
        }

        private XElement fPortrait =
            new XElement("record", new XAttribute("type", "http://fogid.net/o/person"),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/has-title"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/degree")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            null)),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                                    new XElement("record", new XAttribute("type", "http://fogid.net/o/photo-doc"),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/participant"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role-classification")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/role")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-org"),
                                    new XElement("record",
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                        new XElement("field", new XAttribute("prop", "http://fogid.net/o/org-classification")),
                                        null)))),
                        new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/referred-sys"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/alias")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                                null)),
                        null);
    }


}
