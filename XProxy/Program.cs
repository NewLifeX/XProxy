using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NewLife;
using NewLife.Agent;
using NewLife.Log;
using NewLife.Net;
using NewLife.Net.Proxy;
using NewLife.Reflection;
using NewLife.Serialization;
using NewLife.Threading;
using Stardust;
using XProxy.Proxy;

namespace XProxy
{
    class Program
    {
        // 配置读取依赖 XmlConfig 反射访问属性，AOT 裁剪时显式保留
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Setting))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ProxyItem))]
        static void Main(String[] args) => new MyService().Main(args);
    }

    class MyService : ServiceBase
    {
        public MyService()
        {
            ServiceName = "XProxy";

            // 提前加载配置并应用界面语言，保证后续显示文本与日志使用配置语言
            _ = Setting.Current;
            DisplayName = L.T("XProxy Proxy Server");
            Description = L.T("Proxy server host for NAT reverse proxy and HTTP forward/reverse proxy.");

            Task.Run(() => ShowAll());
        }

        private TimerX _timer;
        private readonly IDictionary<String, ProxyBase> _ps = new Dictionary<String, ProxyBase>();
        private void CheckProxy(Object state)
        {
            var set = Setting.Current;
            foreach (var item in set.Items)
            {
                if (item.Enable)
                {
                    if (!_ps.ContainsKey(item.Name) && !item.Provider.IsNullOrEmpty())
                    {
                        try
                        {
                            // 创建代理实例
                            var proxy = CreateProxy(item, set.Debug);
                            if (proxy != null) _ps.Add(item.Name, proxy);
                        }
                        catch (Exception ex)
                        {
                            XTrace.WriteException(ex);

                            item.Enable = false;
                        }
                    }
                }
                else
                {
                    // 停止配置为停用的服务
                    if (_ps.TryGetValue(item.Name, out var pi))
                    {
                        _ps.Remove(item.Name);

                        using var span = _tracer?.NewSpan("StopProxy", pi.Name);

                        pi.Stop(L.T("Disabled by configuration"));
                    }
                }
            }
        }

        private ITracer _tracer;
        public override void StartWork(String reason)
        {
            // 星尘链路追踪依赖动态程序集反射，NativeAOT 下关闭，不影响代理转发
            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                var star = new StarFactory(null, null, null);
                if (!star.Server.IsNullOrEmpty()) _tracer = star.Tracer;
            }
            else
                XTrace.WriteLine("NativeAOT is running without Stardust tracing.");

            //CheckProxy();
            // 检查运行时新增代理配置
            _timer = new TimerX(CheckProxy, null, 100, 10_000) { Async = true };

            base.StartWork(reason);
        }

        public override void StopWork(String reason)
        {
            _timer.TryDispose();
            _timer = null;

            foreach (var item in _ps)
            {
                item.Value.Stop(reason);
            }
            _ps.TryDispose();

            base.StopWork(reason);
        }

        #region 辅助
        private void ShowAll()
        {
            //await Task.Delay(500);
            Thread.Sleep(500);

            //XTrace.WriteLine("");
            Console.WriteLine();

            var ps = ProxyHelper.GetAll();
            XTrace.WriteLine("Proxy providers: {0}", ps.Length);
            foreach (var item in ps)
            {
                var pxy = ProxyHelper.Create(item);
                if (pxy == null) continue;

                XTrace.WriteLine("{0}\t{1}\t{2}", pxy.Name, item.GetDisplayName(), item.FullName);
            }

            Console.WriteLine();
            var set = Setting.Current;
            var xs = set.Items ?? new ProxyItem[0];
            XTrace.WriteLine("Proxy configurations: {0}", xs.Length);
            foreach (var item in xs)
            {
                XTrace.WriteLine("{0}\t{1}\t{2}\t{3}=>{4}\t{5}", item.Name, item.Provider, item.Enable, item.Local, item.Remote, item.Config);
            }
        }

        private ProxyBase CreateProxy(ProxyItem item, Boolean debug)
        {
            var xs = ProxyHelper.GetAll();
            var type = xs.FirstOrDefault(e => item.Provider.EqualIgnoreCase(e.Name, e.FullName, e.Name.TrimSuffix("Proxy")));
            if (type == null) return null;

            var proxy = ProxyHelper.Create(type);
            if (proxy == null) return null;

            using var span = _tracer?.NewSpan("CreateProxy", item);

            proxy.Name = item.Name;
            proxy.Tracer = _tracer;

            XTrace.WriteLine("Creating proxy {0}", item.ToJson());

            proxy.Init(item.Config);

            // 配置本地、远程参数。高级参数直接修改这里，解析item.Value
            proxy.Local = new NetUri(item.Local);
            if (proxy is NATProxy nat && !item.Remote.IsNullOrEmpty()) nat.RemoteServer = new NetUri(item.Remote);

            // 配置日志
            proxy.Log = XTrace.Log;
            if (debug)
            {
                proxy.SessionLog = XTrace.Log;
                proxy.Debug = debug;
            }

            if (XTrace.Log.Level <= LogLevel.Debug)
            {
                proxy.SocketLog = XTrace.Log;
                proxy.LogSend = true;
                proxy.LogReceive = true;
            }

            // 启动服务
            proxy.Start();

            return proxy;
        }
        #endregion
    }
}