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
        public CassetteIntegration(XElement config, bool networking)
        {

        }
    }
}
