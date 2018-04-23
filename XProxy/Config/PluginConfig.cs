using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms;
using XProxy.Plugin;

namespace XProxy.Config
{
	/// <summary>
	/// 插件配置
	/// </summary>
	[Serializable]
	[Description("插件配置")]
	public class PluginConfig
	{
		private String _Name = "未命名";
		/// <summary>
		/// 插件名
		/// </summary>
		[ReadOnly(true)]
		[Category("基本"), DefaultValue("未命名插件"), Description("插件名")]
		public String Name { get { return _Name; } set { _Name = value; } }

		private String _Author;
		/// <summary>
		/// 插件作者
		/// </summary>
		[ReadOnly(true)]
		[Category("基本"), DefaultValue("无名"), Description("插件作者")]
		public String Author { get { return _Author; } set { _Author = value; } }

		private String _Version;
		/// <summary>
		/// 插件版本
		/// </summary>
		[ReadOnly(true)]
		[Category("基本"), Description("插件版本")]
		public String Version { get { return _Version; } set { _Version = value; } }

		private String _Path;
		/// <summary>
		/// 插件路径
		/// </summary>
		[ReadOnly(true)]
		//[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		[Category("配置"), Description("插件路径")]
		public String Path { get { return _Path; } set { _Path = value; } }

		private String _ClassName;
		/// <summary>
		/// 类名
		/// </summary>
		[ReadOnly(true)]
		[Category("配置"), Description("类名")]
		public String ClassName { get { return _ClassName; } set { _ClassName = value; } }

		private String _Extend;
		/// <summary>
		/// 扩展信息1
		/// </summary>
		[Category("扩展"), Description("扩展信息")]
		public String Extend { get { return _Extend; } set { _Extend = value; } }

		private String _Extend2;
		/// <summary>
		/// 扩展信息2
		/// </summary>
		[Category("扩展"), Description("扩展信息二")]
		public String Extend2 { get { return _Extend2; } set { _Extend2 = value; } }

		/// <summary>
		/// 已重载
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Name);
			if (!String.IsNullOrEmpty(Author)) sb.AppendFormat("（{0}）", Author);
			if (!String.IsNullOrEmpty(ClassName)) sb.AppendFormat("，{0} V{1}", ClassName, Version);
			if (!String.IsNullOrEmpty(Path)) sb.AppendFormat("，{0}", Path);
			//sb.AppendFormat("{0}（{1}），{2} {3}，{4}", Name, Author, ClassName, Version, Path);
			return sb.ToString();
		}
	}

	/// <summary>
	/// 插件集合编辑器
	/// </summary>
	public class PluginsEditor : ArrayEditor
	{
		/// <summary>
		/// 已重载
		/// </summary>
		/// <param name="type"></param>
		public PluginsEditor(Type type)
			: base(type)
		{
		}

		/// <summary>
		/// 已重载
		/// </summary>
		/// <param name="itemType"></param>
		/// <returns></returns>
		protected override object CreateInstance(Type itemType)
		{
			PluginSelectorForm form = new PluginSelectorForm();
			form.Plugins = PluginManager.AllPlugins;
			form.ShowDialog();
			if (form.SelectedItem != null)
			{
				PluginConfig pc = form.SelectedItem;
				form.Dispose();
				return pc;
			}
			form.Dispose();
			return null;
		}
	}

	/// <summary>
	/// Http插件集合编辑器
	/// </summary>
	public class HttpPluginsEditor : PluginsEditor
	{
		/// <summary>
		/// 已重载
		/// </summary>
		/// <param name="type"></param>
		public HttpPluginsEditor(Type type)
			: base(type)
		{
		}

		/// <summary>
		/// 已重载
		/// </summary>
		/// <param name="itemType"></param>
		/// <returns></returns>
		protected override object CreateInstance(Type itemType)
		{
			PluginSelectorForm form = new PluginSelectorForm();
			form.Plugins = PluginManager.AllHttpPlugins;
			form.ShowDialog();
			if (form.SelectedItem != null)
			{
				PluginConfig pc = form.SelectedItem;
				form.Dispose();
				return pc;
			}
			form.Dispose();
			return null;
		}
	}
}