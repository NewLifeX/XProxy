namespace XProxy.Config
{
	partial class PluginSelectorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginSelectorForm));
			this.listView1 = new System.Windows.Forms.ListView();
			this.名称 = new System.Windows.Forms.ColumnHeader();
			this.作者 = new System.Windows.Forms.ColumnHeader();
			this.类名 = new System.Windows.Forms.ColumnHeader();
			this.版本 = new System.Windows.Forms.ColumnHeader();
			this.路径 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.名称,
            this.作者,
            this.类名,
            this.版本,
            this.路径});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(616, 258);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			// 
			// 名称
			// 
			this.名称.Text = "名称";
			this.名称.Width = 80;
			// 
			// 作者
			// 
			this.作者.Text = "作者";
			// 
			// 类名
			// 
			this.类名.Text = "类名";
			this.类名.Width = 150;
			// 
			// 版本
			// 
			this.版本.Text = "版本";
			this.版本.Width = 120;
			// 
			// 路径
			// 
			this.路径.Text = "路径";
			this.路径.Width = 200;
			// 
			// PluginSelectorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(616, 258);
			this.Controls.Add(this.listView1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PluginSelectorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "插件选择窗体";
			this.Load += new System.EventHandler(this.PluginSelectorForm_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader 名称;
		private System.Windows.Forms.ColumnHeader 作者;
		private System.Windows.Forms.ColumnHeader 类名;
		private System.Windows.Forms.ColumnHeader 版本;
		private System.Windows.Forms.ColumnHeader 路径;

	}
}