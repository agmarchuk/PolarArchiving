using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using Polar.Cassettes.DocumentStorage;

namespace DBConsoleApp
{
    partial class Program
    {
        // This example requires the System and System.Net namespaces.
        public static void SimpleListenerExample(DbAdapter engine, string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            bool tocontinue = true;

            while (tocontinue)
            {
                Console.WriteLine("Listening...");
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                //foreach (var q in request.QueryString)
                //{
                //    string key = (string)q;
                //    Console.WriteLine(key + "=" + request.QueryString[key]);
                //}
                //var stream = request.InputStream;
                //var qq = new StreamReader(stream);
                //string text = qq.ReadToEnd();
                //Console.WriteLine("Content: " + text);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                string responseString = "<result></result>";
                response.ContentEncoding = System.Text.Encoding.UTF8;
                response.ContentType = "text/xml";
                string comm = request.QueryString["c"];
                if (comm == "SearchByName")
                {
                    string name = request.QueryString["name"];
                    var res = engine.SearchByName(name);
                    responseString = (new XElement("result", res.Select(r => new XElement(r)))).ToString();
                }
                else if (comm == "GetItemByIdBasic")
                {
                    string id = request.QueryString["id"];
                    string addinverse = request.QueryString["addinverse"];
                    var res = engine.GetItemByIdBasic(id, addinverse=="true"?true:false);
                    responseString = res.ToString();
                }
                else if (comm == "GetItemById")
                {
                    Console.WriteLine("GetItemById");
                    string id = request.QueryString["id"];
                    Console.WriteLine("id=" + id);
                    
                    Stream rstream = request.InputStream;
                    XElement format = XElement.Load(rstream);
                    Console.WriteLine(format.ToString());
                    var res = engine.GetItemById(id, format);
                    responseString = res.ToString();
                }


                // Construct a response.
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
    }
}
