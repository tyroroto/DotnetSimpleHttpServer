using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SimpleHttpServer;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var resHandle = new ResponseHandle();
            HttpServer server = new HttpServer("web", 9999, resHandle.getRequests, resHandle.postRequests);
        }
    }

    public class ResponseHandle
    {
        public List<UrlRequest> getRequests = new List<UrlRequest>();
        public List<UrlRequest> postRequests = new List<UrlRequest>();

        public ResponseHandle()
        {
            getRequests.Add(new UrlRequest("getTest", inputs => "this is get Test"));
            getRequests.Add(new UrlRequest("getTest2", GetTest2));
            postRequests.Add(new UrlRequest("postTest", PostTest));
        }

        string GetTest2(HttpListenerRequest req)
        {
            return "this is get test 2";
        }

        string PostTest(HttpListenerRequest req)
        {
            var reader = new StreamReader(req.InputStream, req.ContentEncoding);
            var text = reader.ReadToEnd();
           
            Console.WriteLine("ContentType " + req.ContentType);
            Console.WriteLine("Content " + text);

            //need to decode content to use 
            return "this is Post test";
        }


    }
}
