using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using XProxy.Config;
using XProxy;
using XProxy.Base;

namespace XP
{
    public partial class ListenerManage : Form
    {
        public ListenerManage()
        {
            InitializeComponent();
        }

        private void ListenerManage_Load(Object sender, EventArgs e)
        {
        }

        private Int32 MsgCount = 0;
        ///<summary>输入日志</summary>
        ///<remarks>输入日志信息到UI信息框</remarks>
        ///<param name="log">要输出的日志信息</param>
        public void WriteLog(String log)
        {
            //if (!IsShow.Checked) return;
            if (txtLog.InvokeRequired) // 是否需要Invoke，外部线程使用该函数时，该属性为真
            {
                try
                {
                    var d = new WriteLogDelegate(WriteLog);
                    Invoke(d, new Object[] { log });
                }
                catch { }
            }
            else
            {
                MsgCount++;
                if (MsgCount > 100)
                {
                    txtLog.Clear();
                    MsgCount = 0;
                }
                //txtLog.Text += "\r\n" + log;
				txtLog.AppendText("\r\n" + log);
                txtLog.Select(txtLog.TextLength, 0);
                //然后移动滚动条，使输入点(text entry point)(即光标所在的位置）显示出来
                //这样也可以达到滚动到最下方的目的
                txtLog.ScrollToCaret();
            }
        }

        public void WriteLog(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.Append(ex.Message);
                sb.Append(Environment.NewLine);
                ex = ex.InnerException;
            }
            WriteLog(sb.ToString());
        }

        XProxySvc xps = new XProxySvc();
        private void button1_Click(Object sender, EventArgs e)
        {
            if (button1.Text == "开始")
            {
                xps.WriteLogEvent = new WriteLogDelegate(WriteLog);
                xps.StartService();
				if (xps.watcher != null) xps.watcher.EnableRaisingEvents = false;

                button1.Text = "停止";
            }
            else
            {
                xps.StopService();
                button1.Text = "开始";
            }
        }

        private void ListenerManage_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (button1.Text != "开始") xps.StopService();
            if (Owner != null) Owner.Visible = true;
        }

        private void timer1_Tick(Object sender, EventArgs e)
        {
            if (xps == null) return;
            if (xps.Listeners == null || xps.Listeners.Count < 1) return;

            var count = 0;
            foreach (var item in xps.Listeners)
            {
                if (item.Clients != null) count += item.Clients.Count;
            }
            label2.Text = count.ToString();
        }
    }
}