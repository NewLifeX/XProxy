using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using XProxy.Http;
using XProxy.Http.Plugin;
using XProxy.Base;

namespace XProxy
{
    /// <summary>
    /// Http辅助类
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// 分析HTTP头，包括请求头和相应头
        /// </summary>
        /// <param name="bts">HTTP数据包</param>
        /// <returns></returns>
        public static String[] ParseHeader(Byte[] bts)
        {
            var p = ByteHelper.IndexOf(bts, "\r\n\r\n");
            if (p < 0) return null;
            return Encoding.ASCII.GetString(ByteHelper.SubBytes(bts, 0, p)).Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 是否HTTP请求
        /// </summary>
        /// <param name="bts">HTTP数据包</param>
        /// <returns></returns>
        public static Boolean IsHttpRequest(Byte[] bts)
        {
            var key = "GET ";
            if (bts == null || bts.Length < key.Length) return false;
            if (ByteHelper.StartWith(bts, key)) return true;
            if (ByteHelper.StartWith(bts, "POST ")) return true;
            return false;
        }

        /// <summary>
        /// 是否HTTP响应
        /// </summary>
        /// <param name="bts">HTTP数据包</param>
        /// <returns></returns>
        public static Boolean IsHttpResponse(Byte[] bts)
        {
            var key = "HTTP/1.";
            if (bts == null || bts.Length < key.Length) return false;
            if (ByteHelper.StartWith(bts, key)) return true;
            return false;
        }

        /// <summary>
        /// 处理HTTP请求头。供HTTP间接代理使用
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="header">数据</param>
        /// <returns>处理后的数据</returns>
        public static String ProcessHttpRequestHeader(Session session, String header)
        {
            if (String.IsNullOrEmpty(header)) return header;

            // 找到HTTP头，尝试修正请求地址和主机HOST
            var headers = header.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headers == null) return header;

            var uri = GetUriFromHeader(header);
            session.ServerPort = 80;
			session.ServerPort = uri.Port;
            try
            {
                session.ServerAddress = Dns.GetHostEntry(uri.Host).AddressList[0];
            }
            catch (Exception ex)
            {
                session.WriteLog("无法解析主机地址 " + ex.Message);
                return null;
            }

            // 重新拼接HTTP请求头
            var sb = new StringBuilder();
            foreach (var s in headers)
            {
                var ss = s.ToLower();
                if (ss.StartsWith("get "))
                {
                    sb.Append("GET ");
                    sb.Append(uri.PathAndQuery);
                    sb.Append(" HTTP/1.1");
                }
                else if (ss.StartsWith("post "))
                {
                    sb.Append("POST ");
                    sb.Append(uri.PathAndQuery);
                    sb.Append(" HTTP/1.1");
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

        /// <summary>
        /// 从请求头中取得Uri
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Uri GetUriFromHeader(String header)
        {
            if (String.IsNullOrEmpty(header)) return null;

            if (header.IndexOf("\r\n") < 0) return null;
            header = header.Substring(0, header.IndexOf("\r\n"));

            var i = header.IndexOf(" ");
            if (i < 0) return null;
            var j = header.IndexOf(" ", i + 1);
            if (j < 0) return null;
            return new Uri(header.Substring(i + 1, j - i));
        }
    }
}