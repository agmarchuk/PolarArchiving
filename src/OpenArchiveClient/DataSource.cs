using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenArchiveClient
{
    public abstract class DataSource
    {
        public static XElement GetItemByIdBasic(string id, bool addinverse) { return OAData.OADB.GetItemByIdBasic(id, addinverse); }
        public static XElement GetItemById(string id, XElement format) { return OAData.OADB.GetItemById(id, format); }
        public static IEnumerable<XElement> SearchByName(string name) { return OAData.OADB.SearchByName(name); }

    }
}
