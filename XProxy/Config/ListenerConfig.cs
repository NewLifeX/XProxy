using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel.Design;
using System.Drawing.Design;
using XProxy.Http;

namespace XProxy.Config
{
	/// <summary>
	/// 设置类
	/// </summary>
	[Serializable]
	public class ListenerConfig
	{
		#region 属性
		private String _Name = "未命名";
		/// <summary>
		/// 监听器名
		/// </summary>
		[Category(" 名称"), DefaultValue("未命名"), Description("监听器名")]
		public String Name { get { return _Name; } set { _Name = value; } }

		private Boolean _Enable = true;
		/// <summary>
		/// 激活
		/// </summary>
		[Category(" 名称"), DefaultValue(true), Description("激活")]
		public Boolean Enable { get { return _Enable; } set { _Enable = value; } }

		private String _Address = "0.0.0.0";
		/// <summary>
		/// 监听地址
		/// </summary>
		[Category("基本属性"), DefaultValue("0.0.0.0"), Description("监听地址")]
		public String Address
		{
			get { return _Address; }
			set { _Address = value; }
		}

		private Int32 _Port = 808;
		/// <summary>
		/// 监听端口
		/// </summary>
		[Category("基本属性"), DefaultValue(808), Description("监听端口")]
		public Int32 Port
		{
			get { return _Port; }
			set { _Port = value; }
		}

		private String _ServerAddress = "127.0.0.1";
		/// <summary>
		/// 远程服务器地址
		/// </summary>
		[Category("基本属性"), DefaultValue("127.0.0.1"), Description("远程服务器地址")]
		public String ServerAddress
		{
			get { return _ServerAddress; }
			set { _ServerAddress = value; }
		}

		private Int32 _ServerPort = 80;
		/// <summary>
		/// 远程服务器端口
		/// </summary>
		[Category("基本属性"), DefaultValue(80), Description("远程服务器端口")]
		public Int32 ServerPort
		{
			get { return _ServerPort; }
			set { _ServerPort = value; }
		}

        private Boolean _IsAsync = true;
        /// <summary>
        /// 使用异步
        /// </summary>
        [Category("基本属性"), DefaultValue(true), Description("使用异步")]
        public Boolean IsAsync
        {
            get { return _IsAsync; }
            set { _IsAsync = value; }
        }

		private Boolean _IsShow = false;
		/// <summary>
		/// 显示日志
		/// </summary>
		[Category("日志"), DefaultValue(false), Description("显示日志")]
		public Boolean IsShow
		{
			get { return _IsShow; }
			set { _IsShow = value; }
		}

		private Boolean _IsShowClientData = false;
		/// <summary>
		/// 客户端
		/// </summary>
		[Category("日志"), DefaultValue(false), Description("显示客户端日志")]
		public Boolean IsShowClientData
		{
			get { return _IsShowClientData; }
			set { _IsShowClientData = value; }
		}

		private Boolean _IsShowServerData = false;
		/// <summary>
		/// 服务端
		/// </summary>
		[Category("日志"), DefaultValue(false), Description("显示服务端日志")]
		public Boolean IsShowServerData
		{
			get { return _IsShowServerData; }
			set { _IsShowServerData = value; }
		}

		private PluginConfig[] _Plugins;
		/// <summary>
		/// 插件集合
		/// </summary>
		[Editor(typeof(PluginsEditor), typeof(UITypeEditor))]
		[Category("插件"), Description("插件集合")]
		public PluginConfig[] Plugins { get { return _Plugins; } set { _Plugins = value; } }

		private PluginConfig[] _HttpPlugins;
		/// <summary>
		/// Http插件
		/// </summary>
		[Editor(typeof(HttpPluginsEditor), typeof(UITypeEditor))]
		[Category("插件"), Description("Http插件")]
		public PluginConfig[] HttpPlugins
		{
			get { return _HttpPlugins; }
			set
			{
				_HttpPlugins = value;
				//检查Plugins中是否存在HttpPlugin插件，如果没有则加上
				if (Plugins != null && Plugins.Length > 0)
					foreach (PluginConfig item in Plugins)
					{
						if (item.ClassName == typeof(HttpPlugin).Name ||
							item.ClassName == typeof(HttpPlugin).FullName)
						{
							return;
						}
					}
				PluginConfig[] list = new PluginConfig[Plugins == null ? 1 : Plugins.Length + 1];
				PluginConfig pc = new HttpPlugin().DefaultConfig;
				list[list.Length - 1] = pc;
				Plugins = list;
			}
		}
		#endregion
	}
}