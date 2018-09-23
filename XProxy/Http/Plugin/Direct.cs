using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// 直接代理
    /// </summary>
    public class Direct : HttpPluginBase
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
            return HttpHelper.ProcessHttpRequestHeader(session, requestheader);
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
                pc.Name = "直接代理";
                pc.Author = "大石头";
                return pc;
            }
        }
        #endregion
    }
}
