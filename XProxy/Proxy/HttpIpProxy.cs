using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using NewLife.Threading;

namespace NewLife.Net.Proxy
{
    /// <summary>支持使用多个本地IP地址的Http代理服务器</summary>
    public class HttpIpProxy : HttpProxy
    {
        #region 属性
        /// <summary>本地地址集合</summary>
        public IPAddress[] Addresses { get; set; }

        private Int32 _index;
        private TimerX _timer;
        private String _lastAddresses;
        #endregion

        protected override void OnStart()
        {
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
                    var n = Interlocked.Increment(ref proxy._index);
                    client.Local.Address = addrs[(n - 1) % addrs.Length];
                }

                return client;
            }
            #endregion
        }
    }
}