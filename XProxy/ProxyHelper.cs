using System;
using System.Collections.Generic;
using System.Linq;
using NewLife.Net.Proxy;
using NewLife.Reflection;

namespace XProxy
{
    public static class ProxyHelper
    {
        private static Type[] _proxyArray;
        /// <summary>获取所有代理类</summary>
        /// <returns></returns>
        public static Type[] GetAll()
        {
            if (_proxyArray == null) _proxyArray = typeof(ProxyBase).GetAllSubclasses(true).ToArray();

            return _proxyArray;
        }
    }
}