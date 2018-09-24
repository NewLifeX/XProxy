using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NewLife.Log;
using XProxy.Config;
using XProxy.Plugin;

namespace XProxy.Base
{
    /// <summary>
    /// 代理监听器类
    /// </summary>
    public class Listener : IDisposable
    {
        #region 属性
        private IPAddress _Address;
        /// <summary>
        /// 地址
        /// </summary>
        public IPAddress Address
        {
            get
            {
                if (_Address != null) return _Address;

                if (Config.Address == "0.0.0.0")
                    _Address = IPAddress.Any;
                else
                {
                    try
                    {
                        _Address = Dns.GetHostEntry(Config.Address).AddressList[0];
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        var msg = "获取[" + Config.Address + "]的IP地址时出错！";
                        WriteLog(msg);
                        XTrace.WriteLine(msg);
                        throw ex;
                    }
                }
                return _Address;
            }
        }
        /// <summary>
        /// Tcp监听器
        /// </summary>
        public TcpListener TcpServer { get; set; }

        /// <summary>
        /// 客户端数组。用来记录所有已连接客户端。
        /// </summary>
        public List<Session> Clients = new List<Session>();

        /// <summary>
        /// 写日志委托
        /// </summary>
        public event WriteLogDelegate OnWriteLog;
        /// <summary>
        /// 插件管理器
        /// </summary>
        internal PluginManager Plugin { get; set; }

        private ListenerConfig _Config;
        /// <summary>
        /// 配置
        /// </summary>
        public ListenerConfig Config
        {
            get
            {
                return _Config;
            }
            set
            {
                if (_Config != value)
                {
                    _Config = value;
                    _Address = null;
                }
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化监听器类的新实例
        /// </summary>
        /// <param name="config"></param>
        public Listener(ListenerConfig config) => Config = config;
        #endregion

        #region 开始/停止
        /// <summary>
        /// 在指定的IP和端口上开始监听
        /// </summary>
        public virtual void Start()
        {
            if (Plugin == null)
            {
                Plugin = new PluginManager
                {
                    Listener = this
                };
                if (Config.IsShow && OnWriteLog != null) Plugin.OnWriteLog += new WriteLogDelegate(WriteLog);
                Plugin.OnInit(this);
            }

            try
            {
                var plugincount = Plugin.Plugins == null ? 0 : Plugin.Plugins.Count;
                WriteLog(String.Format("开始监听 {0}:{1} [{2}] 插件：{3}个", Address.ToString(), Config.Port, Config.Name, plugincount));
                if (TcpServer != null) Stop();
                TcpServer = new TcpListener(Address, Config.Port)
                {
                    // 指定 TcpListener 是否只允许一个基础套接字来侦听特定端口。
                    // 只有监听任意IP的时候，才打开 ExclusiveAddressUse
                    ExclusiveAddressUse = (Address == IPAddress.Any)
                };
                TcpServer.Start();
                // 开始异步接受传入的连接
                TcpServer.BeginAcceptTcpClient(new AsyncCallback(OnAccept), TcpServer);
                IsDisposed = false;
                Plugin.OnListenerStart(this);
            }
            catch (Exception ex)
            {
                Dispose();
                var msg = "开始监听" + Address.ToString() + ":" + Config.Port + "时出错！" + ex.Message;
                WriteLog(msg);
                XTrace.WriteLine(msg);
                throw ex;
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public virtual void Stop()
        {
            WriteLog("停止监听" + Address.ToString() + ":" + Config.Port);
            if (Clients != null)
            {
                // Dispose时会关闭每个客户端的连接
                for (var i = Clients.Count - 1; i >= 0; i--)
                    (Clients[i] as Session).Dispose();
                Clients.Clear();
            }
            try
            {
                TcpServer.Stop();
                if (TcpServer.Server != null)
                {
                    if (TcpServer.Server.Connected)
                        TcpServer.Server.Shutdown(SocketShutdown.Both);
                    TcpServer.Server.Close();
                }
                Plugin.OnListenerStop(this);
                IsDisposed = true;
            }
            catch (Exception ex)
            {
                var msg = "停止监听" + Address.ToString() + ":" + Config.Port + "时出错！" + ex.Message;
                WriteLog(msg);
                XTrace.WriteLine(msg);
                throw ex;
            }
        }
        #endregion

        #region 新客户端到来
        ///<summary>
        /// 当客户链接等待传入时被调用。
        /// 关闭监听器时，也被调用，只不过EndAcceptTcpClient会触发异常
        /// </summary>
        ///<param name="ar">异步操作的结果</param>
        private void OnAccept(IAsyncResult ar)
        {
            TcpClient tcp = null;
            try
            {
                tcp = TcpServer.EndAcceptTcpClient(ar);
            }
            catch { return; }
            try
            {
                // 先重新开始监听，不要耽误了别的来访者访问
                TcpServer.BeginAcceptTcpClient(new AsyncCallback(OnAccept), TcpServer);
            }
            catch { Dispose(); }
            try
            {
                if (tcp != null)
                {
                    NetHelper.SetKeepAlive(tcp.Client, true, 30000, 30000);
                    var NewClient = OnAccept(tcp);
                    if (Config.IsShow && OnWriteLog != null) NewClient.OnWriteLog += new WriteLogDelegate(WriteLog);
                    NewClient.OnDestroy += new DestroyDelegate(ClientDestroy);
                    NewClient.Listener = this;
                    NewClient.WriteLog("新客户 (" + tcp.Client.RemoteEndPoint.ToString() + ")");
                    Clients.Add(NewClient);
                    NewClient.Start();
                }
            }
            catch (Exception ex)
            {
                XTrace.WriteLine("监听器接受连接时出错！ " + ex.Message);
            }
        }

        private Boolean ClientDestroy(Session session)
        {
            session.WriteLog("客户(" + session.IPAndPort + ")" + "退出");
            //WriteLog("客户" + session.GUID + "(" + session.IPAndPort + ")" + "退出");
            //GC.Collect();
            return Clients.Remove(session);
        }

        /// <summary>
        /// 新客户端连接。请通过该方法建立相应的客户端实例对象。
        /// 比如：return new HttpClient(session, ServerPort, ServerAddress);
        /// </summary>
        /// <param name="tcp">对应的TcpClient</param>
        /// <returns>客户端实例</returns>
        public virtual Session OnAccept(TcpClient tcp) => new Session(tcp, Config.ServerPort, Config.ServerAddress);
        #endregion

        #region 资源销毁
        /// <summary>
        /// 是否已经销毁
        /// </summary>
        private Boolean IsDisposed = false;

        ///<summary>销毁监听器占用的资源</summary>
        ///<remarks>停止监听，并销毁 <em>所有</em> 客户对象，一旦销毁，对象将不再使用</remarks>
        ///<seealso cref ="System.IDisposable"/>
        public void Dispose()
        {
            if (IsDisposed) return;
            lock (this)
            {
                if (IsDisposed) return;
                IsDisposed = true;
                Stop();
                //GC.Collect();
            }
        }
        ///<summary>终止监听器</summary>
        ///<remarks>调用Dispose的析构函数</remarks>
        ~Listener()
        {
            Dispose();
        }
        #endregion

        #region 日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg">日志</param>
        public void WriteLog(String msg)
        {
            if (Config.IsShow && OnWriteLog != null)
            {
                OnWriteLog(msg);
            }
        }
        #endregion
    }
}