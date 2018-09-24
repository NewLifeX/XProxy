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
        private void ShowAll()
        {
            var ps = ProxyHelper.GetAll();
            XTrace.WriteLine("共有代理提供者[{0}]：", ps.Length);
            foreach (var item in ps)
            {
                XTrace.WriteLine("{0}\t{1}\t{2}", item.Name, item.GetDisplayName(), item.FullName);
            }
        }
        #endregion
    }
}