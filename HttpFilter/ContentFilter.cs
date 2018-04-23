using System;
using System.Text;
using System.Text.RegularExpressions;
using XProxy.Base;
using XProxy.Config;
using XProxy.Http;

namespace HttpFilter
{
    /// <summary>
    /// 内容过滤器
    /// </summary>
    public class ContentFilter : HttpPluginBase
    {
        /// <summary>
        /// 处理响应后的内容。
        /// 现在的延迟处理还没有做好，如果做好了延迟处理，就可以直接OnResponse的另一个版本。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public override byte[] OnResponseContent(Session client, byte[] Data)
        {
            //要注意编码。不一定是gb2312编码。V1.0版中有自动识别编码的功能，现在还没有迁移过来。
            String str = Encoding.Default.GetString(Data);
            str = Regex.Replace(str, @"\w+\.baidu\.com",
                Manager.Manager.Listener.Address.ToString() + ":" +
                Manager.Manager.Listener.Config.Port.ToString());
            return Encoding.Default.GetBytes(str);
        }

        public override PluginConfig DefaultConfig
        {
            get
            {
				PluginConfig pc = base.DefaultConfig;
                pc.Name = "内容过滤器";
                pc.Author = "大石头";
                return pc;
            }
        }
    }
}
