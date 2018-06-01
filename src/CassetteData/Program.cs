using System;
using System.Xml.Linq;
using System.Net;
using System.Threading.Tasks;


namespace CassetteData
{
    class Program
    {
        static void Main1(string[] args)
        {
            var task = Listener();
            Task.Run(() => task);
            Console.WriteLine("exit?");
            Console.ReadKey();

        }
        static async Task Listener()
        { 
            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            listener.Prefixes.Add("http://localhost:5000");

            listener.Start();
            bool tocontinue = true;

            while (tocontinue)
            {
                Console.WriteLine("Listening...");
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = await listener.GetContextAsync();
                Console.WriteLine(context.Request.Url);
                HttpListenerRequest request = context.Request;

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                string responseString = "ok";
                response.ContentEncoding = System.Text.Encoding.UTF8;
                response.ContentType = "text/plain";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }

            listener.Stop();

        }
        static void Main0(string[] args)
        {
            Console.WriteLine("Start CassetteData module");
            XElement xconfig = new XElement("config",
                new XElement("LoadCassette", @"D:\Home\FactographProjects\Cassette_test20180311"),
                null);
            CassetteIntegration ci = new CassetteIntegration(xconfig, false);

            var res = ci.SearchByName("марчук");
            foreach (var r in res)
            {
                Console.WriteLine(r.ToString());
            }
            Console.WriteLine();

            var res2 = ci.GetItemByIdSpecial("syp2001-p-marchuk_a");
            Console.WriteLine(res2.ToString());
            Console.WriteLine();

            var res3 = ci.GetItemByIdBasic("syp2001-p-marchuk_a", true);
            Console.WriteLine(res3.ToString());
            Console.WriteLine();

            XElement format = new XElement("record",
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/participant"),
                    new XElement("record",
                        new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-org"),
                            new XElement("record",
                                new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
                null);
            var res4 = ci.GetItemById("syp2001-p-marchuk_a", format);
            Console.WriteLine(res4.ToString());
            Console.WriteLine();

            //Console.WriteLine("exit?");
            //Console.ReadKey();
        }
    }
}
