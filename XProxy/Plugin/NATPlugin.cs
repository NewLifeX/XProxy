using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;

namespace XProxy.Plugin
{
    /// <summary>
    /// NAT插件。端口重定向
    /// </summary>
    public class NATPlugin : PluginBase
    {
        /// <summary>
        /// 默认设置
        /// </summary>
        public override PluginConfig DefaultConfig
        {
            get
            {
                var pc = base.DefaultConfig;
                pc.Author = "大石头";
                pc.Name = "NAT插件";
                return pc;
            }
        }
    }
}