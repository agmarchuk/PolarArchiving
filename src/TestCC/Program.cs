using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OAData;

namespace TestCC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TestCC");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            XElement xconfig;
            XElement pack = null;

            string fname = "C:/home/data/content1.fogx";
            // Загрузка фог-файла контентом
            bool toloadfog = false;
            if (toloadfog)
            {
                string content1 =
    @"<?xml version='1.0' encoding='utf-8'?>
<rdf:RDF xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' owner='tester' prefix='r' counter='1001'>
  <person rdf:about='p9'>
    <name xml:lang='ru'>Пупкин9</name>
    <age>33</age>
  </person>
</rdf:RDF>";
                if (File.Exists(fname)) File.Delete(fname);
                XElement xcontent = XElement.Parse(content1);

                pack = XElement.Parse(
    @"<?xml version='1.0' encoding='utf-8'?>
<rdf:RDF xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
  <person rdf:about='p10' owner='tester'>
    <name xml:lang='ru'>Пупкин10</name>
    <age>34</age>
  </person>
</rdf:RDF>"
    );

                XElement item10 = pack.Elements().First();
                //xcontent.Add();
                xcontent.Save(fname);
            }

            // Загрузка конфигуратора фога и базы данных
            xconfig = XElement.Parse(
@"<?xml version='1.0' encoding='utf-8'?>
<config>
  <database connectionstring='trs:C:\Home\data\Databases\OADataService\' />
  <LoadFog write='yes'>C:/home/data/content1.fogx</LoadFog>
</config>");
            OADB cc2 = new OADB(xconfig);
            var sequ = OADB.SearchByName("пупкин");
            foreach (var lo in sequ)
            {
                Console.WriteLine(lo);
            }

            pack = XElement.Parse(
@"<?xml version='1.0' encoding='utf-8'?>
<rdf:RDF xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
  <person rdf:about='p9' owner='tester'>
    <name xml:lang='ru'>Пупкин 09 Добавленный</name>
    <age>35</age>
  </person>
</rdf:RDF>"
);

            OADB.PutItem(pack.Elements().First());

            string delete9 = "<delete rdf:about='p9' owner='tester' xmlns='http://fogid.net/o/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' />";
            XElement xdelete9 = XElement.Parse(delete9);

            var rec1 = OADB.GetItemByIdBasic("p9", true);
            Console.WriteLine(rec1.ToString());

            Console.WriteLine("===");
            var query2 = OADB.PutItem(xdelete9);
            Console.WriteLine("==" + query2.ToString());

            var sequ3 = OADB.GetAll();
            foreach (var lo in sequ3)
            {
                Console.WriteLine(lo);
            }

            return;

            //var query = DB.PutItem(item10);
            //Console.WriteLine(query.ToString());



            var sequ2 = OADB.GetAll();
            foreach (var lo in sequ2)
            {
                Console.WriteLine(lo);
            }


            cc2.Close();
        }
    }
}

