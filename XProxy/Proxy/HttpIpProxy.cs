﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using NewLife.Net.Http;
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
            private String[] _white_ips;
            private String[] _black_ips;

            #region 方法
            /// <summary>收到请求时</summary>
            /// <param name="request"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            protected override Boolean OnRequest(HttpRequest request, ReceivedEventArgs e)
            {
                _white_ips = request.Headers["White-Ips"]?.Split(",");
                _black_ips = request.Headers["Black-Ips"]?.Split(",");

                return base.OnRequest(request, e);
            }

            protected override ISocketClient CreateRemote(ReceivedEventArgs e)
            {
                var client = base.CreateRemote(e);

                // 绑定本地IP地址
                var proxy = Host as HttpIpProxy;
                var addrs = proxy.Addresses;
                if (addrs != null && addrs.Length > 0)
                {
                    IPAddress addr = null;

                    // 指定了白名单，从中取IP
                    if (_white_ips != null && _white_ips.Length > 0)
                    {
                        // 计算白名单中有效的IP地址
                        var ds = addrs.Where(e => _white_ips.Contains(e + "")).ToArray();
                        if (ds.Length > 0)
                        {
                            var n = Interlocked.Increment(ref proxy._index);
                            addr = ds[(n - 1) % ds.Length];
                        }
                    }

                    for (var i = 0; addr != null && i < 100; i++)
                    {
                        var n = Interlocked.Increment(ref proxy._index);
                        var addr2 = addrs[(n - 1) % addrs.Length];

                        if (_black_ips == null || !_black_ips.Contains(addr2 + ""))
                        {
                            addr = addr2;
                            break;
                        }
                    }

                    if (addr != null) client.Local.Address = addr;
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
                    response.Headers["Local-Ip"] = RemoteServer.Local.Address + "";
                    e.Packet = response.Build();
                }

                base.OnReceiveRemote(e);
            }
            #endregion
        }
    }
}