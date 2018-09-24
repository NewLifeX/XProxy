using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using NewLife.Net.Proxy;
using NewLife.Xml;

namespace XProxy
{
    /// <summary>配置</summary>
    [XmlConfigFile(@"Config\XProxy.config", 10000)]
    class Setting : XmlConfig<Setting>
    {
        #region 属性
        /// <summary>调试开关。默认true</summary>
        [Description("调试开关。默认true")]
        public Boolean Debug { get; set; } = true;

        /// <summary>配置项</summary>
        [Description("配置项")]
        public ProxyItem[] Items { get; set; }
        #endregion

        #region 方法
        private static Type[] _proxyArray;
        /// <summary>已加载</summary>
        protected override void OnLoaded()
        {
            // 本进程的第一次加载时，补齐缺失项
            if (_proxyArray == null)
            {
                var list = new List<ProxyItem>();

                var ms = Items;
                if (ms != null && ms.Length > 0) list.AddRange(ms);

                var demo = list.Count == 0;

                var arr = ProxyHelper.GetAll();
                foreach (var item in arr)
                {
                    if (!item.IsAbstract && item.GetGenericArguments().Length == 0)
                    {
                        var name = item.Name.TrimEnd("Proxy");
                        // 如果没有该类型的代理项，则增加一个
                        if (!list.Any(e => e.Provider == name))
                        {
                            var pi = new ProxyItem
                            {
                                Name = name + "_Demo",
                                Provider = name,
                            };

                            if (item == typeof(NATProxy))
                            {
                                pi.Local = "tcp://0.0.0.0:3388";
                                pi.Remote = "tcp://localhost:3389";
                            }
                            else if (item == typeof(HttpReverseProxy))
                            {
                                pi.Local = "tcp://0.0.0.0:8081";
                                pi.Remote = "tcp://www.newlifex.com:80";
                            }
                            else if (item == typeof(HttpProxy))
                            {
                                pi.Local = "tcp://0.0.0.0:1080";
                                pi.Remote = "";
                            }

                            // 演示模式时，默认开启所有代理
                            if (demo) pi.Enable = true;

                            list.Add(pi);
                        }
                    }
                }

                _proxyArray = arr;

                Items = list.ToArray();
            }

            base.OnLoaded();
        }

        /// <summary>获取</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProxyItem Get(String name) => Items.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));

        /// <summary>获取或添加</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProxyItem GetOrAdd(String name)
        {
            if (name.IsNullOrEmpty()) return null;

            var mi = Items.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));
            if (mi != null) return mi;

            lock (this)
            {
                var list = new List<ProxyItem>(Items);
                mi = list.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));
                if (mi != null) return mi;

                mi = new ProxyItem { Name = name };
                list.Add(mi);

                Items = list.ToArray();

                return mi;
            }
        }
        #endregion
    }

    /// <summary>代理配置项</summary>
    public class ProxyItem
    {
        /// <summary>名称</summary>
        [XmlAttribute]
        public String Name { get; set; }

        /// <summary>代理类型</summary>
        [XmlAttribute]
        public String Provider { get; set; }

        /// <summary>启用</summary>
        [XmlAttribute]
        public Boolean Enable { get; set; }

        /// <summary>本地监听地址和端口</summary>
        [XmlAttribute]
        public String Local { get; set; } = "tcp://0.0.0.0:8080";

        /// <summary>远程地址和端口</summary>
        [XmlAttribute]
        public String Remote { get; set; } = "tcp://www.newlifex.com:80";

        /// <summary>配置字符串</summary>
        [XmlAttribute]
        public String Value { get; set; }
    }
}