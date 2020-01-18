using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlazorApp2.Data
{
    public class ViewerService
    {
        public Task<XElement> GetItemAsync(string searchstring)
        {
            var q = OAData.OADB.SearchByName(searchstring);
            return Task.FromResult(new XElement("xresult", q));
        }
    }
}
