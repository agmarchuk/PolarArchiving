using System;
using System.Xml.Linq;
using OAData;

namespace TestCC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TestCC");
            XElement xconfig = XElement.Parse(
@"<?xml version='1.0' encoding='utf-8'?>
<config>
  <database connectionstring='trs:C:\Home\data\Databases\OADataService\' />
  <LoadCassette write='yes' >C:\Home\FactographProjects\CassTest20190726</LoadCassette >
</config>
"
                );
            OAData.OAData cc = new OAData.OAData(xconfig);
            cc.Test();
        }
    }
}
