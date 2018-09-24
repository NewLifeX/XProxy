using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XProxy.Base
{
    /// <summary>
    /// 声明当 <c>客户端</c> 断开与服务器的链接时的回调方法
    /// </summary>
    /// <param name="session">要关闭其链接的 <c>客户端</c></param>
    public delegate Boolean DestroyDelegate(Session session);
    /// <summary>
    /// 写日志委托
    /// </summary>
    /// <param name="msg">日志信息</param>
    public delegate void WriteLogDelegate(String msg);

    ///<summary>声明 <c>客户端</c> 的基本方法和属性</summary>
    ///<remarks>客户端类描述客户端和服务器的链接</remarks>
    public class Session : IDisposable
    {
        #region 事件
        /// <summary>
        /// 客户端被销毁事件
        /// </summary>
        public event DestroyDelegate OnDestroy;

        /// <summary>
        /// 写日志事件
        /// </summary>
        public event WriteLogDelegate OnWriteLog;
        #endregion

        #region 属性
        /// <summary>
        /// 客户端连接
        /// </summary>
        public Connection Client { get; set; }

        private Connection _Server;
        /// <summary>
        /// 服务器连接
        /// </summary>
        public Connection Server
        {
            get
            {
                if (_Server == null)
                {
                    var tcpclient = ConnectServer(true);
                    if (tcpclient != null)
                    {
                        _Server = new Connection(tcpclient)
                        {
                            Name = "服务器",
                            Session = this
                        };
                        _Server.OnRead += new ReadCompleteDelegate(OnServerToClient);

                        if (IsAsync)
                        {
                            // 连接已经建立，必须建立一个等待服务器数据的委托
                            _Server.BeginRead();
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ExchangeServer));
                        }
                    }
                }
                return _Server;
            }
            set { _Server = value; }
        }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public Int32 ServerPort { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public IPAddress ServerAddress { get; set; }
        /// <summary>
        /// 对象唯一标识
        /// </summary>
        public String GUID { get; } = NewGuid();

        private static Int32 _NewGuid = 1;
        private static String NewGuid() => (_NewGuid++).ToString();

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; } = DateTime.Now;
        /// <summary>
        /// 超时时间。默认30秒
        /// </summary>
        public Int32 TimeOut { get; set; } = 30000;
        /// <summary>
        /// IP端口
        /// </summary>
        public String IPAndPort { get; set; } = String.Empty;
        /// <summary>
        /// 对应的监听器
        /// </summary>
        public Listener Listener { get; set; }
        /// <summary>
        /// 数据项。用于存储扩展数据
        /// </summary>
        public Dictionary<String, Object> Items { get; set; } = new Dictionary<String, Object>();

        /// <summary>
        /// 使用异步
        /// </summary>
        public Boolean IsAsync
        {
            get { return Listener.Config.IsAsync; }
            set { Listener.Config.IsAsync = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化一个客户端实例
        /// </summary>
        /// <param name="tcpclient">到客户端的连接</param>
        public Session(TcpClient tcpclient)
            : this(tcpclient, 0, IPAddress.Any)
        {
        }

        /// <summary>
        /// 初始化一个客户端实例
        /// </summary>
        /// <param name="tcpclient">到客户端的连接</param>
        /// <param name="port">远程服务器端口</param>
        /// <param name="address">远程服务器地址</param>
        public Session(TcpClient tcpclient, Int32 port, String address)
            : this(tcpclient, port, address == "0.0.0.0" ? IPAddress.Any : Dns.GetHostEntry(address).AddressList[0])
        {
        }

        /// <summary>
        /// 初始化一个客户端实例
        /// </summary>
        /// <param name="tcpclient">到客户端的连接</param>
        /// <param name="port">远程服务器端口</param>
        /// <param name="address">远程服务器地址</param>
        public Session(TcpClient tcpclient, Int32 port, IPAddress address)
        {
            Client = new Connection(tcpclient)
            {
                Session = this,
                Name = "客户端"
            };
            Client.OnRead += new ReadCompleteDelegate(OnClientToServer);
            IPAndPort = tcpclient.Client.RemoteEndPoint.ToString();

            // 在发送或接收缓冲区未满时禁用延迟，让其马上发送数据
            // Winsock的Nagle算法将降低小数据报的发送速度，而系统默认是使用Nagle算法
            // Nagle 算法使套接字缓冲最多 200 毫秒内的数据包，然后使用一个数据包发送它们，从而减少网络流量
            // 这里关闭Nagle算法，以加快网络处理速度
            tcpclient.NoDelay = true;

            ServerPort = port;
            ServerAddress = address;
        }
        #endregion

        #region 析构函数 以及 Dispose资源回收
        /// <summary>
        /// 销毁客户端占用的资源
        /// </summary>
        /// <param name="msg">消息</param>
        public void Dispose(String msg)
        {
            WriteLog(msg);
            Dispose();
        }

        private Boolean IsDisposed = false;
        ///<summary>销毁客户端占用的资源</summary>
        ///<remarks>关闭所有与客户端和服务器的链接</remarks>
        ///<seealso cref ="System.IDisposable"/>
        public void Dispose()
        {
            if (IsDisposed) return;
            lock (this)
            {
                if (IsDisposed) return;
                IsDisposed = true;
                try
                {
                    Client.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("关闭客户端连接出错！" + ex.Message);
                }
                try
                {
                    Server.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("关闭远程服务器连接出错！" + ex.Message);
                }
                if (OnDestroy != null)
                    OnDestroy(this);
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Session()
        {
            Dispose();
        }
        #endregion

        #region 数据交换过程
        #region 开始
        ///<summary>接受客户端端连接，开始代理工作</summary>
        public void Start()
        {
            if (!Listener.Plugin.OnClientStart(this))
            {
                Dispose();
                return;
            }
            //异步或同步
            if (IsAsync)
                Client.BeginRead();
            else
            {
                //现在还在主线程，必须开新线程来进行同步处理
                ThreadPool.QueueUserWorkItem(new WaitCallback(ExchangeClient));
            }
        }
        #endregion

        #region 同步交换
        /// <summary>同步交换</summary>
        protected void ExchangeClient(Object obj)
        {
            try
            {
                while (true)
                {
                    var buf = Client.Read();
                    if (buf == null || buf.Length < 1) break;
                    buf = OnClientToServer(buf);
                    if (buf == null || buf.Length < 1) break;

                    //写入服务端
                    Server.Write(buf);
                }
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose("同步交换客户端数据时出错 " + ex.Message);
            }
        }

        /// <summary>同步交换</summary>
        protected void ExchangeServer(Object obj)
        {
            try
            {
                while (true)
                {
                    var buf = Server.Read();
                    if (buf == null || buf.Length < 1) break;
                    buf = OnServerToClient(buf);
                    if (buf == null || buf.Length < 1) break;

                    //写入客户端
                    Client.Write(buf);
                }
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose("同步交换服务端数据时出错 " + ex.Message);
            }
        }
        #endregion
        #endregion

        #region 数据交换事件
        /// <summary>
        /// 客户端向服务器发数据时触发。重载时应该调用该方法，以保证能输出日志和调用插件。
        /// </summary>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        internal virtual Byte[] OnClientToServer(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;

            Data = Listener.Plugin.OnClientToServer(this, Data);

            if (Listener.Config.IsShowClientData)
                WriteLog("客户端数据（" + Data.Length + "Byte）（" + GUID + "）");

            //异步方式时，本函数处于接收事件中，需要使用异步写入，使得能尽快进入下一轮数据的接收过程之中
            if (IsAsync)
                Server.BeginWrite(Data);
            else
                Server.Write(Data);

            return Data;
        }

        /// <summary>
        /// 服务器相客户端发数据时触发。重载时应该调用该方法，以保证能输出日志和调用插件。
        /// </summary>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        internal virtual Byte[] OnServerToClient(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;

            Data = Listener.Plugin.OnServerToClient(this, Data);

            if (Listener.Config.IsShowServerData)
                WriteLog("服务端数据（" + Data.Length + "Byte）（" + GUID + "）");

            if (IsAsync)
                Client.BeginWrite(Data);
            else
                Client.Write(Data);

            return Data;
        }
        #endregion

        #region 连接服务器
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="KeepAlive">是否使用KeepAlive</param>
        /// <returns>连接</returns>
        protected TcpClient ConnectServer(Boolean KeepAlive)
        {
            TcpClient tcpclient = null;
            if (!Listener.Plugin.OnServerStart(this)) return null;
            try
            {
                tcpclient = new TcpClient();
                tcpclient.Connect(ServerAddress, ServerPort);
                // 在发送或接收缓冲区未满时禁用延迟
                tcpclient.NoDelay = true;
                // 不要保持连接。必要时，要断开连接，以免影响服务器性能。
                if (KeepAlive)
                {
                    tcpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    NetHelper.SetKeepAlive(tcpclient.Client, true, TimeOut, TimeOut);
                }
            }
            catch (Exception ex)
            {
                Dispose("连接服务器出错 " + ex.Message);
                return null;
            }
            return tcpclient;
        }
        #endregion

        #region 辅助函数
        ///<summary>输入日志</summary>
        ///<remarks>输入日志信息到UI信息框</remarks>
        ///<param name="log">要输出的日志信息</param>
        public void WriteLog(String log)
        {
            if (OnWriteLog != null)
            {
                //使用线程池线程写日志
                ThreadPool.QueueUserWorkItem(new WaitCallback(WriteLogCallBack), String.Format("[{2}]{0,6} {1}", GUID, log, Listener.Config.Name));
            }
        }
        private void WriteLogCallBack(Object msg)
        {
            if (msg == null || String.IsNullOrEmpty(msg.ToString())) return;
            OnWriteLog(msg.ToString());
        }
        #endregion
    }
}