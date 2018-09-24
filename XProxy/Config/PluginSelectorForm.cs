using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace XProxy.Config
{
	internal partial class PluginSelectorForm : Form
	{
		public PluginSelectorForm()
		{
			InitializeComponent();
		}
        /// <summary>
        /// 选择项
        /// </summary>
        public PluginConfig SelectedItem { get; set; }

        private PluginConfig[] _Plugins;
		/// <summary>
		/// 插件集合
		/// </summary>
		public PluginConfig[] Plugins { get { return _Plugins; } set { _Plugins = value; } }

		private void PluginSelectorForm_Load(Object sender, EventArgs e)
		{
			listView1.Items.Clear();
			if (Plugins == null || Plugins.Length < 1) return;

			foreach (var item in Plugins)
			{
				var lv = listView1.Items.Add(item.Name);
				lv.SubItems.Add(item.Author);
				lv.SubItems.Add(item.ClassName);
				lv.SubItems.Add(item.Version);
				lv.SubItems.Add(item.Path);
				lv.Tag = item;
			}
		}

		private void listView1_DoubleClick(Object sender, EventArgs e)
		{
			if (listView1.SelectedItems == null || listView1.SelectedItems.Count < 1) return;
			SelectedItem = listView1.SelectedItems[0].Tag as PluginConfig;
			Close();
		}
	}
}