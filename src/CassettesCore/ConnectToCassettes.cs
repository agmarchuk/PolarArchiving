using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Polar.Cassettes
{
    public delegate void LogLine(string message);
    public class CassettesConnection
    {
        public Dictionary<string, Cassettes.CassetteInfo> cassettesInfo = new Dictionary<string, Cassettes.CassetteInfo>();
        public Dictionary<string, Cassettes.RDFDocumentInfo> docsInfo = new Dictionary<string, Cassettes.RDFDocumentInfo>();
        public void ConnectToCassettes(IEnumerable<XElement> LoadCassette_elements, LogLine protocol)
        {
            cassettesInfo = new Dictionary<string, Cassettes.CassetteInfo>();
            docsInfo = new Dictionary<string, Cassettes.RDFDocumentInfo>();

            foreach (XElement lc in LoadCassette_elements)
            {
                bool loaddata = false;
                if (lc.Attribute("write")?.Value == "write") loaddata = true;
                string cassettePath = lc.Value;

                Cassettes.CassetteInfo ci = null;
                try
                {
                    ci = Cassettes.Cassette.LoadCassette(cassettePath, loaddata);
                }
                catch (Exception ex)
                {
                    protocol("Ошибка при загрузке кассеты [" + cassettePath + "]: " + ex.Message);
                }
                if (ci == null || cassettesInfo.ContainsKey(ci.fullName.ToSpecialCase())) continue;
                cassettesInfo.Add(ci.fullName.ToSpecialCase(), ci);
                if (ci.docsInfo != null) foreach (var docInfo in ci.docsInfo)
                    {
                        docsInfo.Add(docInfo.dbId, docInfo);
                        if (loaddata)
                        {
                            try
                            {
                                docInfo.isEditable = (lc.Attribute("write") != null && docInfo.GetRoot().Attribute("counter") != null);
                                //sDataModel.LoadRDF(docInfo.Root);
                                //if (!docInfo.isEditable) docInfo.root = null; //Иногда это действие нужно закомментаривать...
                            }
                            catch (Exception ex)
                            {
                                protocol("error in document " + docInfo.uri + "\n" + ex.Message);
                            }
                        }
                    }
            }
        }
        public IEnumerable<Cassettes.RDFDocumentInfo> GetFogFiles()
        {
            foreach (var cpair in cassettesInfo)
            {
                var ci = cpair.Value;
                var toload = ci.loaddata;
                Cassettes.RDFDocumentInfo di0 = new Cassettes.RDFDocumentInfo(ci.cassette, true);
                yield return di0;
                var qu = di0.GetRoot().Elements("document").Where(doc => doc.Element("iisstore").Attribute("documenttype").Value == "application/fog");
                foreach (var docnode in qu)
                {
                    var di = new Cassettes.RDFDocumentInfo(docnode, ci.cassette.Dir.FullName, toload);
                    if (toload) di.ClearRoot();
                    yield return di;
                }
                di0.ClearRoot();
            }
        }
    }
}
