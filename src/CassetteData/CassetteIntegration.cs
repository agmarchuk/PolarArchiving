using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Polar.Cassettes.DocumentStorage;

namespace CassetteData
{
    public class CassetteIntegration : DbAdapter
    {
        public DStorage storage = null;
        private DbAdapter _adapter;
        private DbAdapter Adapter { get { return _adapter; } }
        private CassetteDataRequester requester;
        public CassetteIntegration(XElement xconfig, bool networking)
        {
            storage = new DStorage();
            storage.Init(xconfig);
            _adapter = new XmlDbAdapter();
            storage.InitAdapter(_adapter);
            storage.LoadFromCassettesExpress();
            if (networking)
            {
                try
                {
                    requester = new CassetteDataRequester();
                    var res = requester.Ping();
                    if (res != "Pong") requester = null;
                }
                catch (Exception)
                {
                    requester = null;
                }
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
