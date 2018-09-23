using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using NewLife.Log;
using XProxy.Base;
using XProxy.Config;

namespace XP
{
    partial class XProxySvc : ServiceBase
    {
        public XProxySvc() => InitializeComponent();

        private WriteLogDelegate _WriteLogEvent;
        /// <summary>
        /// 写日志事件
        /// </summary>
        public WriteLogDelegate WriteLogEvent
        {
            get
            {
                if (_WriteLogEvent == null) return new WriteLogDelegate(XTrace.WriteLine);
                return _WriteLogEvent;
            }
            set { _WriteLogEvent = value; }
        }

        protected override void OnStart(String[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            XTrace.WriteLine("服务启动！");

            StartService();
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            XTrace.WriteLine("服务停止！");

            StopService();
        }

        /// <summary>
        /// 监听器集合
        /// </summary>
        public IList<Listener> Listeners = new List<Listener>();

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            var config = LoadConfig();
            if (config == null)
            {
                XTrace.WriteLine("无法找到配置文件！");
                return;
            }

            foreach (var lc in config.Listeners)
            {
                if (lc.Enable)
                {
                    try
                    {
                        var listener = new Listener(lc);
                        Listeners.Add(listener);
                        if (lc.IsShow) listener.OnWriteLog += WriteLogEvent;
                        listener.Start();
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteLine("启动[" + lc.Name + "]时出错！" + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            if (Listeners == null || Listeners.Count < 1) return;

            foreach (var listener in Listeners)
            {
                try
                {
                    listener.Dispose();
                }
                catch (Exception ex)
                {
                    XTrace.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 配置文件监视
        /// </summary>
        public FileSystemWatcher watcher;

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        ProxyConfig LoadConfig()
        {
            if (!File.Exists(ProxyConfig.DefaultFile)) return null;

            var config = ProxyConfig.Instance;
            if (config == null || config.Listeners == null || config.Listeners.Length < 1) return null;
            //服务不会修改配置文件，所以不保存
            //config.IsSaved = true;

            //建立监视器
            if (watcher == null)
            {
                watcher = new FileSystemWatcher();
                var path = ProxyConfig.DefaultFile;
                watcher.Path = Path.GetDirectoryName(path);
                watcher.Filter = Path.GetFileName(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                watcher.EnableRaisingEvents = true;
                //XTrace.WriteLine("建立监视器！");
            }
            return config;
        }

        /// <summary>
        /// 配置文件修改时重启服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Changed(Object sender, FileSystemEventArgs e)
        {
            XTrace.WriteLine("配置文件被修改！重新加载配置！");
            StopService();
            StartService();
        }
    }
}