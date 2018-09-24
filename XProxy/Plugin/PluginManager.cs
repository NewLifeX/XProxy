using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using XProxy.Base;
using XProxy.Config;
using XProxy.Http;

namespace XProxy.Plugin
{
    /// <summary>
    /// 插件管理类
    /// </summary>
    public class PluginManager
    {
        #region 属性
        /// <summary>
        /// 插件集合。处理各事件的时候，将按照先后顺序调用插件的处理方法。
        /// </summary>
        public IList<IPlugin> Plugins { get; set; }

        /// <summary>
        /// 当前监听器
        /// </summary>
        public Listener Listener { get; set; }

        /// <summary>
        /// 写日志事件
        /// </summary>
        public event WriteLogDelegate OnWriteLog;
        #endregion

        #region 加载插件
        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="configs"></param>
        private void LoadPlugin(IList<PluginConfig> configs)
        {
            if (configs == null || configs.Count < 1) return;
            var list = new List<IPlugin>();
            foreach (var config in configs)
            {
                if (!String.IsNullOrEmpty(config.ClassName))
                {
                    Assembly asm;
                    if (String.IsNullOrEmpty(config.Path))
                        asm = Assembly.GetExecutingAssembly();
                    else
                    {
                        var path = config.Path;
                        if (!Path.IsPathRooted(path)) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                        try
                        {
                            asm = Assembly.LoadFile(path);
                        }
                        catch (Exception ex)
                        {
                            WriteLog(String.Format("加载插件出错：{0}\n{1}", config, ex.ToString()));
                            continue;
                        }
                    }
                    if (asm != null)
                    {
                        var t = asm.GetType(config.ClassName, false);
                        if (t == null)
                        {
                            var ts = asm.GetTypes();
                            foreach (var tt in ts)
                            {
                                if (tt.Name.Contains(config.ClassName))
                                {
                                    t = tt;
                                    break;
                                }
                            }
                        }
                        if (t != null)
                        {
                            var obj = Activator.CreateInstance(t);
                            if (obj != null)
                            {
                                var p = obj as IPlugin;
                                if (p != null)
                                {
                                    p.Config = config;
                                    list.Add(p);
                                    WriteLog(String.Format("加载插件：{0}", config));
                                }
                            }
                        }
                    }
                }
            }
            Plugins = list;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="listener">监听器</param>
        public void OnInit(Listener listener)
        {
            LoadPlugin(listener.Config.Plugins);
            if (Plugins == null || Plugins.Count < 1) return;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Plugins[i].OnWriteLog += new WriteLogDelegate(WriteLog);
                Plugins[i].OnInit(this);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void OnDispose()
        {
            if (Plugins == null || Plugins.Count < 1) return;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Plugins[i].Dispose();
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="listener">监听器</param>
        public void OnListenerStart(Listener listener)
        {
            if (Plugins == null || Plugins.Count < 1) return;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Plugins[i].OnListenerStart(listener);
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="listener">监听器</param>
        public void OnListenerStop(Listener listener)
        {
            if (Plugins == null || Plugins.Count < 1) return;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Plugins[i].OnListenerStop(listener);
            }
        }

        /// <summary>
        /// 第一次向客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        public Boolean OnClientStart(Session session)
        {
            if (Plugins == null || Plugins.Count < 1) return true;

            for (var i = 0; i < Plugins.Count; i++)
            {
                if (!Plugins[i].OnClientStart(session)) return false;
            }
            return true;
        }

        /// <summary>
        /// 连接远程服务器时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <returns>是否允许通行</returns>
        public Boolean OnServerStart(Session session)
        {
            if (Plugins == null || Plugins.Count < 1) return true;

            for (var i = 0; i < Plugins.Count; i++)
            {
                if (!Plugins[i].OnServerStart(session)) return false;
            }
            return true;
        }
        #endregion

        #region 数据交换事件
        /// <summary>
        /// 客户端向服务器发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后Data参数中的数据大小</returns>
        public Byte[] OnClientToServer(Session session, Byte[] Data)
        {
            if (Plugins == null || Plugins.Count < 1) return Data;
            if (Data == null || Data.Length < 1) return null;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Data = Plugins[i].OnClientToServer(session, Data);
                if (Data == null || Data.Length < 1) return null;
            }
            return Data;
        }

        /// <summary>
        /// 服务器相客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后Data参数中的数据大小</returns>
        public Byte[] OnServerToClient(Session session, Byte[] Data)
        {
            if (Plugins == null || Plugins.Count < 1) return Data;
            if (Data == null || Data.Length < 1) return null;

            for (var i = 0; i < Plugins.Count; i++)
            {
                Data = Plugins[i].OnServerToClient(session, Data);
                if (Data == null || Data.Length < 1) return null;
            }
            return Data;
        }
        #endregion

        #region 日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg">日志</param>
        public void WriteLog(String msg)
        {
            if (OnWriteLog != null)
            {
                if (Listener == null)
                    OnWriteLog(msg);
                else
                    OnWriteLog(String.Format("[{0}] {1}", Listener.Config.Name, msg));
            }
        }
        #endregion

        #region 静态 插件管理
        private static PluginConfig[] _AllPlugins;
        /// <summary>
        /// 所有插件
        /// </summary>
        public static PluginConfig[] AllPlugins
        {
            get
            {
                LoadAllAssembly();

                var list = new List<PluginConfig>();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.GetInterface(typeof(IPlugin).Name) != null)
                        {
                            try
                            {
                                var ip = asm.CreateInstance(t.FullName) as IPlugin;
                                if (ip != null) list.Add(ip.DefaultConfig);
                            }
                            catch { }
                        }
                    }
                }
                _AllPlugins = new PluginConfig[list.Count];
                list.CopyTo(_AllPlugins);

                return _AllPlugins;
            }
        }

        private static PluginConfig[] _AllHttpPlugins;
        /// <summary>
        /// 所有Http插件
        /// </summary>
        public static PluginConfig[] AllHttpPlugins
        {
            get
            {
                LoadAllAssembly();

                var list = new List<PluginConfig>();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in asm.GetTypes())
                    {
                        //加上IsAbstract的判断，防止把抽象插件基类识别为插件
                        if (!t.IsAbstract && t.GetInterface(typeof(IHttpPlugin).Name) != null)
                        {
                            try
                            {
                                var ip = asm.CreateInstance(t.FullName) as IHttpPlugin;
                                if (ip != null) list.Add(ip.DefaultConfig);
                            }
                            catch { }
                        }
                    }
                }
                _AllHttpPlugins = new PluginConfig[list.Count];
                list.CopyTo(_AllHttpPlugins);

                return _AllHttpPlugins;
            }
        }

        private static void LoadAllAssembly()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            //path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path)) return;

            //不能使用AllDirectories，当很多子目录的时候，会卡死程序
            var fs = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            if (fs == null || fs.Length < 1) return;

            foreach (var s in fs)
            {
                try
                {
                    Assembly.LoadFile(s);
                }
                catch { }
            }
        }
        #endregion
    }
}