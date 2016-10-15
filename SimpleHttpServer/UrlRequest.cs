using System;
using System.Net;

namespace SimpleHttpServer
{
    public delegate string OnRequestUrl(HttpListenerRequest inputs);
    public class UrlRequest
    {
        public string path = "";
        public OnRequestUrl onRequestUrl;

        public UrlRequest(string p, OnRequestUrl callback)
        {
            path = p;
            onRequestUrl = callback;
        }

        public UrlRequest(string p)
        {
            path = p;
        }

        public string StartRequest(HttpListenerRequest req)
        {
            var res = onRequestUrl?.Invoke(req);
            return res;
        }
    }
}
