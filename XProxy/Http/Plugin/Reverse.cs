using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Reflection;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// 反向代理
    /// </summary>
    public class Reverse : HttpPluginBase
    {
        #region IHttpPlugin 成员
        #region 请求头/响应头 处理
        /// <summary>
        /// 请求头
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="requestheader">请求头</param>
        /// <returns>处理后的请求头</returns>
        public override String OnRequestHeader(Session session, String requestheader)
        {
            return ProcessHttpRequestHeader(requestheader);
        }
        #endregion

        /// <summary>
        /// 默认设置
        /// </summary>
        public override PluginConfig DefaultConfig
        {
            get
            {
                var pc = base.DefaultConfig;
                pc.Name = "反向代理";
                pc.Author = "大石头";
                return pc;
            }
        }
        #endregion

        #region 扩展
        /// <summary>
        /// 处理HTTP请求头。供HTTP代理使用
        /// </summary>
        /// <param name="header">数据</param>
        /// <returns>处理后的数据</returns>
        public String ProcessHttpRequestHeader(String header)
        {
            if (String.IsNullOrEmpty(header)) return header;

            // 找到HTTP头，尝试修正请求地址和主机HOST
            var headers = header.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headers == null) return header;

            // 重新拼接HTTP请求头
            var sb = new StringBuilder();
            foreach (var s in headers)
            {
                var ss = s.ToLower();
                if (ss.StartsWith("host:"))
                {
                    sb.Append("Host: ");
                    sb.Append(Manager.Manager.Listener.Config.ServerAddress);
                }
                else
                {
                    sb.Append(s);
                }
                sb.Append("\r\n");
            }
            // 加上自己的标识
            sb.Append("X-Proxy: NewLifeXProxy");

            return sb.ToString();
        }
        #endregion
    }
}