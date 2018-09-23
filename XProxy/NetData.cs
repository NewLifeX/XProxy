using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy
{
	/// <summary>
	/// 网络数据
	/// </summary>
	public class NetData
	{
		/// <summary>
		/// 记录时间
		/// </summary>
		public DateTime RecordTime = DateTime.Now;
		/// <summary>
		/// 数据
		/// </summary>
		public Byte[] Buf;
		/// <summary>
		/// 实例化一个网络数据对象
		/// </summary>
		/// <param name="buffer"></param>
		public NetData(Byte[] buffer)
			: this(buffer, 0, buffer.Length)
		{
		}
		/// <summary>
		/// 实例化一个网络数据对象
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
        public NetData(Byte[] buffer, Int32 index, Int32 count)
        {
            if (buffer == null || buffer.Length < 1) return;

            Buf = new Byte[count];
            Buffer.BlockCopy(buffer, 0, Buf, index, count);
        }

		/// <summary>
		/// 连接
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static Byte[] Join(List<NetData> list)
		{
			if (list == null || list.Count < 1) return null;

			var count = 0;
			for (var i = 0; i < list.Count; i++)
			{
				count += list[i].Buf.Length;
			}

			if (count < 1) return null;

			//开始连接。不能调用ByteHelper，那样会不断分配内存
			var bts = new Byte[count];
			var pos = 0;
			for (var i = 0; i < list.Count; i++)
			{
				Buffer.BlockCopy(list[i].Buf, 0, bts, pos, list[i].Buf.Length);
				pos += list[i].Buf.Length;
			}
			return bts;
		}
	}
}