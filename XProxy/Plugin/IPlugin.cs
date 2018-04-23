using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Plugin
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">插件管理器</param>
        void OnInit(PluginManager manager);

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="listener">监听器</param>
        void OnListenerStart(Listener listener);

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="listener">监听器</param>
        void OnListenerStop(Listener listener);

        /// <summary>
        /// 第一次向客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        Boolean OnClientStart(Session session);

        /// <summary>
        /// 连接远程服务器时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        Boolean OnServerStart(Session session);

        /// <summary>
        /// 客户端向服务器发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        Byte[] OnClientToServer(Session session, Byte[] Data);

        /// <summary>
        /// 服务器相客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        Byte[] OnServerToClient(Session session, Byte[] Data);

        /// <summary>
        /// 当前设置
        /// </summary>
        PluginConfig Config { get; set; }

        /// <summary>
        /// 默认设置
        /// </summary>
        PluginConfig DefaultConfig { get; }

        /// <summary>
        /// 写日志
        /// </summary>
        event WriteLogDelegate OnWriteLog;
    }
}