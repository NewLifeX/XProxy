using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using NewLife.Log;

namespace XProxy.Config
{
    /// <summary>
    /// 代理配置
    /// </summary>
    [Serializable]
    public class ProxyConfig
    {
        #region 属性
        /// <summary>
        /// 监听器集合
        /// </summary>
        public ListenerConfig[] Listeners { get; set; }
        #endregion

        #region 构造函数
        //public static List<ListenerConfig> list = new List<ListenerConfig>();
        //public ProxyConfig()
        //{
        //    list.Add(this);
        //}

        private static ProxyConfig _Instance;
        /// <summary>
        /// 默认实例
        /// </summary>
        public static ProxyConfig Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Load(null);
                    //if (_Instance != null) _Instance.IsSaved = false;
                }
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        ///// <summary>
        ///// BindSource默认会初始化一个Config类，然后才调用GetList方法。
        ///// 所以，应该是只有调用了Load的那个方法才能Save。
        ///// </summary>
        //[NonSerialized]
        //private bool IsSaved = true;

        /// <summary>
        /// 析构时保存
        /// </summary>
        ~ProxyConfig()
        {
            //if (!IsSaved)
            //{
            //    Save(null, this);
            //    IsSaved = true;
            //}
        }
        #endregion

        #region 加载保存
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns></returns>
		public static ProxyConfig Load(String filename)
		{
			if (String.IsNullOrEmpty(filename)) filename = DefaultFile;
			var config = new ProxyConfig();
			if (File.Exists(filename))
			{
				using (var sr = new StreamReader(filename, Encoding.UTF8))
				{
					var xs = new XmlSerializer(typeof(ProxyConfig));
					try
					{
						config = xs.Deserialize(sr) as ProxyConfig;
					}
					catch (Exception ex)
					{
						XTrace.WriteLine("加载代理配置文件" + filename + "时发生错误！\n" + ex.ToString());
					}
					sr.Close();
				}
			}
			return config;
		}

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="config">要保存的对象</param>
        public static void Save(String filename, ProxyConfig config)
        {
            if (config == null) return;
            if (String.IsNullOrEmpty(filename)) filename = DefaultFile;
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                try
                {
                    var xs = new XmlSerializer(typeof(ProxyConfig));
                    xs.Serialize(sw, config);
                }
				catch (Exception ex)
				{
					XTrace.WriteLine("保存代理配置文件" + filename + "时发生错误！\n" + ex.ToString());
				}
				sw.Close();
            }
        }

        /// <summary>
        /// 默认配置文件
        /// </summary>
        public static String DefaultFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Proxy.xml");
        #endregion
    }
}
