using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewLife.Agent;
using NewLife.Log;
using NewLife.Net.Proxy;

namespace XProxy
{
    class Program
    {
        static void Main(String[] args) => MyService.ServiceMain();
    }

    class MyService : AgentServiceBase<MyService>
    {
        public MyService()
        {
            ServiceName = "XProxy";
            DisplayName = "XProxy代理服务器";

            Task.Run(() => ShowAll());
        }

        private readonly IDictionary<String, ProxyBase> _ps = new Dictionary<String, ProxyBase>();

        protected override void StartWork(String reason)
        {
            base.StartWork(reason);
        }

        protected override void StopWork(String reason)
        {
            base.StopWork(reason);
        }

        #region 辅助
        private async Task ShowAll()
        {
            await Task.Delay(500);

            //XTrace.WriteLine("");
            Console.WriteLine();

            var ps = ProxyHelper.GetAll();
            XTrace.WriteLine("共有代理提供者[{0}]：", ps.Length);
            foreach (var item in ps)
            {
                XTrace.WriteLine("{0}\t{1}\t{2}", item.Name, item.GetDisplayName(), item.FullName);
            }

            Console.WriteLine();
            var set = Setting.Current;
            var xs = set.Items ?? new ProxyItem[0];
            XTrace.WriteLine("共有代理配置[{0}]：", xs.Length);
            foreach (var item in xs)
            {
                XTrace.WriteLine("{0}({1})\t{2}\t{3}=>{4}\t{5}", item.Name, item.Provider, item.Enable, item.Local, item.Remote, item.Value);
            }
        }
        #endregion
    }
}