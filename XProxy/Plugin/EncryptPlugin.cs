using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Plugin
{
    /// <summary>
    /// 加密插件。端口重定向的过程中，对数据进行加解密
    /// </summary>
    public class EncryptPlugin : PluginBase
    {
        /// <summary>
        /// 客户端向服务器发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        public override Byte[] OnClientToServer(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// 服务器相客户端发数据时触发。
        /// </summary>
        /// <param name="session">客户端</param>
        /// <param name="Data">数据</param>
        /// <returns>经过处理后的数据</returns>
        public override Byte[] OnServerToClient(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        private Byte[] Encrypt(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;
            for (var i = 0; i < Data.Length; i++)
            {
				Data[i] ^= EncryptKey;
            }
            return Data;
        }

        /// <summary>
        /// 默认设置
        /// </summary>
        public override PluginConfig DefaultConfig
        {
            get
            {
                var pc = base.DefaultConfig;
                pc.Author = "大石头";
                pc.Name = "加密插件";
                return pc;
            }
        }

		/// <summary>
		/// 密钥。目前只能是单个字符，
		/// 因为数据包在传输的过程中可能会被重新组合，
		/// 从而不能使用块加密
		/// </summary>
		private Byte EncryptKey
		{
			get
			{
				if (Config == null || String.IsNullOrEmpty(Config.Extend)) return (Byte)'X';
				return (Byte)Config.Extend[0];
			}
		}
    }
}