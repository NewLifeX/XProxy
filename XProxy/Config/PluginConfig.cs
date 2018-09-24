using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text;
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
        /// <summary>
        /// 插件名
        /// </summary>
        [ReadOnly(true)]
        [Category("基本"), DefaultValue("未命名插件"), Description("插件名")]
        public String Name { get; set; } = "未命名";

        /// <summary>
        /// 插件作者
        /// </summary>
        [ReadOnly(true)]
        [Category("基本"), DefaultValue("无名"), Description("插件作者")]
        public String Author { get; set; }

        /// <summary>
        /// 插件版本
        /// </summary>
        [ReadOnly(true)]
        [Category("基本"), Description("插件版本")]
        public String Version { get; set; }

        /// <summary>
        /// 插件路径
        /// </summary>
        [ReadOnly(true)]
        //[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        [Category("配置"), Description("插件路径")]
        public String Path { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        [ReadOnly(true)]
        [Category("配置"), Description("类名")]
        public String ClassName { get; set; }

        /// <summary>
        /// 扩展信息1
        /// </summary>
        [Category("扩展"), Description("扩展信息")]
        public String Extend { get; set; }
        /// <summary>
        /// 扩展信息2
        /// </summary>
        [Category("扩展"), Description("扩展信息二")]
        public String Extend2 { get; set; }

        /// <summary>
        /// 已重载
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            var sb = new StringBuilder();
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
        protected override Object CreateInstance(Type itemType)
        {
            var form = new PluginSelectorForm
            {
                Plugins = PluginManager.AllPlugins
            };
            form.ShowDialog();
            if (form.SelectedItem != null)
            {
                var pc = form.SelectedItem;
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
        protected override Object CreateInstance(Type itemType)
        {
            var form = new PluginSelectorForm
            {
                Plugins = PluginManager.AllHttpPlugins
            };
            form.ShowDialog();
            if (form.SelectedItem != null)
            {
                var pc = form.SelectedItem;
                form.Dispose();
                return pc;
            }
            form.Dispose();
            return null;
        }
    }
}