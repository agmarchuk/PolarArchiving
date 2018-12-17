using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Polar.Cassettes.DocumentStorage;

namespace CassetteData
{
    public class CassetteIntegration : DbAdapter
    {
        public DStorage localstorage = null;
        private DbAdapter _adapter;
        private DbAdapter Adapter { get { return _adapter; } }
        private CassetteDataRequester requester;
        // Видимо временное решение
        public string DaraSrc {  get { return requester?.DataSrc; } }

        public CassetteIntegration(XElement xconfig)
        {
            localstorage = new DStorage();
            localstorage.Init(xconfig);

            string connectionstring = xconfig.Element("database")?.Attribute("connectionstring")?.Value;
            if (connectionstring == null || connectionstring.StartsWith("xml:"))
            {
                _adapter = new XmlDbAdapter();
            }
            else if (connectionstring.StartsWith("sxml:"))
            {
                _adapter = new SimpleDbAdapter();
            }

            localstorage.InitAdapter(_adapter);
            XElement net = xconfig.Element("Net");
            if (net != null)
            {
                try
                {
                    requester = new CassetteDataRequester(net.Attribute("src")?.Value);
                    var res = requester.Ping();
                    if (res != "Pong") requester = null;
                }
                catch (Exception)
                {
                    requester = null;
                }
            }
            else
            {
                localstorage.LoadFromCassettesExpress();

            }
        }
        // ============== API ==============

        public override IEnumerable<XElement> SearchByName(string searchstring)
        {
            if (requester != null) return requester.SearchByName(searchstring);
            return _adapter.SearchByName(searchstring);
        }
        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            if (requester != null) return requester.GetItemByIdBasic(id, addinverse);
            return _adapter.GetItemByIdBasic(id, addinverse);
        }
        public override XElement GetItemById(string id, XElement format)
        {
            if (requester != null) return requester.GetItemById(id, format);
            return _adapter.GetItemById(id, format);
        }
        public override XElement GetItemByIdSpecial(string id)
        {
            if (requester != null) return requester.GetItemByIdSpecial(id);
            return _adapter.GetItemByIdSpecial(id);
        }
        public override XElement Delete(string id)
        {
            if (requester != null) return requester.Delete(id);
            return _adapter.Delete(id);
        }
        public override XElement Add(XElement record)
        {
            if (requester != null) return requester.Add(record);
            return _adapter.Add(record);
        }
        public override XElement AddUpdate(XElement record)
        {
            if (requester != null) return requester.AddUpdate(record);
            return _adapter.AddUpdate(record);
        }



        public override void Init(string connectionstring)
        {
            throw new NotImplementedException();
        }
        public override void StartFillDb(Action<string> turlog)
        {
            throw new NotImplementedException();
        }

        public override void LoadFromCassettesExpress(IEnumerable<string> fogfilearr, Action<string> turlog, Action<string> convertlog)
        {
            throw new NotImplementedException();
        }

        public override void FinishFillDb(Action<string> turlog)
        {
            throw new NotImplementedException();
        }

        public override void Save(string filename)
        {
            throw new NotImplementedException();
        }


        public override void LoadXFlowUsingRiTable(IEnumerable<XElement> xflow)
        {
            throw new NotImplementedException();
        }
    }
}
