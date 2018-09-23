using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NewLife.Log;

namespace XProxy
{
    /// <summary>
    /// 网络助手
    /// </summary>
    public static class NetHelper
    {
        /// <summary>
        /// 设置超时检测时间和检测间隔
        /// </summary>
        /// <param name="socket">要设置的Socket对象</param>
        /// <param name="iskeepalive">是否启用Keep-Alive</param>
        /// <param name="starttime">多长时间后开始第一次探测（单位：毫秒）</param>
        /// <param name="interval">探测时间间隔（单位：毫秒）</param>
        public static void SetKeepAlive(Socket socket,Boolean iskeepalive, Int32 starttime, Int32 interval)
        {
            UInt32 dummy = 0;
            var inOptionValues = new Byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((UInt32)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((UInt32)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((UInt32)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        //struct tcp_keepalive
        //{
        //    u_long onoff; //是否启用Keep-Alive
        //    u_long keepalivetime; //多长时间后开始第一次探测（单位：毫秒）
        //    u_long keepaliveinterval; //探测时间间隔（单位：毫秒）
        //};

        /// <summary>
        /// 输出堆栈
        /// </summary>
        public static void OutStack()
        {
            var st = new StackTrace(1);
            var sb = new StringBuilder();
            var sfs = st.GetFrames();
            foreach (var sf in sfs)
            {
                sb.Append(sf.GetMethod().DeclaringType.FullName);
                sb.Append(".");
                sb.Append(sf.GetMethod().Name);
                var s = sf.GetFileName();
                if (!String.IsNullOrEmpty(s))
                {
                    sb.Append("(");
                    sb.Append(s);
                    sb.Append(",");
                    sb.Append(sf.GetFileLineNumber().ToString());
                    sb.Append("行)");
                }
                sb.Append(Environment.NewLine);
            }
            XTrace.WriteLine(sb.ToString());
        }
    }
}