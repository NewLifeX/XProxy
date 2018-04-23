using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Plugin;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Http
{
	/// <summary>
	/// Http插件接口
	/// </summary>
	public interface IHttpPlugin : IDisposable
	{
		#region 初始化
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="manager">插件管理器</param>
		void OnInit(HttpPlugin manager);
		#endregion

		#region 请求头/响应头 处理
		///// <summary>
		///// 请求头
		///// </summary>
		///// <param name="session">客户端</param>
		///// <param name="requestheader">请求头</param>
		///// <returns>处理后的请求头</returns>
		//String OnRequestHeader(Session session, String requestheader);

		///// <summary>
		///// 响应头
		///// </summary>
		///// <param name="session">客户端</param>
		///// <param name="responseheader">响应头</param>
		///// <returns>处理后的响应头</returns>
		//String OnResponseHeader(Session session, String responseheader);

		/// <summary>
		/// 请求头
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>处理后的请求头数据</returns>
		Byte[] OnRequestHeader(Session session, Byte[] Data);

		/// <summary>
		/// 响应头
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>处理后的响应头数据</returns>
		Byte[] OnResponseHeader(Session session, Byte[] Data);
		#endregion

		#region 请求/响应 处理，需要启用延迟
		///// <summary>
		///// 请求时触发。不包括头部。需要启用延迟。
		///// </summary>
		///// <param name="session">客户端</param>
		///// <param name="request">请求</param>
		///// <returns>处理后的请求</returns>
		//String OnRequestContent(Session session, String request);

		///// <summary>
		///// 响应时触发。不包括头部。需要启用延迟。
		///// </summary>
		///// <param name="session">客户端</param>
		///// <param name="response">响应</param>
		///// <returns>处理后的响应</returns>
		//String OnResponseContent(Session session, String response);
		#endregion

		#region 请求/响应 原始数据处理
		/// <summary>
		/// 请求时触发。不包括头部。不需要启用延迟。
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>处理后的数据</returns>
		Byte[] OnRequestContent(Session session, Byte[] Data);

		/// <summary>
		/// 响应时触发。不包括头部。不需要启用延迟。
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="Data">数据</param>
		/// <returns>处理后的数据</returns>
		Byte[] OnResponseContent(Session session, Byte[] Data);
		#endregion

		#region 属性
		/// <summary>
		/// 当前设置
		/// </summary>
		PluginConfig Config { get; set; }

		/// <summary>
		/// 默认设置
		/// </summary>
		PluginConfig DefaultConfig { get; }

		/// <summary>
		/// 写日志
		/// </summary>
		event WriteLogDelegate OnWriteLog;
		#endregion
	}
}