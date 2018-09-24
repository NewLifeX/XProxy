using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Reflection;
using XProxy.Base;

namespace XProxy.Http
{
    /// <summary>
    /// Http插件基类
    /// </summary>
    public abstract class HttpPluginBase : IHttpPlugin
    {
        #region 属性
        private HttpPlugin _Manager;
        /// <summary>
        /// Http插件管理器
        /// </summary>
        public virtual HttpPlugin Manager { get { return _Manager; } set { _Manager = value; } }
        /// <summary>
        /// 当前设置
        /// </summary>
        public PluginConfig Config { get; set; }

        /// <summary>
        /// 默认设置
        /// </summary>
        public virtual PluginConfig DefaultConfig
		{
			get
			{
				var pc = new PluginConfig();
				//pc.Name = "Http插件";
				pc.Name = GetType().Name;
				pc.Author = "无名";
				pc.ClassName = GetType().FullName;
				//如果是内部插件，则只显示类名，而不显示全名
				if (Assembly.GetExecutingAssembly() == GetType().Assembly)
					pc.ClassName = GetType().Name;
				pc.Version = GetType().Assembly.GetName().Version.ToString();
				pc.Path = System.IO.Path.GetFileName(GetType().Assembly.Location);
				return pc;
			}
		}

        /// <summary>
        /// 写日志事件
        /// </summary>
        public virtual event WriteLogDelegate OnWriteLog;
        #endregion

        #region IHttpPlugin 成员
		#region 初始化
		/// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">插件管理器</param>
        public virtual void OnInit(HttpPlugin manager)
        {
            Manager = manager;
		}
		#endregion

		#region 请求头/响应头 处理
        /// <summary>
        /// 请求头
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的请求头数据</returns>
        public virtual Byte[] OnRequestHeader(Session session, Byte[] Data)
        {
			var header = Encoding.ASCII.GetString(Data);
			header = OnRequestHeader(session, header);
			return Encoding.ASCII.GetBytes(header);
        }

        /// <summary>
        /// 响应头
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的响应头数据</returns>
        public virtual Byte[] OnResponseHeader(Session session, Byte[] Data)
        {
			var header = Encoding.ASCII.GetString(Data);
			header = OnResponseHeader(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

		/// <summary>
		/// 请求头
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="requestheader">请求头</param>
		/// <returns>处理后的请求头</returns>
		public virtual String OnRequestHeader(Session session, String requestheader)
		{
			return requestheader;
		}

		/// <summary>
		/// 响应头
		/// </summary>
		/// <param name="session">客户端</param>
		/// <param name="responseheader">响应头</param>
		/// <returns>处理后的响应头</returns>
		public virtual String OnResponseHeader(Session session, String responseheader)
		{
			return responseheader;
		}
		#endregion

        #region 请求/响应 处理，需要启用延迟，不包括头部
        /// <summary>
        /// 请求时触发。不包括头部。需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="request">请求</param>
        /// <returns>处理后的请求</returns>
        public virtual String OnRequestBody(Session session, String request)
        {
            return request;
        }

        /// <summary>
        /// 响应时触发。不包括头部。需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="response">响应</param>
        /// <returns>处理后的响应</returns>
        public virtual String OnResponseBody(Session session, String response)
        {
            return response;
        }
        #endregion

        #region 请求/响应 原始数据处理，不包括头部
        /// <summary>
        /// 请求时触发。不包括头部。不需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的数据</returns>
        public virtual Byte[] OnRequestContent(Session session, Byte[] Data)
        {
			var header = Encoding.ASCII.GetString(Data);
			header = OnRequestBody(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

        /// <summary>
        /// 响应时触发。不包括头部。不需要启用延迟。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>处理后的数据</returns>
        public virtual Byte[] OnResponseContent(Session session, Byte[] Data)
        {
			//String header = Encoding.ASCII.GetString(Data);
			//header = OnResponseBody(session, header);
			//return Encoding.ASCII.GetBytes(header);
			return Data;
		}
        #endregion
        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
        }
        #endregion

        #region 日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg">日志</param>
        public virtual void WriteLog(String msg)
        {
            if (OnWriteLog != null)
            {
                OnWriteLog(msg);
            }
        }
        #endregion

		#region 重载
		/// <summary>
        /// 已重载。
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return Config == null ? base.ToString() : Config.ToString();
		}
		#endregion
	}
}