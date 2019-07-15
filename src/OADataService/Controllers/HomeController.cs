using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OADataService.Controllers
{
    public class HomeController : Controller
    {
        //public IActionResult Index() { return View(); }
        public IActionResult Index()
        {
            ContentResult cr = new ContentResult() { ContentType = "text/html" };
            string message = "OK! ";// + CassettesConfiguration.CassDirPath("iiss://SypCassete@iis.nsk.su");
            cr.Content = 
@"<html><head><meta charset='utf-8'/></head>
<body>
<h1>Сервис работает!</h1>
<div>" + message + @"</div>
<pre>
" + CassettesConfiguration.look + @"
</pre>
</body></html>";
            return cr;
        }

    }
}