using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OpenArchiveClient.Controllers
{
    public class Room216Controller : Controller
    {
        public IActionResult Load()
        {
            var requ = Request;
            // Можно обойтись без принудительной загрузки вначале, только надо проверить инициированность(?)
            //if (OAData.OADB.adapter != null) OAData.OADB.adapter.Close();
            //System.Xml.Linq.XElement xconfig = System.Xml.Linq.XElement.Load("wwwroot/config.xml");
            //OAData.OADB conf = new OAData.OADB(xconfig);
            OAData.OADB.Reload();

            return new RedirectResult("~/Home/Index");
        }
    }
}