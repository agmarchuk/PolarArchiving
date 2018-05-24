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
            string requeststring = "http://localhost:5005/?c=SearchByName&name=" + searchstring;
            XElement result = AskByRequest(requeststring);
            return result.Elements();
        }

        private static XElement AskByRequest(string requeststring)
        {
            WebRequest request = WebRequest.Create(requeststring);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            XElement result = XElement.Load(dataStream);
            return result;
        }

        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            string requeststring = "http://localhost:5005/?c=GetItemByIdBasic&id=" + id + "&addinverse=" + addinverse.ToString();
            XElement result = AskByRequest(requeststring);
            return result;
        }
        public override XElement GetItemById(string id, XElement format)
        {
            string requeststring = "http://localhost:5005/?c=GetItemById&id=" + id;
            WebRequest request = WebRequest.Create(requeststring);
            request.Method = "POST";
            request.ContentType = "text/xml";
            string contentstring = format.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(contentstring);
            request.ContentLength = buffer.Length;
            Stream requStream = request.GetRequestStream();
            requStream.Write(buffer, 0, buffer.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            XElement result = XElement.Load(dataStream);
            return result;
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
