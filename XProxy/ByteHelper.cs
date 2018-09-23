using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy
{
    /// <summary>
    /// 字节辅助类
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// 连接两个字节数组成为一个
        /// </summary>
        /// <param name="buf1">字节数组1</param>
        /// <param name="buf2">字节数组2</param>
        /// <returns></returns>
        public static Byte[] Cat(Byte[] buf1, Byte[] buf2)
        {
			//if (buf1 == null || buf1.Length <= 0) return buf2;
			//if (buf2 == null || buf2.Length <= 0) return buf1;
			if (buf1 == null || buf1.Length <= 0) return Copy(buf2);
			if (buf2 == null || buf2.Length <= 0) return Copy(buf1);

            var buf = new Byte[buf1.Length + buf2.Length];
			//for (int i = 0; i < buf1.Length; i++)
			//    buf[i] = buf1[i];
			//for (int i = 0; i < buf2.Length; i++)
			//    buf[buf1.Length + i] = buf2[i];
			Buffer.BlockCopy(buf1, 0, buf, 0, buf1.Length);
			Buffer.BlockCopy(buf2, 0, buf, buf1.Length, buf2.Length);
            return buf;
        }

        /// <summary>
        /// 截取字节数组的某一部分，成为新的字节数组
        /// </summary>
        /// <param name="buf">要截取的字节数组</param>
        /// <param name="index">开始位置</param>
        /// <param name="count">字节个数</param>
        /// <returns></returns>
        public static Byte[] SubBytes(Byte[] buf, Int32 index, Int32 count)
        {
            if (buf == null || buf.Length < index) return null;

            if (count < 1 || count > buf.Length - index) count = buf.Length - index;

			//直接复制
			if (index == 0 && count == buf.Length) return Copy(buf);

            var b = new Byte[count];
			//for (int i = 0; i < count; i++)
			//    b[i] = buf[index + i];
			Buffer.BlockCopy(buf, index, b, 0, b.Length);
            return b;
        }

		/// <summary>
		/// 复制
		/// </summary>
		/// <param name="buf"></param>
		/// <returns></returns>
		public static Byte[] Copy(Byte[] buf)
		{
			if (buf == null || buf.Length < 1) return null;

			var bts = new Byte[buf.Length];
			Buffer.BlockCopy(buf, 0, bts, 0, bts.Length);
			return bts;
		}

        /// <summary>
        /// 字节数组是否以另一指定的字符串开始
        /// </summary>
        /// <param name="bts">字节数组</param>
        /// <param name="str">要查找的字符串</param>
        /// <returns></returns>
        public static Boolean StartWith(Byte[] bts, String str)
        {
            if (String.IsNullOrEmpty(str)) return false;
            return StartWith(bts, Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// 字节数组是否以另一指定的字节数组开始
        /// </summary>
        /// <param name="bts1">字节数组</param>
        /// <param name="bts2">另一指定的字节数组</param>
        /// <returns></returns>
        public static Boolean StartWith(Byte[] bts1, Byte[] bts2)
        {
            if (bts1 == null || bts2 == null) return false;
            if (bts1.Length < bts2.Length) return false;
            for (var i = 0; i < bts2.Length; i++)
            {
                if (bts1[i] != bts2[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 在字节数组中查找指定字符串的位置
        /// </summary>
        /// <param name="bts">字节数组</param>
        /// <param name="str">要查找的字符串</param>
        /// <returns></returns>
        public static Int32 IndexOf(Byte[] bts, String str)
        {
            if (String.IsNullOrEmpty(str)) return -1;
            return IndexOf(bts, Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// 在字节数组中查找另一字节数组的位置
        /// </summary>
        /// <param name="bts1">字节数组</param>
        /// <param name="bts2">另一字节数组</param>
        /// <returns>成功返回位置，失败返回-1</returns>
        public static Int32 IndexOf(Byte[] bts1, Byte[] bts2)
        {
            if (bts1 == null || bts2 == null) return -1;

            for (Int32 i = 0, j = 0; i < bts1.Length; i++, j++)
            {
                if (bts1[i] != bts2[j]) // 找到一个不等，把j复位
                    j = -1;
                else if (j == bts2.Length - 1) // bts2比较完毕，表示找到，返回结果
                    return i - j;
            }
            return -1;
        }

        /// <summary>
        /// 查找字节数组中是否包含指定字符串
        /// </summary>
        /// <param name="bts">字节数组</param>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        public static Boolean Contains(Byte[] bts, String str)
        {
            if (String.IsNullOrEmpty(str)) return false;
            return Contains(bts, Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// 查找字节数组中是否包含指定另一字节数组
        /// </summary>
        /// <param name="bts1">字节数组</param>
        /// <param name="bts2">另一字节数组</param>
        /// <returns></returns>
        public static Boolean Contains(Byte[] bts1, Byte[] bts2)
        {
            return IndexOf(bts1, bts2) >= 0;
        }

        /// <summary>
        /// 把字节数组的指定区域替换为指定字符串
        /// </summary>
        /// <param name="bts">字节数组</param>
        /// <param name="count">字节数组总字节数</param>
        /// <param name="start">开始位置</param>
        /// <param name="length">区域长度，0表示直接插入而不是替换</param>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static Int32 Replace(ref Byte[] bts, Int32 count, Int32 start, Int32 length, String str)
        {
            if (String.IsNullOrEmpty(str))
                return Replace(ref bts, count, start, length, new Byte[0]);
            else
                return Replace(ref bts, count, start, length, Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// 把字节数组的指定区域替换为另一指定字节数组
        /// </summary>
        /// <param name="bts">字节数组</param>
        /// <param name="count">字节数组总字节数</param>
        /// <param name="start">开始位置</param>
        /// <param name="length">区域长度，0表示直接插入而不是替换</param>
        /// <param name="bts2">另一指定字节数组</param>
        /// <returns></returns>
        public static Int32 Replace(ref Byte[] bts, Int32 count, Int32 start, Int32 length, Byte[] bts2)
        {
            if (bts == null || start < 0 || length < 0 || start + length > count - 1) return count;

            // 另一指定字节数组为空，表明要直接把后面的字节数据前移
            if (bts2 == null || bts2.Length == 0)
            {
                //前面空间过大，前移
                for (var i = start; i < count; i++) bts[i] = bts[length + i];
                return count - length;
            }

            var off = 0;
            //为新字节数组挪出位置来
            if (length < bts2.Length)
            {
                //前面空间不够，后移。注意：后移需要从右往左移
                off = bts2.Length - length;
                for (var i = count - 1; i >= start + length; i--) bts[off + i] = bts[i];
            }
            else if (length > bts2.Length)
            {
                //前面空间过大，前移。注意：前移需要从左往右移
                off = length - bts2.Length;
                for (var i = start + length - off; i < count; i++) bts[i] = bts[off + i];
            }
            //如果另一指定字节数组刚好能放进指定区域，就不用移动后面部分了
            for (var i = 0; i < bts2.Length; i++) bts[start + i] = bts2[i];

            return count + off;
        }

        /// <summary>
        /// 加密解密
        /// </summary>
        /// <param name="Data">数据</param>
        /// <param name="Count">字节个数</param>
        /// <param name="key">密码</param>
        /// <returns>加密后数据个数</returns>
        public static Int32 Encrypt(ref Byte[] Data, Int32 Count, String key)
        {
            var bts = Encoding.ASCII.GetBytes(key);
            for (Int32 i = 0, j = 0; i < Count; i++, j++)
            {
                if (j >= bts.Length) j = 0;
                Data[i] ^= bts[j];
            }
            return Count;
        }
    }
}