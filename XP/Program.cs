using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using NewLife.Log;

namespace XP
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            XTrace.UseWinForm();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (args[1].ToLower() == "-s")  //启动服务
                {
                    var ServicesToRun = new ServiceBase[] { new XProxySvc() };
                    try
                    {
                        ServiceBase.Run(ServicesToRun);
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteLine(ex.ToString());
                        Console.WriteLine(ex.ToString());
                    }
                    return;
                }
                else if (args[1].ToLower() == "-i") //安装服务
                {
                    Install(true);
                    return;
                }
                else if (args[1].ToLower() == "-u") //卸载服务
                {
                    Install(false);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }

        /// <summary>
        /// 安装、卸载 服务
        /// </summary>
        /// <param name="isinstall">是否安装</param>
        public static void Install(Boolean isinstall)
        {
            var p = new Process();
            var si = new ProcessStartInfo();
            //String path = Environment.GetEnvironmentVariable("SystemRoot");
            //path = Path.Combine(path, @"Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe");
            var path = Environment.SystemDirectory;
            path = Path.Combine(path, @"sc.exe");
            if (!File.Exists(path)) path = "sc.exe";
            if (!File.Exists(path)) return;
            si.FileName = path;
            if (isinstall)
                si.Arguments = String.Format("create XProxy BinPath= \"{0} -s\" start= auto DisplayName= XProxy代理服务器", Application.ExecutablePath);
            else
                si.Arguments = @"Delete XProxy";
            si.UseShellExecute = false;
            si.CreateNoWindow = false;
            p.StartInfo = si;
            p.Start();
            p.WaitForExit();
        }
    }
}