using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NewLife.Net.Proxy;
using NewLife.Reflection;
using XProxy.Proxy;

namespace XProxy
{
    public static class ProxyHelper
    {
        /// <summary>内置代理类型。AOT 下不依赖程序集扫描，保证类型与构造函数被裁剪器保留</summary>
        public static Type[] BuiltinTypes { get; } = [typeof(NATProxy), typeof(HttpProxy), typeof(HttpIpProxy), typeof(HttpReverseProxy)];

        private static Type[] _proxyArray;
        /// <summary>获取所有代理类</summary>
        /// <returns></returns>
        public static Type[] GetAll()
        {
            if (_proxyArray != null) return _proxyArray;

            // AOT 下反射扫描不可用，直接使用内置清单
            if (!RuntimeFeature.IsDynamicCodeSupported) return _proxyArray = BuiltinTypes;

            var list = typeof(ProxyBase).GetAllSubclasses().ToList();
            foreach (var item in BuiltinTypes)
            {
                if (!list.Contains(item)) list.Add(item);
            }

            return _proxyArray = list.ToArray();
        }

        /// <summary>创建代理实例</summary>
        /// <param name="type">代理类型</param>
        /// <returns></returns>
        public static ProxyBase Create(Type type)
        {
            if (type == null) return null;

            if (type == typeof(NATProxy)) return new NATProxy();
            if (type == typeof(HttpReverseProxy)) return new HttpReverseProxy();
            if (type == typeof(HttpIpProxy)) return new HttpIpProxy();
            if (type == typeof(HttpProxy)) return new HttpProxy();

            if (!RuntimeFeature.IsDynamicCodeSupported) return null;

            return type.CreateInstance() as ProxyBase;
        }
    }
}