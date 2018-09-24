using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Plugin;
using XProxy.Config;
using System.Reflection;
using System.IO;
using XProxy.Base;

namespace XProxy.Http
{
	/// <summary>
	/// Http插件类
	/// </summary>
	public class HttpPlugin : PluginBase
	{
        #region 属性
        /// <summary>
        /// 请求延迟。
        /// 收到一个完整的请求后，才转发，否则直接转发。
        /// 如果要处理整个请求包，请启用延迟。
        /// 启用延迟将需要很大的内存资源来缓存数据。
        /// </summary>
        public Boolean RequestDelay { get; set; } = false;

        /// <summary>
        /// 响应延迟。
        /// 收到一个完整的响应后，才转发，否则直接转发。
        /// 如果要处理整个响应包，请启用延迟。
        /// 启用延迟将需要很大的内存资源来缓存数据。
        /// </summary>
        public Boolean ResponseDelay { get; set; } = false;
        /// <summary>
        /// 插件集合。处理各事件的时候，将按照先后顺序调用插件的处理方法。
        /// </summary>
        public IList<IHttpPlugin> Plugins { get; set; }

        /// <summary>
        /// 显示请求
        /// </summary>
        public Boolean ShowRequest { get; set; }
        /// <summary>
        /// 显示响应
        /// </summary>
        public Boolean ShowResponse { get; set; }
        #endregion

        #region 加载插件
        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="configs"></param>
        private void LoadPlugin(IList<PluginConfig> configs)
		{
			if (configs == null || configs.Count < 1) return;
			var list = new List<IHttpPlugin>();
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
							WriteLog(String.Format("加载Http插件出错：{0}\n{1}", config, ex));
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
								var p = obj as IHttpPlugin;
								if (p != null)
								{
									p.Config = config;
									list.Add(p);
									WriteLog(String.Format("加载Http插件：{0}", config));
								}
							}
						}
					}
				}
			}
			Plugins = list;
		}
		#endregion

		#region IPlugin 成员
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="manager">插件管理器</param>
		public override void OnInit(PluginManager manager)
		{
			Manager = manager;

			LoadPlugin(manager.Listener.Config.HttpPlugins);
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].OnWriteLog += new WriteLogDelegate(WriteLog);
				Plugins[i].OnInit(this);
			}

			//不让核心显示客户端和服务器数据，由这里来显示
			ShowRequest = manager.Listener.Config.IsShow && manager.Listener.Config.IsShowClientData;
            ShowResponse = manager.Listener.Config.IsShow && manager.Listener.Config.IsShowServerData;
            manager.Listener.Config.IsShowClientData = false;
            manager.Listener.Config.IsShowServerData = false;
		}

		public override Boolean OnClientStart(Session session)
		{
			//使用同步
			//session.IsAsync = false;

			return base.OnClientStart(session);
		}

		/// <summary>
		/// 客户端向服务器发数据时触发。
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>经过处理后的数据</returns>
		public override Byte[] OnClientToServer(Session session, Byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;

			//一般的请求都是ASCII编码，可以直接显示
			if (ShowRequest)
				session.WriteLog("请求头（" + Data.Length + "Byte）：\n" + Encoding.ASCII.GetString(Data));

			if (HttpHelper.IsHttpRequest(Data))
			{
				var p = ByteHelper.IndexOf(Data, "\r\n\r\n");
				if (p < 0) return null;
				var bts = ByteHelper.SubBytes(Data, 0, p);

				for (var i = 0; i < Plugins.Count; i++)
				{
					bts = Plugins[i].OnRequestHeader(session, bts);
					//直接阻止
					if (bts == null || bts.Length < 1) return null;
				}

				//String header = Encoding.ASCII.GetString(bts);

				//for (int i = 0; i < Plugins.Count; i++)
				//{
				//    header = Plugins[i].OnRequestHeader(session, header);
				//    //直接阻止
				//    if (String.IsNullOrEmpty(header)) return null;
				//}

				//bts = Encoding.ASCII.GetBytes(header + "\r\n\r\n");

				bts = ByteHelper.Cat(bts, Encoding.ASCII.GetBytes("\r\n\r\n"));

				//是否有数据需要处理
				if (Data.Length > p + 4)
				{
					Data = ByteHelper.SubBytes(Data, p + 4, -1);
					for (var i = 0; i < Plugins.Count; i++)
					{
						Data = Plugins[i].OnRequestContent(session, Data);
						//直接阻止
						if (Data == null || Data.Length < 1) return null;
					}
					Data = ByteHelper.Cat(bts, Data);
				}
				else
					Data = bts;
			}
			else
			{
				for (var i = 0; i < Plugins.Count; i++)
				{
					Data = Plugins[i].OnRequestContent(session, Data);
					//直接阻止
					if (Data == null || Data.Length < 1) return null;
				}
			}

			return Data;
		}

		/// <summary>
		/// 服务器相客户端发数据时触发。
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>经过处理后的数据</returns>
		public override Byte[] OnServerToClient(Session session, Byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;

			if (HttpHelper.IsHttpResponse(Data))
			{
				var p = ByteHelper.IndexOf(Data, "\r\n\r\n");
				if (p < 0) return null;
				var bts = ByteHelper.SubBytes(Data, 0, p);

				for (var i = 0; i < Plugins.Count; i++)
				{
					bts = Plugins[i].OnResponseHeader(session, bts);
					//直接阻止
					if (bts == null || bts.Length < 1) return null;
				}

				var header = Encoding.ASCII.GetString(bts);

				//对于响应，只能直接显示头部，只有头部可以用ASCII解码
				if (ShowResponse)
					session.WriteLog("响应头（" + Data.Length + "Byte）：\n" + header);

				//for (int i = 0; i < Plugins.Count; i++)
				//{
				//    header = Plugins[i].OnResponseHeader(session, header);
				//    //直接阻止
				//    if (String.IsNullOrEmpty(header)) return null;
				//}

				//bts = Encoding.ASCII.GetBytes(header + "\r\n\r\n");

				bts = ByteHelper.Cat(bts, Encoding.ASCII.GetBytes("\r\n\r\n"));

				//是否有数据需要处理
				if (Data.Length > p + 4)
				{
					Data = ByteHelper.SubBytes(Data, p + 4, -1);
					for (var i = 0; i < Plugins.Count; i++)
					{
						Data = Plugins[i].OnResponseContent(session, Data);
						//直接阻止
						if (Data == null || Data.Length < 1) return null;
					}
					Data = ByteHelper.Cat(bts, Data);
				}
				else
					Data = bts;
			}
			else
			{
                if (ShowResponse)
                {
#if !DEBUG
                    session.WriteLog("响应数据（" + Data.Length + "Byte）");
#else
                    session.WriteLog("响应数据（" + Data.Length + "Byte）：\n" + Encoding.Default.GetString(Data));
#endif
                }
				for (var i = 0; i < Plugins.Count; i++)
				{
					Data = Plugins[i].OnResponseContent(session, Data);
					//直接阻止
					if (Data == null || Data.Length < 1) return null;
				}
			}

			return Data;
		}

		/// <summary>
		/// 默认设置
		/// </summary>
		public override PluginConfig DefaultConfig
		{
			get
			{
				var pc = base.DefaultConfig;
				pc.Name = "Http插件";
				pc.Author = "大石头";
				return pc;
			}
		}

		#endregion

		#region IDisposable 成员
		/// <summary>
		/// 释放资源
		/// </summary>
		public override void Dispose()
		{
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].Dispose();
			}
		}

		#endregion
	}
}