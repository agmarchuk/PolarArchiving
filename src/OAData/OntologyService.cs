using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OAData.Adapters;

namespace OAData
{
    public class OntologyService : IOntologyService
    {
        private string path;
        public OntologyService()
        {
            Console.WriteLine("mag: FactoraphDataService Constructing " + DateTime.Now);
            path = "wwwroot/";
            Init(path + "ontology_iis-v13.xml");
        }
        private void Init(string ontologypath)
        {
            _ontology = XElement.Load(ontologypath);
            LoadOntNamesFromOntology();
            LoadInvOntNamesFromOntology();
        }
        public string GetOntName(string name)
        {
            string res = null;
            if (OntNames.TryGetValue(name, out res)) return res;
            return name;
        }
        public string GetInvOntName(string name)
        {
            string res = null;
            if (InvOntNames.TryGetValue(name, out res)) return res;
            return name;
        }

        private Dictionary<string, string> OntNames { get; set; }
        private Dictionary<string, string> InvOntNames { get; set; }
        private XElement _ontology;
        private void LoadOntNamesFromOntology()
        {
            var ont_names = _ontology.Elements()
                .Where(el => el.Name == "Class" || el.Name == "ObjectProperty" || el.Name == "DatatypeProperty")
                .Where(el => el.Elements("label").Any())
                .Select(el => new
                {
                    type_id = el.Attribute(ONames.rdfabout).Value,
                    label = el.Elements("label").First(lab => lab.Attribute(ONames.xmllang).Value == "ru").Value
                })
                .ToDictionary(pa => pa.type_id, pa => pa.label);
            OntNames = ont_names;
        }
        private void LoadInvOntNamesFromOntology()
        {
            var i_ont_names = _ontology.Elements("ObjectProperty")
                //.Where(el => el.Name == "Class" || el.Name == "ObjectProperty" || el.Name == "DatatypeProperty")
                .Where(el => el.Elements("inverse-label").Any())
                .Select(el => new
                {
                    type_id = el.Attribute(ONames.rdfabout).Value,
                    label = el.Elements("inverse-label").First(lab => lab.Attribute(ONames.xmllang).Value == "ru").Value
                })
                .ToDictionary(pa => pa.type_id, pa => pa.label);
            InvOntNames = i_ont_names;
        }
        public string GetEnumStateLabel(string enum_type, string state_value)
        {
            var et_def = _ontology.Elements("EnumerationType")
                .FirstOrDefault(et => et.Attribute(ONames.rdfabout).Value == enum_type);
            if (et_def == null) return "";
            var et_state = et_def.Elements("state")
                .FirstOrDefault(st => st.Attribute("value").Value == state_value && st.Attribute(ONames.xmllang).Value == "ru");
            if (et_state == null) return "";
            return et_state.Value;
        }
        public IEnumerable<XElement> GetEnumStates(string enum_type)
        {
            var et_def = _ontology.Elements("EnumerationType")
                .FirstOrDefault(et => et.Attribute(ONames.rdfabout).Value == enum_type);
            if (et_def == null) return null;
            var et_states = et_def.Elements("state")
                .Where(st => st.Attribute(ONames.xmllang).Value == "ru");
            return et_states;
        }

        // ================== Реализация базового интерфейса ==================
        public string EnumValue(string specificator, string stateval, string lang)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AncestorsAndSelf(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DescendantsAndSelf(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetInversePropsByType(string tp)
        {
            throw new NotImplementedException();
        }

        public string LabelOfOnto(string id)
        {
            throw new NotImplementedException();
        }

        public string InvLabelOfOnto(string propId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DomainsOfProp(string propId)
        {
            throw new NotImplementedException();
        }
    }
}
