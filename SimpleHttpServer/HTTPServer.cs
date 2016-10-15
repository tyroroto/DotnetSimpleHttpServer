// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// Kawin Sirikhanarat 15/10/2016

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace SimpleHttpServer
{

    public class HttpServer
    {
        private readonly string[] _indexFiles = {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };

        private static readonly IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
         };

        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;
        public List<UrlRequest> PostUrls;
        public List<UrlRequest> GetUrls;
        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public HttpServer(string path, int port, List<UrlRequest> listGetUrl, List<UrlRequest> listPostUrl)
        {
            PostUrls = listPostUrl;
            GetUrls = listGetUrl;
            foreach (var p in PostUrls)
            {
                Console.WriteLine("Create Post url "+ p.path);
            }
            foreach (var g in GetUrls)
            {
                Console.WriteLine("Create Get url " + g.path);
            }
            Initialize(path, port);
        }


        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        public HttpServer(string path)
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(path, port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            Console.WriteLine("Start Listen at port " + _port);
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server - HttpListener", ex.Message);
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            var c = context.Request.ContentType;
            filename = filename.Substring(1);
            Console.WriteLine("Receive request URL:" + filename +" type:"+context.Request.HttpMethod);
            switch (context.Request.HttpMethod)
            {
                case "POST":
                    PostRequest(context, filename);
                    break;
                case "GET":
                    GetRequest(context, filename);
                    break;
            }
        }

        public void PostRequest(HttpListenerContext context, string url)
        {
            string postRes = "";
            foreach (var req in PostUrls)
            {
                if (url == req.path)
                {
                    postRes = req.StartRequest(context.Request);
                    break;
                }
            }
            byte[] b = Encoding.UTF8.GetBytes(postRes);
            //context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.KeepAlive = false;
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Close();
        }

        private void GetRequest(HttpListenerContext context, string filename)
        {
            try
            {
                Stream input;
                byte[] buffer = new byte[1024 * 64];
                int nbytes;
                string responseText = "";
                foreach (var req in GetUrls)
                {
                    if (filename == req.path)
                    {
                        responseText = req.StartRequest(context.Request);
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(responseText))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(responseText);

                    input = new MemoryStream(byteArray);

                    //Adding permanent http response headers
                    //context.Response.ContentType = "";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));

                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    }
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                    context.Response.OutputStream.Close();
                    return;
                }


                // if url is empty will redirect to index file
                if (string.IsNullOrEmpty(filename))
                {
                    foreach (string indexFile in _indexFiles)
                    {
                        if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                        {
                            filename = indexFile;
                            break;
                        }
                    }
                }


                filename = Path.Combine(_rootDirectory, filename);
                // Get File
                if (File.Exists(filename))
                {
                    try
                    {
                        input = new FileStream(filename, FileMode.Open);

                        //Adding permanent http response headers
                        string mime;
                        context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename),
                            out mime)
                            ? mime
                            : "application/octet-stream";
                        context.Response.ContentLength64 = input.Length;
                        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                        context.Response.AddHeader("Last-Modified",
                            System.IO.File.GetLastWriteTime(filename).ToString("r"));

                        while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            context.Response.OutputStream.Write(buffer, 0, nbytes);
                        }
                        input.Close();
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                        context.Response.OutputStream.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ServerInternalServerError 501" + ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Server InternalServerError 502" + e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.Response.OutputStream.Close();
        }

        private void Initialize(string path, int port)
        {
            this._rootDirectory = path;
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }


    }

}