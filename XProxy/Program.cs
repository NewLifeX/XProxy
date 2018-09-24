using System;
using NewLife.Agent;

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
        }
    }
}