using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlazorApp1.Data
{
    public class ViewerService
    {
        public Task<XElement> SearchAsync(string searchstring)
        {
            var q = OAData.OADB.SearchByName(searchstring);
            return Task.FromResult(new XElement("xresult", q));
        }
        public Task<XElement> GetItemAsync(string id)
        {
            var q = OAData.OADB.GetItemByIdBasic(id, false);
            return Task.FromResult(q);
        }
    }
}
