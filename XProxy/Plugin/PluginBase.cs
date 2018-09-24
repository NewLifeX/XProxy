using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Reflection;
using XProxy.Base;

namespace XProxy.Plugin
{
    /// <summary>
    /// 插件基类。
    /// 可以直接继承该类来实现插件。
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        #region 属性
        private PluginManager _Manager;
        /// <summary>
        /// 插件管理器
        /// </summary>
        public virtual PluginManager Manager { get { return _Manager; } set { _Manager = value; } }

        /// <summary>
        /// 写日志事件
        /// </summary>
        public virtual event WriteLogDelegate OnWriteLog;
        #endregion

        #region IPlugin 成员
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">插件管理器</param>
        public virtual void OnInit(PluginManager manager)
        {
            Manager = manager;
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="listener">监听器</param>
        public virtual void OnListenerStart(Listener listener)
        {
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="listener">监听器</param>
        public virtual void OnListenerStop(Listener listener)
        {
        }

        /// <summary>
        /// 第一次向客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        public virtual Boolean OnClientStart(Session session)
        {
            return true;
        }

        /// <summary>
        /// 连接远程服务器时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        public virtual Boolean OnServerStart(Session session)
        {
            return true;
        }

        /// <summary>
        /// 客户端向服务器发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        public virtual Byte[] OnClientToServer(Session session, Byte[] Data)
        {
            return Data;
        }

        /// <summary>
        /// 服务器相客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        public virtual Byte[] OnServerToClient(Session session, Byte[] Data)
        {
            return Data;
        }
        /// <summary>
        /// 当前设置
        /// </summary>
        public PluginConfig Config { get; set; }

        /// <summary>
        /// 默认设置
        /// </summary>
        public virtual PluginConfig DefaultConfig
        {
            get
            {
                var pc = new PluginConfig();
                //pc.Name = "未命名插件";
				pc.Name = GetType().Name;
				pc.Author = "无名";
                pc.ClassName = GetType().FullName;
				//如果是内部插件，则只显示类名，而不显示全名
				if (Assembly.GetExecutingAssembly() == GetType().Assembly)
					pc.ClassName = GetType().Name;
				pc.Version = GetType().Assembly.GetName().Version.ToString();
				pc.Path = System.IO.Path.GetFileName(GetType().Assembly.Location);
				return pc;
            }
        }

        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
        }
        #endregion

        #region 日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg">日志</param>
        public virtual void WriteLog(String msg)
        {
            if (OnWriteLog != null)
            {
                OnWriteLog(msg);
            }
        }
        #endregion

        /// <summary>
        /// 已重载。
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return Config == null ? base.ToString() : Config.ToString();
        }
    }
}