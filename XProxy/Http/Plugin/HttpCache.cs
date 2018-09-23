using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
	/// <summary>
	/// 缓存插件
	/// </summary>
	public class HttpCache : HttpPluginBase
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
			//判断是否存在过滤项
			if (Include == null || Include.Length < 1) return requestheader;

			//取得请求Url
			var uri = HttpHelper.GetUriFromHeader(requestheader);
			if (uri == null) return requestheader;

			//过滤检查
			foreach (var item in Include)
			{
				if (uri.OriginalString.Contains(item))
				{
					//检查例外
					if (Exclude != null && Exclude.Length > 0)
					{
						var isexclude = false;
						foreach (var elm in Exclude)
						{
							if (uri.OriginalString.Contains(elm))
							{
								isexclude = true;
								break;
							}
						}
						if (isexclude) continue;
					}

					//缓存处理
					ProcessCache(session, uri);

					//禁止往下
					return null;
				}
			}

			return requestheader;
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
				pc.Name = "Http缓存";
				pc.Author = "大石头";
				return pc;
			}
		}
		#endregion

		#region 属性
		/// <summary>
		/// 过滤Url数组。使用Extend，分号隔开
		/// </summary>
		private String[] Include
		{
			get
			{
				if (Config == null || String.IsNullOrEmpty(Config.Extend)) return null;

				return Config.Extend.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		/// <summary>
		/// 例外Url数组。使用Extend2，分号隔开
		/// </summary>
		private String[] Exclude
		{
			get
			{
				if (Config == null || String.IsNullOrEmpty(Config.Extend2)) return null;

				return Config.Extend2.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}
		#endregion

		#region 扩展
		/// <summary>
		/// 处理缓存
		/// </summary>
		/// <param name="session"></param>
		/// <param name="uri"></param>
		private void ProcessCache(Session session, Uri uri)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HttpCache");
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			var filename = uri.AbsolutePath;
			filename = Path.Combine(path, filename);
			filename = Path.GetFullPath(filename);

			//缓存是否存在
			if (!File.Exists(filename))
			{
				WriteLog("无法找到" + filename + "的缓存，准备下载！");
				//没找到，下载
				if (!Directory.Exists(Path.GetDirectoryName(filename))) 
					Directory.CreateDirectory(Path.GetDirectoryName(filename));
				var wc = new WebClient();
				try
				{
					wc.DownloadFile(uri, filename);
					var header = wc.ResponseHeaders.ToString();
				}
				catch (Exception ex)
				{
					WriteLog("下载文件 " + uri.ToString() + " 出错！" + ex.ToString());
				}
			}

			//读取缓存
			Byte[] bts;
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				fs.Position = 0;
				bts = new Byte[fs.Length];
				fs.Read(bts, 0, bts.Length);
			}
			Write(session, uri, bts);
		}

		private void Write(Session session, Uri uri, Byte[] Data)
		{
			var sb = new StringBuilder();
			sb.AppendLine("HTTP/1.1 200 OK");
			sb.AppendFormat("Content-Length: {0}\r\n", Data.Length);
			var ctype = ContentType.Other;
			var ext = Path.GetExtension(uri.LocalPath).ToLower();
			switch (ext)
			{
				case ".xml":
					ctype = ContentType.Xml;
					break;
				case ".inf":
					ctype = ContentType.Inf;
					break;
				case ".zip":
					ctype = ContentType.Zip;
					break;
				case ".ini":
					ctype = ContentType.Ini;
					break;
				case ".rp":
				case ".zip1":
					ctype = ContentType.Rp;
					break;
			}
			sb.AppendFormat("Content-Type: {0}\r\n", ctype);
			sb.AppendLine("Last-Modified: Tue, 22 Jan 2008 11:26:53 GMT");
			sb.AppendLine("Accept-Ranges: bytes");
			sb.AppendLine("ETag: W/\"3415b0e95cc81:53b\"");
			sb.AppendLine("Server: Microsoft-IIS/6.0");
			sb.AppendLine("X-Powered-By: ASP.NET");
			sb.AppendLine("Date: Tue, 22 Jan 2008 14:11:14 GMT");
			sb.AppendLine("X-Cache: MISS from CNC-TJ-167-83.fastcdn.com");
			sb.AppendLine("Age: 983");
			sb.AppendLine("X-Cache: HIT from CNC-HBCZ-2-73.fastcdn.com");
			sb.AppendLine("Connection: keep-alive");
			sb.AppendLine("");
			//sb.AppendLine("");
			//回发数据
			Data = ByteHelper.Cat(Encoding.ASCII.GetBytes(sb.ToString()), Data);
			//session.SendToClient(Encoding.ASCII.GetBytes(sb.ToString()));
			//session.SendToClient(Data);
		}
		#endregion
	}

	internal static class ContentType
	{
		public static String Zip = "application/x-zip-compressed";
		public static String Xml = "text/xml";
		public static String Inf = "application/octet-stream";
		public static String Ini = "application/all";
		public static String Rp = "application/all";
		public static String Dat = "application/octet-stream";
		public static String Dll = "application/octet-stream";
		public static String Exe = "application/octet-stream";
		public static String Other = "application/octet-stream";
	}
}