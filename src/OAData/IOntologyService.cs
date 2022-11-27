using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace OAData
{
    public interface IOntologyService
    {
        string GetOntName(string name);
        string GetInvOntName(string name);
        string GetEnumStateLabel(string enum_type, string state_value);
        IEnumerable<XElement> GetEnumStates(string enum_type);


    }

}
