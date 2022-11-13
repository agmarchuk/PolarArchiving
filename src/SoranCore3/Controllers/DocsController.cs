using Microsoft.AspNetCore.Mvc;

namespace SoranCore3.Controllers
{
    public class DocsController : Controller
    {
        private readonly OAData.IFactographDataService db;
        public DocsController(OAData.IFactographDataService db) { this.db = db; }

        [HttpGet("docs/GetPhoto")]
        public IActionResult GetImage(string u, string s)
        {
            string path = db.GetFilePath(u, s);
            if (!System.IO.File.Exists(path + ".jpg"))
            {
                s = s == "medium" ? "normal" : "medium";
                path = db.GetFilePath(u, s);
            }
            return PhysicalFile(path + ".jpg", "image/jpg");
        }

        [HttpGet("docs/GetVideo")]
        public IActionResult GetVideo(string u)
        {
            string path = db.GetFilePath(u, "medium");
            if (path == null) return NotFound();

            string dir_path = path.Substring(0, path.Length - 5);
            string file_num = path.Substring(path.Length - 4);
            System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo(dir_path);
            System.IO.FileInfo[] qu = dinfo.GetFiles(file_num + ".*");
            if (qu.Length == 0)
            {
                string beg = path.Substring(0, path.Length - 26);
                string end = path.Substring(path.Length - 10);
                path = beg + "originals" + end;

                dir_path = path.Substring(0, path.Length - 5);
                file_num = path.Substring(path.Length - 4);
                dinfo = new System.IO.DirectoryInfo(dir_path);
                qu = dinfo.GetFiles(file_num + ".*");
                if (qu.Length == 0) return NotFound();
            }
            int pos = qu[0].Name.LastIndexOf('.');
            if (pos == -1) return NotFound();
            string ext = qu[0].Name.Substring(pos + 1);
            return PhysicalFile(path + "." + ext, "video/" + ext);
        }
        [HttpGet("docs/GetPdf")]
        public IActionResult GetPdf(string u)
        {
            string path = db.GetFilePath(u, null);
            if (path == null) return NotFound();
            var q = path.Replace("documents/normal", "originals");
            return PhysicalFile(q + ".pdf", "application/pdf");
        }

    }
}
