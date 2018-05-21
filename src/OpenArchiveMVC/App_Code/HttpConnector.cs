using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;

namespace OpenArchiveMVC.App_Code
{
    public class HttpConnector : Polar.Cassettes.DocumentStorage.DbAdapter
    {
        public override IEnumerable<XElement> SearchByName(string searchstring)
        {
            WebRequest request = WebRequest.Create("http://localhost:5005/?c=SearchByName&name=марчук");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            XElement result = XElement.Load(dataStream);

            return result.Elements();
        }

        public override XElement Add(XElement record)
        {
            throw new NotImplementedException();
        }

        public override XElement AddUpdate(XElement record)
        {
            throw new NotImplementedException();
        }

        public override XElement Delete(string id)
        {
            throw new NotImplementedException();
        }

        public override void FinishFillDb(Action<string> turlog)
        {
            throw new NotImplementedException();
        }

        public override XElement GetItemById(string id, XElement format)
        {
            throw new NotImplementedException();
        }

        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            throw new NotImplementedException();
        }

        public override XElement GetItemByIdSpecial(string id)
        {
            throw new NotImplementedException();
        }

        public override void Init(string connectionstring)
        {
            //throw new NotImplementedException();
        }

        public override void LoadFromCassettesExpress(IEnumerable<string> fogfilearr, Action<string> turlog, Action<string> convertlog)
        {
            throw new NotImplementedException();
        }

        public override void LoadXFlowUsingRiTable(IEnumerable<XElement> xflow)
        {
            throw new NotImplementedException();
        }

        public override void Save(string filename)
        {
            throw new NotImplementedException();
        }


        public override void StartFillDb(Action<string> turlog)
        {
            throw new NotImplementedException();
        }
    }
}
