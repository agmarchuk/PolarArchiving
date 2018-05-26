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
        public CassetteIntegration(XElement xconfig, bool networking)
        {
            storage = new DStorage();
            storage.Init(xconfig);
            _adapter = new XmlDbAdapter();
            storage.InitAdapter(_adapter);
            storage.LoadFromCassettesExpress();
        }
        // ============== API ==============
        public override void Init(string connectionstring)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<XElement> SearchByName(string searchstring)
        {
            return _adapter.SearchByName(searchstring);
        }

        public override XElement GetItemByIdBasic(string id, bool addinverse)
        {
            return _adapter.GetItemByIdBasic(id, addinverse);
        }

        public override XElement GetItemById(string id, XElement format)
        {
            return _adapter.GetItemById(id, format);

        }

        public override XElement GetItemByIdSpecial(string id)
        {
            return _adapter.GetItemByIdSpecial(id);
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

        public override XElement Delete(string id)
        {
            throw new NotImplementedException();
        }

        public override XElement Add(XElement record)
        {
            throw new NotImplementedException();
        }

        public override XElement AddUpdate(XElement record)
        {
            throw new NotImplementedException();
        }

        public override void LoadXFlowUsingRiTable(IEnumerable<XElement> xflow)
        {
            throw new NotImplementedException();
        }
    }
}
