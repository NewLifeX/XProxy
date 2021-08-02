﻿using System;
using System.Net;
using System.Net.Http;
using NewLife.Net.Http;

namespace NewLife.Net.Proxy
{
    /// <summary>Http代理。可用于代理各种Http通讯请求。</summary>
    /// <remarks>Http代理请求与普通请求唯一的不同就是Uri，Http代理请求收到的是可能包括主机名的完整Uri</remarks>
    public class HttpProxy : ProxyBase
    {
        #region 构造
        /// <summary>实例化</summary>
        public HttpProxy()
        {
            Port = 8080;
            ProtocolType = NetType.Tcp;
        }
        #endregion

        #region 会话
        /// <summary>创建会话</summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected override INetSession CreateSession(ISocketSession session) => new Session();

        /// <summary>Http反向代理会话</summary>
        public class Session : ProxySession
        {
            /// <summary>已完成处理，正在转发数据的请求头</summary>
            public HttpHeader Request { get; set; }

            /// <summary>收到客户端发来的数据。子类可通过重载该方法来修改数据</summary>
            /// <remarks>
            /// 如果数据包包括头部和主体，可以分开处理。
            /// 最麻烦的就是数据包不是一个完整的头部，还落了一部分在后面的包上。
            /// </remarks>
            /// <param name="e"></param>
            protected override void OnReceive(ReceivedEventArgs e)
            {
                if (e.Packet.Total == 0 || !HttpBase.FastValidHeader(e.Packet))
                {
                    base.OnReceive(e);
                    return;
                }

                var request = new HttpRequest();
                if (request.Parse(e.Packet))
                {
                    WriteLog("{0} {1}", request.Method, request.Url);

                    if (!OnRequest(request, e)) return;

                    e.Packet = request.Build();
                }

                base.OnReceive(e);
            }

            /// <summary>收到请求时</summary>
            /// <param name="request"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            protected virtual Boolean OnRequest(HttpRequest request, ReceivedEventArgs e)
            {
                // 特殊处理CONNECT
                if (request.Method.EqualIgnoreCase("CONNECT")) return ProcessConnect(request, e);

                var remote = RemoteServer;
                var ruri = RemoteServerUri;
                if (request.Url.IsAbsoluteUri)
                {
                    var uri = request.Url;
                    var host = uri.Host + ":" + uri.Port;

                    // 可能不含Host
                    if (request.Host.IsNullOrEmpty()) request.Host = host;

                    // 如果地址或端口改变，则重新连接服务器
                    if (remote != null && (uri.Host != ruri.Host || uri.Port != ruri.Port))
                    {
                        remote.Dispose();
                        RemoteServer = null;
                    }

                    ruri.Host = uri.Host;
                    ruri.Port = uri.Port;
                    request.Url = new Uri(uri.PathAndQuery, UriKind.Relative);
                }
                else if (!request.Host.IsNullOrEmpty())
                {
                    ruri.Host = request.Host;
                    ruri.Port = 80;
                }
                else
                    throw new XException("无法处理的请求！{0}", request);

                // 处理KeepAlive
                if (request.Headers.TryGetValue("Proxy-Connection", out var str))
                {
                    request.KeepAlive = str.EqualIgnoreCase("keep-alive");
                    request.Headers.Remove("Proxy-Connection");
                }

                return true;
            }

            private Boolean ProcessConnect(HttpRequest request, ReceivedEventArgs e)
            {
                var pxy = Host as HttpProxy;

                var uri = RemoteServerUri;
                var ep = NetHelper.ParseEndPoint(request.Url.ToString(), 80);
                uri.EndPoint = ep;

                // 不要连自己，避免死循环
                if (ep.Port == pxy.Server.Port &&
                    (ep.Address == IPAddress.Loopback || ep.Address == IPAddress.IPv6Loopback))
                {
                    WriteLog("不要连自己，避免死循环");
                    Dispose();
                    return false;
                }

                var rs = new HttpResponse { Version = request.Version };
                try
                {
                    // 连接远程服务器，启动数据交换
                    if (RemoteServer == null) ConnectRemote(e);

                    rs.StatusCode = HttpStatusCode.OK;
                    rs.StatusDescription = "OK";
                }
#if NET5_0
                catch (HttpRequestException ex)
                {
                    rs.StatusCode = ex.StatusCode ?? HttpStatusCode.BadRequest;
                    rs.StatusDescription = ex.Message;
                }
#endif
                catch (Exception ex)
                {
                    rs.StatusCode = HttpStatusCode.BadGateway;
                    rs.StatusDescription = ex.Message;
                }

                // 告诉客户端，已经连上了服务端，或者没有连上，这里不需要向服务端发送任何数据
                Send(rs.Build());

                return false;
            }

            /// <summary>收到客户端发来的数据。子类可通过重载该方法来修改数据</summary>
            /// <param name="e"></param>
            /// <returns>修改后的数据</returns>
            protected override void OnReceiveRemote(ReceivedEventArgs e) => base.OnReceiveRemote(e);

            /// <summary>远程连接断开时触发。默认销毁整个会话，子类可根据业务情况决定客户端与代理的链接是否重用。</summary>
            /// <param name="session"></param>
            protected override void OnRemoteDispose(ISocketClient session)
            {
                // 如果客户端不要求保持连接，就销毁吧
                if (Request != null && !Request.KeepAlive) base.OnRemoteDispose(session);
            }
        }
        #endregion
    }
}