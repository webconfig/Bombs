//using System;
//using System.Net.Sockets;
//using Comm.Util;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using ProtoBuf;
//using System.Linq;
//using System.Threading;

//namespace Comm.Network.Iocp
//{
//    public class Session2<C> : IDisposable
//    {
//        private int BufferSize = 1024;
//        public ushort SessionId;
//        public Socket Socket { get; set; }
//        public CallBack<Session<C>, byte, byte[], ushort, ushort> Handle;
//        private byte[] RecvBuffer;
//        private int CurrentOffset = 0;
//        public C data;

//        public void Init(int _bufferSize, CallBack<Session<C>, byte, byte[], ushort, ushort> _Handle, CallBack<Session<C>> _closeEvent)
//        {
//            BufferSize = _bufferSize;
//            Handle = _Handle;
//            RecvBuffer = new byte[BufferSize];
//            CloseEvent = _closeEvent;
//        }
//        private void Asyn_Completed(object sender, SocketAsyncEventArgs e)
//        {
//            switch (e.LastOperation)
//            {
//                case SocketAsyncOperation.Connect:
//                    if (ConnectOk != null)
//                    {
//                        ConnectOk();
//                        ConnectOk = null;
//                    }
//                    asyn.RemoteEndPoint = null;
//                    this.BeginRecv();
//                    break;
//                case SocketAsyncOperation.Receive:
//                    this.ProcessReceive();
//                    break;
//                case SocketAsyncOperation.Send:
//                    this.ProcessSend();
//                    break;
//                default:
//                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
//            }
//        }
//        public void Dispose()
//        {
//            //Todo: dispose pools
//            //m_hSendOp.Dispose();
//            //m_hRecvOp.Dispose();
//        }

//        public void Close()
//        {
//            try
//            {
//                this.Socket.Shutdown(SocketShutdown.Both);
//                this.Socket.Close();
//                this.Socket = null;
//            }
//            catch (Exception)
//            {
//                //TODO: better check if something went wrong during shutdown
//            }
//        }

//        #region 连接
//        public CallBack ConnectOk;
//        public void Connect(IPEndPoint ipEndPoint, CallBack _ConnectOk)
//        {
//            ConnectOk = _ConnectOk;
//            asyn.RemoteEndPoint = ipEndPoint;
//            if (!Socket.ConnectAsync(asyn))
//            {
//                asyn.RemoteEndPoint = null;
//                this.BeginRecv();
//            }
//        }
//        #endregion

//        #region 接受
//        public void BeginRecv()
//        {
//            if (!Socket.ReceiveAsync(asyn))
//            {
//                ProcessReceive();
//            }
//        }

//        private void ProcessReceive()
//        {
//            // 检查远程主机是否关闭连接
//            if (asyn.BytesTransferred > 0)
//            {
//                Comm.Util.Log.Debug("ReceiveAsync--->:" + asyn.SocketError);
//                if (asyn.SocketError == SocketError.Success)
//                {
//                    //判断所有需接收的数据是否已经完成
//                    if (Socket.Available == 0)
//                    {
//                        //===拷贝数据到缓存===
//                        Buffer.BlockCopy(asyn.Buffer, 0, RecvBuffer, CurrentOffset, asyn.BytesTransferred);
//                        CurrentOffset += asyn.BytesTransferred;

//                        ushort DataSize, TotalSize, MsgSize;
//                        byte command;
//                        while (CurrentOffset >= 3)
//                        {
//                            DataSize = BitConverter.ToUInt16(RecvBuffer, 0);
//                            TotalSize = DataSize;
//                            if (TotalSize <= CurrentOffset)
//                            {
//                                command = RecvBuffer[2];//命令
//                                MsgSize = (ushort)(DataSize - 3);
//                                Handle(this, command, RecvBuffer, 3, MsgSize);
//                                //---删除----
//                                Array.Clear(RecvBuffer, 0, TotalSize);
//                                CurrentOffset -= TotalSize;
//                            }
//                            else
//                            {
//                                break;
//                            }
//                        }
//                    }
//                    else if (!Socket.ReceiveAsync(asyn))    //为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
//                    {
//                        // 同步接收时处理接收完成事件
//                        this.ProcessReceive();
//                    }
//                }
//            }
//            else
//            {
//                this.ProcessError();
//            }
//        }
//        #endregion

//        #region 发送
//        public void Send<T>(byte type, T t)
//        {
//            try
//            {
//                //============生成数据============
//                byte[] msg;
//                using (MemoryStream ms = new MemoryStream())
//                {
//                    Serializer.Serialize<T>(ms, t);
//                    msg = new byte[ms.Length];
//                    ms.Position = 0;
//                    ms.Read(msg, 0, msg.Length);
//                }

//                ushort total_length = (ushort)(msg.Length + 3);
//                byte[] total_length_bytes = BitConverter.GetBytes(total_length);
//                //消息体结构：消息体长度+消息体
//                byte[] data = new byte[total_length];
//                total_length_bytes.CopyTo(data, 0);
//                data[2] = type;
//                msg.CopyTo(data, 3);
//                //============发送数据============
//                Array.Copy(data, 0, asyn.Buffer, 0, data.Length);
//                asyn.SetBuffer(asyn.Offset, data.Length);

//                //using (ExecutionContext.SuppressFlow())
//                //{
//                //    pending = this.Socket.SendAsync(operation);
//                //}

//                if (!Socket.SendAsync(asyn))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
//                {
//                    // 同步发送时处理发送完成事件
//                    this.ProcessSend();
//                }
//            }
//            catch (Exception hEx)
//            {
//                Console.WriteLine(hEx);
//            }

//        }
//        /// <summary>
//        /// 发送完成时处理函数
//        /// </summary>
//        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
//        private void ProcessSend()
//        {
//            Comm.Util.Log.Debug("ProcessSend--->:" + asyn.SocketError);
//            if (asyn.SocketError == SocketError.Success)
//            {
//                //接收时根据接收的字节数收缩了缓冲区的大小，因此投递接收请求时，恢复缓冲区大小
//                asyn.SetBuffer(0, BufferSize);
//                Comm.Util.Log.Debug("ReceiveAsync--->");
//                if (!Socket.ReceiveAsync(asyn))     //投递接收请求
//                {
//                    // 同步接收时处理接收完成事件
//                    this.ProcessReceive();
//                }
//            }
//            else
//            {
//                this.ProcessError();
//            }
//        }
//        #endregion

//        #region 错误
//        public CallBack<Session<C>> CloseEvent;
//        /// <summary>
//        /// 处理socket错误
//        /// </summary>
//        /// <param name="e"></param>
//        private void ProcessError()
//        {
//            IPEndPoint localEp = Socket.LocalEndPoint as IPEndPoint;
//            CloseEvent(this);
//            Console.WriteLine(String.Format("套接字错误 {0}, IP {1}, 操作 {2}。", (Int32)asyn.SocketError, localEp, asyn.LastOperation));
//        }
//        #endregion
//    }
//}
