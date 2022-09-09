namespace NewLife.Net.Proxy;

/// <summary>通用NAT代理。所有收到的数据，都转发到指定目标</summary>
public class NATProxy : ProxyBase
{
    #region 属性
    /// <summary>远程服务器地址</summary>
    public NetUri RemoteServer { get; set; } = new NetUri();

    /// <summary>端口数。支持映射一批端口到目标服务器的对应端口上</summary>
    public Int32 Ports { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public NATProxy() { }

    /// <summary>实例化</summary>
    /// <param name="hostname">目标服务器地址</param>
    /// <param name="port">目标服务器端口</param>
    public NATProxy(String hostname, Int32 port) : this(hostname, port, NetType.Tcp) { }

    /// <summary>实例化</summary>
    /// <param name="hostname">目标服务器地址</param>
    /// <param name="port">目标服务器端口</param>
    /// <param name="protocol">协议</param>
    public NATProxy(String hostname, Int32 port, NetType protocol)
        : this() => RemoteServer = new NetUri(protocol, hostname, port);
    #endregion

    #region 方法
    public override void Init(String config)
    {
        var dic = config?.SplitAsDictionary("=", ";");
        if (dic != null && dic.Count > 0)
        {
            Ports = dic["Ports"].ToInt();
        }
    }

    /// <summary>开始</summary>
    protected override void OnStart()
    {
        WriteLog("NAT代理 => {0}", RemoteServer);

        if (RemoteServer.Type == 0) RemoteServer.Type = ProtocolType;

        // 多端口
        if (Ports > 1)
        {
            for (var i = 0; i < Ports; i++)
            {
                var list = CreateServer(Local.Address, Port + i, Local.Type, AddressFamily);
                foreach (var item in list)
                {
                    AttachServer(item);
                }
            }
        }

        base.OnStart();
    }

    /// <summary>添加会话。子类可以在添加会话前对会话进行一些处理</summary>
    /// <param name="session"></param>
    protected override void AddSession(INetSession session)
    {
        var svr = RemoteServer;

        // 多端口
        if (Ports > 1)
        {
            // 算出来当前端口的索引位置
            var port = session.Session.Local.Port;
            var index = port - Port;

            // 计算目标端口
            svr = new NetUri(svr.ToString());
            svr.Port += index;
        }

        var ps = session as ProxySession;
        ps.RemoteServerUri = svr;
        // 如果不是Tcp/Udp，则使用本地协议
        if (!RemoteServer.IsTcp && !RemoteServer.IsUdp)
            ps.RemoteServerUri.Type = Local.Type;

        base.AddSession(session);
    }
    #endregion
}