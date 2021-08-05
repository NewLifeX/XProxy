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

            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", true);

            try
            {
                var proxy = new WebProxy("10.0.0.3:8080")
                {
                    Credentials = new NetworkCredential("stone", "password"),
                    //BypassProxyOnLocal = false,
                    //UseDefaultCredentials = false,
                };
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                };
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

        public class MyProxy : IWebProxy
        {
            public Uri ProxyUri { get; set; }

            public ICredentials Credentials { get; set; }

            public MyProxy(String proxyUri)
            {
                ProxyUri = new Uri(proxyUri);
            }

            public Uri GetProxy(Uri destination) => ProxyUri;

            public Boolean IsBypassed(Uri host) => false;
        }
    }
}