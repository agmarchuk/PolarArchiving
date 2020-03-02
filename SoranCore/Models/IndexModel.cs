using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;


namespace SoranCore.Models
{
    public class IndexModel
    {
        private string sdirection = "person";
        public string TabDirection { get { return sdirection; } set { sdirection = value; } }
        private IEnumerable<XElement> _sresults = null;
        public IEnumerable<XElement> SearchResults { get { return _sresults; } set { _sresults = value; } }
    }
}
