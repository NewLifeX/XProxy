using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace XProxy.Base
{
    /// <summary>
    /// 网络连接。
    /// </summary>
    public class Connection : IDisposable
    {
        #region 属性
        #region 基本属性
        /// <summary>
        /// Tcp连接
        /// </summary>
        public TcpClient Tcp;

        /// <summary>
        /// 网络流 
        /// </summary>
        private NetworkStream Stream
        {
            get
            {
                return Tcp == null ? null : Tcp.GetStream();
            }
        }

        /// <summary>
        /// 缓存数据
        /// </summary>
        private Byte[] Buffer = new Byte[1024 * 8];
        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encode { get; set; }

        private String _Name;
        /// <summary>
        /// 连接名
        /// </summary>
        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        /// <summary>
        /// 会话
        /// </summary>
        public Session Session { get; set; }
        #endregion

        #region 便捷属性
        /// <summary>
        /// 基础Socket
        /// </summary>
        private Socket Socket
        {
            get { return Tcp == null ? null : Tcp.Client; }
        }

        /// <summary>
        /// 网络流是否可用
        /// </summary>
        public Boolean Active
        {
            get
            {
                return Socket != null && Socket.Connected;
            }
        }
        #endregion
        #endregion

        #region 事件
        /// <summary>
        /// 异步读取数据完成时触发
        /// </summary>
        public event ReadCompleteDelegate OnRead;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tcpclient"></param>
        public Connection(TcpClient tcpclient)
        {
            Tcp = tcpclient;
            Tcp.SendTimeout = 5000;
            Tcp.ReceiveTimeout = 5000;
        }
        #endregion

        #region IDisposable 成员
        private Boolean IsDisposed = false;
        /// <summary>
        /// 销毁网络连接占用的资源
        /// </summary>
        public void Dispose()
        {
            //双锁检查，防止多线程冲突
            if (IsDisposed) return;
            lock (this)
            {
                if (IsDisposed) return;
                IsDisposed = true;
                try
                {
                    if (Stream != null) Stream.Close();
                    if (Tcp != null) Tcp.Close();
                    if (Buffer != null) Buffer = null;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("关闭连接出错！" + ex.Message);
                }

                Session.Dispose();
            }
        }

        private void End(String msg)
        {
            WriteLog(msg);
            Dispose();
        }

        private void End(String format, params Object[] args)
        {
            End(String.Format(format, args));
        }

        private void End(String action, Exception ex)
        {
            End("{0}操作出错！{1}", action, ex.Message);
        }

        /// <summary>
        /// 终止处理
        /// </summary>
        /// <param name="ex"></param>
        private void End(Exception ex)
        {
            //取得调用本函数的方法名
            var st = new StackTrace(1, true);
            var sf = st.GetFrame(0);
            End(String.Format("{0}->{1}", sf.GetMethod().DeclaringType.FullName, sf.GetMethod().ToString()), ex);
        }
        #endregion

        #region 数据交换过程
        #region 异步读
        /// <summary>
        /// 开始异步读取
        /// </summary>
        public void BeginRead()
        {
            if (!Active) return;

            try
            {
                var iar = Stream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(EndRead), null);
            }
            catch (Exception ex)
            {
                End(ex);
                return;
            }
        }

        /// <summary>
        /// 处理异步读取的结束
        /// </summary>
        /// <param name="ar"></param>
        private void EndRead(IAsyncResult ar)
        {
            var Ret = 0;
            if (!Active) return;

            try
            {
                Ret = Stream.EndRead(ar);
            }
            catch (Exception ex)
            {
                End(ex);
                return;
            }

            if (Ret <= 0)
            {
                if (Ret == 0)
                    End("收到空数据 断开");
                else
                    End("读取数据出错");
                return;
            }
            //取出数据
            var buf = ByteHelper.SubBytes(Buffer, 0, Ret);

            //业务处理
            try
            {
                if (OnRead == null) End("必须指定异步读取完成事件！");

                buf = OnRead(buf);
            }
            catch (Exception ex)
            {
                WriteLog("EndRead业务处理出错！" + ex.Message);
            }

            // 重新建立委托
            BeginRead();
        }
        #endregion

        #region 异步写
        /// <summary>
        /// 开始异步写入
        /// </summary>
        public void BeginWrite(Byte[] buf)
        {
            if (buf == null || buf.Length < 1) return;

            if (!Active) return;

            try
            {
                var iar = Stream.BeginWrite(buf, 0, buf.Length, new AsyncCallback(EndWrite), null);
                CheckTimeOut(iar);
            }
            catch (Exception ex)
            {
                End(ex);
                return;
            }
        }

        /// <summary>
        /// 处理异步写入的结束
        /// </summary>
        /// <param name="ar"></param>
        private void EndWrite(IAsyncResult ar)
        {
            if (!Active) return;

            try
            {
                Stream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                End(ex);
                return;
            }
        }
        #endregion

        #region 同步读
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        public Byte[] Read()
        {
            if (!Active) return null;

            var list = new List<NetData>();
            var buf = new Byte[1024 * 8];
            try
            {
                //如果没数据，等100ms
                if (Socket.Available == 0 && Socket.Connected)
                {
                    Socket.Poll(100000 /* 100ms */, SelectMode.SelectRead);
                    //如果还是没数据，等10秒
                    if (Socket.Available == 0 && Socket.Connected)
                    {
                        Socket.Poll(Session.TimeOut * 1000 /* 10sec */, SelectMode.SelectRead);
                        //如果还是没有数据，退出
                        if (Socket.Available == 0 && Socket.Connected) return null;
                    }
                }

                do
                {
                    //读数据
                    var count = Stream.Read(buf, 0, buf.Length);
                    if (count == 0) break;
                    list.Add(new NetData(buf, 0, count));
                } while (Socket.Available != 0 && Socket.Connected);
            }
            catch { }

            if (list == null || list.Count < 1) return null;
            return NetData.Join(list);
        }
        #endregion

        #region 同步写
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buf"></param>
        public void Write(Byte[] buf)
        {
            if (!Active) return;

            Stream.Write(buf, 0, buf.Length);
        }
        #endregion
        #endregion

        #region 辅助函数
        #region 异步超时检查
        /// <summary>
        /// 建立检查是否超时的委托
        /// </summary>
        /// <param name="iar"></param>
        private void CheckTimeOut(IAsyncResult iar)
        {
            ThreadPool.RegisterWaitForSingleObject(iar.AsyncWaitHandle, new WaitOrTimerCallback(TimeOutCallback), iar, Session.TimeOut, true);
        }

        /// <summary>
        /// 超时回调函数
        /// </summary>
        /// <param name="state">一个对象，包含回调方法在每次执行时要使用的信息</param>
        /// <param name="timedOut">如果 WaitHandle 超时，则为 true；如果其终止，则为 false</param>
        private void TimeOutCallback(Object state, Boolean timedOut)
        {
            // 如果是因为超时而触发，不是因为对象被销毁而触发
            if (timedOut)
            {
                var ar = state as IAsyncResult;
                if (ar != null && !ar.IsCompleted)
                {
                    End("超时退出");
                    //Dispose();
                }
            }
        }
        #endregion

        #region 写日志
        ///<summary>输入日志</summary>
        ///<remarks>输入日志信息到UI信息框</remarks>
        ///<param name="log">要输出的日志信息</param>
        public void WriteLog(String log)
        {
            Session.WriteLog(String.Format("{0} {1}", Name, log));
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// 异步读取完成委托
    /// </summary>
    /// <param name="buf">数据</param>
    public delegate Byte[] ReadCompleteDelegate(Byte[] buf);
}