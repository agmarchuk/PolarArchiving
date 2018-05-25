using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Polar.Cassettes.DocumentStorage;

namespace CassetteData
{
    public class CassetteIntegration
    {
        private DStorage storage = null;
        private DbAdapter _engine;
        private DbAdapter engine { get { return _engine; } }
        public CassetteIntegration(XElement xconfig, bool networking)
        {
            storage = new DStorage();
            storage.Init(xconfig);
            _engine = new XmlDbAdapter();
            storage.InitAdapter(_engine);
            storage.LoadFromCassettesExpress();

        }
    }
}
