using System;
using System.Net;
using System.Net.Http;
using NewLife;
using NewLife.Log;

namespace Test
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            XTrace.UseConsole();

            try
            {
                var proxy = new MyProxy("http://127.0.0.1:8080", null, null);
                var handler = new HttpClientHandler { Proxy = proxy };
                var client = new HttpClient(handler);

                var html = client.GetStringAsync("http://star.newlifex.com/cube/info").Result;
                XTrace.WriteLine(html);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }

    public class MyProxy : IWebProxy
    {
        public Uri ProxyUri { get; set; }

        public ICredentials Credentials { get; set; }

        public MyProxy(String proxyUri, String username, String password)
        {
            ProxyUri = new Uri(proxyUri);

            if (!username.IsNullOrEmpty()) Credentials = new NetworkCredential(username, password);
        }

        public Uri GetProxy(Uri destination) => ProxyUri;

        public Boolean IsBypassed(Uri host) => false;
    }
}