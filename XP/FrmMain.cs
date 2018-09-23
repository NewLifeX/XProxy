using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using XProxy.Config;
using XProxy;
using XProxy.Http;
using XProxy.Plugin;
using System.Diagnostics;
using System.IO;

namespace XP
{
    public partial class FrmMain : Form
    {
        #region 构造函数 属性
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(Object sender, EventArgs e)
        {
            button2.Enabled = false;
            button3.Enabled = false;
            LoadConfig();
        }

        /// <summary>
        /// 配置
        /// </summary>
        public ProxyConfig Config
        {
            get
            {
                return ProxyConfig.Instance;
            }
        }
        #endregion

        #region 监听器配置
        private void button1_Click(Object sender, EventArgs e)
        {
            //新增
            if (Config == null) ProxyConfig.Instance = new ProxyConfig();
            ListenerConfig[] lcs = null;
            if (Config.Listeners == null)
                lcs = new ListenerConfig[1];
            else
            {
                lcs = new ListenerConfig[Config.Listeners.Length + 1];
                Config.Listeners.CopyTo(lcs, 0);
            }
            var lc = new ListenerConfig();
            lcs[lcs.Length - 1] = lc;
            lc.Name = "未命名" + lcs.Length.ToString();
            Config.Listeners = lcs;

            LoadConfig();

            propertyGrid1.SelectedObject = lc;
        }

        private void button2_Click(Object sender, EventArgs e)
        {
            //删除
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count < 1) return;

            if (Config.Listeners == null) return;
            var lc = listView1.SelectedItems[0].Tag as ListenerConfig;
            if (lc == null) return;

            var lcs = new ListenerConfig[Config.Listeners.Length - 1];
            for (Int32 i = 0, j = 0; i < Config.Listeners.Length; i++)
            {
                if (Config.Listeners[i] != lc) lcs[j++] = Config.Listeners[i];
            }
            Config.Listeners = lcs;

            LoadConfig();
            button2.Enabled = false;
        }

        private void button3_Click(Object sender, EventArgs e)
        {
            //保存
            ProxyConfig.Save(ProxyConfig.DefaultFile, ProxyConfig.Instance);
            button3.Enabled = false;
        }

        private void button4_Click(Object sender, EventArgs e)
        {
            //取消
            ProxyConfig.Instance = ProxyConfig.Load(ProxyConfig.DefaultFile);

            LoadConfig();
        }

        void LoadConfig()
        {
            var pc = ProxyConfig.Instance;
            if (pc == null) return;
            var list = pc.Listeners;
            if (list == null || list.Length < 1) return;
            Tag = list;
            listView1.Items.Clear();
            foreach (var lc in list)
            {
                var str = lc.Name;
                if (String.IsNullOrEmpty(str))
                {
                    str = "未命名";
                }
                var item = listView1.Items.Add("");
                //item.ImageList = imageList1;
                item.ImageKey = lc.Enable ? "Ok.ico" : "Delete.ico";
                item.SubItems.Add(str);
                item.SubItems.Add(lc.Port.ToString());
                item.SubItems.Add(lc.ServerAddress + ":" + lc.ServerPort.ToString());
                item.Tag = lc;
            }
            propertyGrid1.SelectedObject = null;
        }
        #endregion

        #region 服务控制
        private void button5_Click(Object sender, EventArgs e)
        {
            ControlService(false);
        }

        private void button6_Click(Object sender, EventArgs e)
        {
            ControlService(true);
        }

        private void button7_Click(Object sender, EventArgs e)
        {
            Program.Install(false);
        }

        private void button8_Click(Object sender, EventArgs e)
        {
            Program.Install(true);
        }

        /// <summary>
        /// 启动、停止 服务
        /// </summary>
        /// <param name="isstart"></param>
        void ControlService(Boolean isstart)
        {
            var p = new Process();
            var si = new ProcessStartInfo();
            var path = Environment.SystemDirectory;
            path = Path.Combine(path, @"cmd.exe");
            si.FileName = path;
            if (isstart)
                si.Arguments = @"/c net start XProxySvc";
            else
                si.Arguments = @"/c net stop XProxySvc";
            si.UseShellExecute = false;
            si.CreateNoWindow = false;
            p.StartInfo = si;
            p.Start();
            p.WaitForExit();
        }
        #endregion

        private void listView1_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count < 1)
            {
                button2.Enabled = false;
                return;
            }

            propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;

            button2.Enabled = true;
        }

        private void propertyGrid1_PropertyValueChanged(Object s, PropertyValueChangedEventArgs e)
        {
            //允许保存
            button3.Enabled = true;
            var obj = propertyGrid1.SelectedObject;
            //if (e.ChangedItem.PropertyDescriptor.Name == "Name") 
            LoadConfig();
            propertyGrid1.SelectedObject = obj;
        }

        private void linkLabel1_LinkClicked(Object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(Object sender, EventArgs e)
        {
            //new XProxySvc().StartService();
            Visible = false;
            new ListenerManage().ShowDialog(this);
        }

        #region 注释
        //private Boolean CanSave = false;

        //private void FrmMain_Load(object sender, EventArgs e)
        //{
        //    //这个时候不允许保存，因为下面会修改各个属性
        //    CanSave = false;

        //    //加载配置
        //    cbAddress.Text = Config.Address;
        //    txtPort.Text = Config.Port.ToString();
        //    txtServerAddress.Text = Config.ServerAddress;
        //    txtServerPort.Text = Config.ServerPort.ToString();
        //    //IsServer.Checked = Config.IsServer;
        //    IsShow.Checked = Config.IsShow;
        //    IsShowClientData.Checked = Config.IsShowClientData;
        //    IsShowServerData.Checked = Config.IsShowServerData;

        //    PluginConfig[] plugins = PluginManager.AllHttpPlugins;
        //    comboBox1.Items.Clear();
        //    foreach (PluginConfig pc in plugins)
        //    {
        //        comboBox1.Items.Add(pc.Name);
        //    }

        //    //检查插件
        //    if (Config.Plugins != null &&
        //        Config.Plugins.Length > 0 &&
        //        Config.Plugins[0].ClassName == typeof(HttpPlugin).FullName)
        //    {
        //        if (Config.HttpPlugins != null &&
        //            Config.HttpPlugins.Length > 0)
        //        {
        //            comboBox1.Text = Config.HttpPlugins[0].Name;
        //        }
        //    }

        //    // 监听IP列表
        //    String sel = cbAddress.Text;
        //    IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        //    List<IPAddress> list = new List<IPAddress>(ips);
        //    list.Add(IPAddress.Any);
        //    list.Add(IPAddress.Loopback);
        //    if (ips != null)
        //    {
        //        cbAddress.Items.Clear();
        //        foreach (IPAddress ip in list)
        //        {
        //            cbAddress.Items.Add(ip.ToString());
        //        }
        //        cbAddress.Text = sel;
        //    }

        //    //这个时候，才允许保存
        //    CanSave = true;

        //    //设置默认插件，这将会写入配置文件
        //    if (String.IsNullOrEmpty(comboBox1.Text))
        //    {
        //        comboBox1.Text = plugins[0].Name;
        //    }
        //}

        //private void txtPort_TextChanged(object sender, EventArgs e)
        //{
        //    if (!CanSave) return;

        //    //保存配置
        //    Config.Address = cbAddress.Text;
        //    Config.Port = int.Parse(txtPort.Text);
        //    Config.ServerAddress = txtServerAddress.Text;
        //    Config.ServerPort = int.Parse(txtServerPort.Text);
        //    //Config.IsServer = IsServer.Checked;
        //    Config.IsShow = IsShow.Checked;
        //    Config.IsShowClientData = IsShowClientData.Checked;
        //    Config.IsShowServerData = IsShowServerData.Checked;
        //    Config.Plugins = new PluginConfig[] { (new HttpPlugin()).DefaultConfig };

        //    PluginConfig[] plugins = PluginManager.AllHttpPlugins;
        //    foreach (PluginConfig p in plugins)
        //    {
        //        if (comboBox1.Text == p.Name)
        //            Config.HttpPlugins = new PluginConfig[] { p };
        //    }
        //    if (Config.HttpPlugins == null)
        //        Config.HttpPlugins = new PluginConfig[] { plugins[0] };

        //    ProxyConfig pc = new ProxyConfig();
        //    pc.Listeners = new ListenerConfig[] { Config };
        //    ProxyConfig.Save("XP.xml", pc);
        //}

        //private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    txtPort_TextChanged(sender, e);
        //}

        //private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    txtServerAddress.Visible = false;
        //    txtServerPort.Visible = false;
        //    label3.Visible = false;
        //    label4.Visible = false;
        //    if (comboBox1.Text == "间接客户" || comboBox1.Text == "反向代理")
        //    {
        //        txtServerAddress.Visible = true;
        //        txtServerPort.Visible = true;
        //        label3.Visible = true;
        //        label4.Visible = true;
        //    }

        //    txtPort_TextChanged(sender, e);
        //}
        //#endregion

        //Listener listener;
        //private void btnStart_Click(object sender, EventArgs e)
        //{
        //    if (btnStart.Text == "开始")
        //    {
        //        try
        //        {
        //            listener = new Listener(Config);
        //            listener.OnWriteLog += new XProxy.WriteLogDelegate(WriteLog);

        //            Client.TimeOut = 180000;    // 三分钟超时

        //            listener.Start();

        //            groupBox1.Enabled = false;
        //            btnStart.Text = "停止";
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(ex);
        //        }
        //    }
        //    else
        //    {
        //        try
        //        {
        //            listener.Stop();
        //            listener.Dispose();
        //            listener = null;

        //            groupBox1.Enabled = true;
        //            btnStart.Text = "开始";
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(ex);
        //        }
        //    }
        //}

        //private int MsgCount = 0;
        /////<summary>输入日志</summary>
        /////<remarks>输入日志信息到UI信息框</remarks>
        /////<param name="log">要输出的日志信息</param>
        //public void WriteLog(string log)
        //{
        //    if (!IsShow.Checked) return;
        //    if (this.txtLog.InvokeRequired) // 是否需要Invoke，外部线程使用该函数时，该属性为真
        //    {
        //        WriteLogDelegate d = new WriteLogDelegate(WriteLog);
        //        this.Invoke(d, new object[] { log });
        //    }
        //    else
        //    {
        //        MsgCount++;
        //        if (MsgCount > 100)
        //        {
        //            txtLog.Clear();
        //            MsgCount = 0;
        //        }
        //        txtLog.Text += "\r\n" + log;
        //        txtLog.Select(txtLog.TextLength, 0);
        //        //然后移动滚动条，使输入点(text entry point)(即光标所在的位置）显示出来
        //        //这样也可以达到滚动到最下方的目的
        //        txtLog.ScrollToCaret();
        //    }
        //}

        //public void WriteLog(Exception ex)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    while (ex != null)
        //    {
        //        sb.Append(ex.Message);
        //        sb.Append(Environment.NewLine);
        //        ex = ex.InnerException;
        //    }
        //    WriteLog(sb.ToString());
        //}

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (listener != null && listener.Clients != null)
        //            lbCount.Text = listener.Clients.Count.ToString();
        //    }
        //    catch { }
        //}

        //private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        //{
        //    this.Visible = !this.Visible;
        //    if (this.Visible == true)
        //    {
        //        this.Activate();
        //        this.BringToFront();
        //    }
        //}

        //private void btnConfig_Click(object sender, EventArgs e)
        //{
        //    (new ListenerManage()).ShowDialog();
        //}
        #endregion
    }
}