using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSeminskiy.Controllers
{
    public class Room216Controller : Controller
    {
        public IActionResult Load(int nom)
        {
            //OAData.OADB.Close();
            if (nom == 1) OAData.OADB.configfilename = "config1.xml";
            else OAData.OADB.configfilename = "config.xml";
            SObjects.Init();

            return Redirect("~/Home/Index");
        }
    }
}
