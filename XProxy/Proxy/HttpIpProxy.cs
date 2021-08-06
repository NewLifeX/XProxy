using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using NewLife.Net.Http;
using NewLife.Security;
using NewLife.Threading;

namespace NewLife.Net.Proxy
{
    /// <summary>代理服务器使用本地IP的模式</summary>
    public enum IpModes
    {
        /// <summary>任意IP</summary>
        Any,

        /// <summary>轮询</summary>
        Round,

        /// <summary>随机</summary>
        Random,

        /// <summary>同源，从进来的IP出去</summary>
        Origin,
    }

    /// <summary>支持使用多个本地IP地址的Http代理服务器</summary>
    public class HttpIpProxy : HttpProxy
    {
        #region 属性
        /// <summary>本地地址集合</summary>
        public IPAddress[] Addresses { get; set; }

        /// <summary>使用IP的模式。支持any/round/random/origin</summary>
        public IpModes IpMode { get; set; }

        private Int32 _index;
        private TimerX _timer;
        private String _lastAddresses;
        #endregion

        public override void Init(String config)
        {
            var dic = config?.SplitAsDictionary("=", ";");
            if (dic != null && dic.Count > 0)
            {
                if (Enum.TryParse<IpModes>(dic["IpMode"], true, out var mode)) IpMode = mode;
            }

            base.Init(config);
        }

        protected override void OnStart()
        {
            WriteLog("IpMode: {0}", IpMode);

            _timer = new TimerX(DoGetIps, null, 0, 15_000) { Async = true };

            base.OnStart();
        }

        protected override void OnStop()
        {
            _timer.TryDispose();

            base.OnStop();
        }

        private void DoGetIps(Object state)
        {
            //var addrs = NetHelper.GetIPs().ToArray();
            var addrs = new List<IPAddress>();
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus != OperationalStatus.Up) continue;
                if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var ipp = item.GetIPProperties();
                if (ipp != null && ipp.UnicastAddresses.Count > 0)
                {
                    var gw = ipp.GatewayAddresses.Count;
                    if (gw == 0) continue;

                    foreach (var elm in ipp.UnicastAddresses)
                    {
                        try
                        {
                            if (elm.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred) continue;
                        }
                        catch { }

                        if (elm.Address.IsIPv4()) addrs.Add(elm.Address);
                    }
                }
            }

            var str = addrs.Join();

            if (!_lastAddresses.IsNullOrEmpty() && _lastAddresses == str) return;
            _lastAddresses = str;

            WriteLog("可用本机地址：{0}", str);

            Addresses = addrs.ToArray();
        }

        /// <summary>创建会话</summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected override INetSession CreateSession(ISocketSession session) => new IpSession();

        private class IpSession : Session
        {
            #region 方法
            protected override ISocketClient CreateRemote(ReceivedEventArgs e)
            {
                var client = base.CreateRemote(e);

                // 绑定本地IP地址
                var proxy = Host as HttpIpProxy;
                var addrs = proxy.Addresses;
                if (addrs != null && addrs.Length > 0)
                {
                    IPAddress addr = null;

                    switch (proxy.IpMode)
                    {
                        case IpModes.Any:
                            break;
                        case IpModes.Round:
                            {
                                var n = Interlocked.Increment(ref proxy._index) - 1;
                                addr = addrs[n % addrs.Length];
                            }
                            break;
                        case IpModes.Random:
                            {
                                var n = Rand.Next(addrs.Length);
                                addr = addrs[n % addrs.Length];
                            }
                            break;
                        case IpModes.Origin:
                            {
                                addr = Remote.Address;
                                if (addr.IsAny() || addr == IPAddress.Loopback || addr == IPAddress.IPv6Loopback) addr = null;
                            }
                            break;
                        default:
                            break;
                    }

                    if (addr != null && !addr.IsAny() && addrs.Any(e => e.Equals(addr)))
                    {
                        client.Local.Address = addr;
                        WriteLog("CreateRemote IpMode={0}, LocalIp={1}", proxy.IpMode, addr);
                    }
                }

                return client;
            }

            /// <summary>收到客户端发来的数据。子类可通过重载该方法来修改数据</summary>
            /// <param name="e"></param>
            /// <returns>修改后的数据</returns>
            protected override void OnReceiveRemote(ReceivedEventArgs e)
            {
                var response = new HttpResponse();
                if (response.Parse(e.Packet))
                {
                    // 响应头增加所使用的本地IP地址，让客户端知道
                    response.Headers["Proxy-Local-Ip"] = RemoteServer.Local.Address + "";
                    e.Packet = response.Build();
                }

                base.OnReceiveRemote(e);
            }
            #endregion
        }
    }
}