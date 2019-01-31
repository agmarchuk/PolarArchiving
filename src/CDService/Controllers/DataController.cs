using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDService.Controllers
{
    //[Route("data")]
    //[ApiController]
    public class DataController : ControllerBase
    {
        // GET: data/get
        [HttpGet]
        public IActionResult Get(string id)
        {
            return new ObjectResult("ok.");
        }

    }
}