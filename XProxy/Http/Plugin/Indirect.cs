using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using XProxy.Config;
using System.Net;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// 间接代理
    /// </summary>
    public class Indirect : HttpPluginBase
    {
        #region 属性
        /// <summary>
        /// 为头部加上内容，防止被识别。
        /// </summary>
        public static String Key = "Get a blow! Love XinXin forever!";
        #endregion

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
            if (!IsLocal(session)) requestheader = HttpHelper.ProcessHttpRequestHeader(session, requestheader);
            return requestheader;
        }

        ///// <summary>
        ///// 响应头
        ///// </summary>
        ///// <param name="session">客户端</param>
        ///// <param name="responseheader">响应头</param>
        ///// <returns>处理后的响应头</returns>
        //public override string OnResponseHeader(Session session, string responseheader)
        //{
        //    if (!IsLocal(session)) return Indirect.Key + responseheader;

        //    if (responseheader.StartsWith(Indirect.Key))
        //        responseheader = responseheader.Substring(Indirect.Key.Length);
        //    return responseheader;
        //}

        /// <summary>
        /// 请求头
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的请求头数据</returns>
        public override Byte[] OnRequestHeader(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// 响应头
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的响应头数据</returns>
        public override Byte[] OnResponseHeader(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// 请求时触发。不包括头部。不需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的数据</returns>
        public override Byte[] OnRequestContent(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// 响应时触发。不包括头部。不需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的数据</returns>
		public override Byte[] OnResponseContent(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        private static Byte[] Encrypt(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] ^= (Byte)('X');
            }
            return Data;
        }

        private static List<String> _LocalIPs;
        /// <summary>
        /// 本地IP集合
        /// </summary>
        protected static List<String> LocalIPs
        {
            get
            {
                if (_LocalIPs == null)
                {
                    var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    var list = new List<IPAddress>(ips);
                    list.Add(IPAddress.Any);
                    list.Add(IPAddress.Loopback);
                    _LocalIPs = new List<String>();
                    foreach (var ip in list)
                    {
                        _LocalIPs.Add(ip.ToString());
                    }
                }
                return _LocalIPs;
            }
        }

        /// <summary>
        /// 是否本机IP。该方法用于判断间接代理是工作在客户端还是服务端。
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private Boolean IsLocal(Session session)
        {
            var ip = session.IPAndPort;
            if (ip.IndexOf(":") > 0) ip = ip.Substring(0, ip.IndexOf(":"));
            return LocalIPs.Contains(ip);
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
                pc.Name = "间接代理";
                pc.Author = "大石头";
                return pc;
            }
        }
        #endregion
    }
}
