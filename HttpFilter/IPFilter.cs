using System;
using XProxy.Base;
using XProxy.Config;
using XProxy.Plugin;

namespace HttpFilter
{
    /// <summary>
    /// IP过滤器。禁止某些IP访问。
    /// 本例子演示了禁止127.0.0.1访问，把该插件放在反向代理插件之前，即可禁止127.0.0.1使用反向代理。
    /// </summary>
    public class IPFilter : PluginBase
    {
        public override Boolean OnClientStart(Session client)
        {
            var ip = client.IPAndPort;
            if (ip.IndexOf(":") > 0) ip = ip.Substring(0, ip.IndexOf(":"));
            return ip != "127.0.0.1";
        }

        public override PluginConfig DefaultConfig
        {
            get
            {
				var pc = base.DefaultConfig;
                pc.Name = "IP过滤器";
                pc.Author = "大石头";
                return pc;
            }
        }
    }
}