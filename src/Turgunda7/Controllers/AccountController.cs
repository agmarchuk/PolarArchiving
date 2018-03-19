using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Turgunda7.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Logon()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Logon(string uuser, string pass)
        {
            //var q = new Microsoft.AspNetCore.Http.HttpResponse();
            //this.Request;
            Turgunda7.Models.UserModel umodel = new Models.UserModel(this.Request);
            umodel.ActivateUserMode(this.Response, uuser);
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Logout()
        {
            Turgunda7.Models.UserModel umodel = new Models.UserModel(this.Request);
            umodel.DeactivateUserMode(this.Response);
            return RedirectToAction("Index", "Home");
        }
    }
}