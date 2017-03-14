using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;
using google.protobuf;
namespace GameEngine.Network
{
    public class Client
    {
        public int Id;
        public int GameId;
        public PlayerInfo Info;
        private int BufferSize = 1024;
        private byte[] RecvBuffer;
        private int RecvOffset = 0;
        private int PackOffset = 0;
        private int PackLength = 0;

        public  CallBack<Client, SocketAsyncEventArgs, byte, byte[], ushort, ushort> Handle;
        private ServerIOCP server;
        private SocketAsyncEventArgs recv;
        public static Pools<SocketAsyncEventArgs> sendEventArgsPool;

        public void Init(ServerIOCP _server, SocketAsyncEventArgs _recv, int _bufferSize, CallBack<Client, SocketAsyncEventArgs, byte, byte[], ushort, ushort> _Handle)
        {
            server = _server;
            recv = _recv;
            BufferSize = _bufferSize;
            Handle = _Handle;
            RecvBuffer = new byte[BufferSize];
            recv.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
            recv.SetBuffer(new Byte[this.BufferSize], 0, this.BufferSize);
        }
        
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        #region 接收
        /// <summary>
        /// 开始接受数据
        /// </summary>
        /// <param name="socket"></param>
        public void StartRecv(Socket socket)
        {
            recv.AcceptSocket = socket;
            if (!socket.ReceiveAsync(recv))
            {
                ProcessReceive();
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive()
        {
            //Console.WriteLine("ProcessReceive");
            // 检查远程主机是否关闭连接
            if (recv.BytesTransferred > 0)
            {
                if (recv.SocketError == SocketError.Success)
                {
                    //Socket s = recv.AcceptSocket;
                    int total_length = RecvOffset + recv.BytesTransferred;
                    if (total_length > RecvBuffer.Length)
                    {//接受的数据超过缓冲区
                        Log.Debug("====接受的数据超过缓冲区====");
                        int buff_data_size = RecvOffset - PackOffset;
                        int buff_total_size = recv.BytesTransferred + buff_data_size;
                        if(buff_total_size > BufferSize)
                        {
                            BufferSize = buff_total_size;
                        }
                        Byte[] newBuffer = new Byte[BufferSize];
                        if (buff_data_size > 0)
                        {
                            Buffer.BlockCopy(RecvBuffer, PackOffset, newBuffer, 0, buff_data_size);
                        }
                        RecvBuffer = newBuffer;
                        PackOffset = 0;
                        RecvOffset = buff_data_size;
                    }

                    //===拷贝数据到缓存===
                    Buffer.BlockCopy(recv.Buffer, 0, RecvBuffer, RecvOffset, recv.BytesTransferred);
                    RecvOffset += recv.BytesTransferred;
                    PackLength = RecvOffset - PackOffset;//接受数据的长度
                    //================解析数据==============
                    ushort DataSize, MsgSize;
                    byte command;
                    while (PackLength >= 3)//接受数据长度必须至少包含2个字节的长度和一个字节的命令
                    {
                        DataSize = BitConverter.ToUInt16(RecvBuffer, PackOffset);//包长度
                        if (DataSize <= PackLength)//包长度大于接受数据长度
                        {
                            command = RecvBuffer[PackOffset+2];//命令
                            MsgSize = (ushort)(DataSize - 3);//消息体长度
                            Handle(this, recv, command, RecvBuffer, (ushort)(PackOffset +3), MsgSize);
                            //==========
                            PackOffset += DataSize;
                            PackLength = RecvOffset - PackOffset;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                    if (!recv.AcceptSocket.ReceiveAsync(recv))
                    {
                        // 同步接收时处理接收完成事件
                        this.ProcessReceive();
                    }
                }
                else
                {
                    this.CloseReceiveSocket();
                }
            }
            else
            {
                this.CloseReceiveSocket();
            }
        }

        #endregion
        #region 接收错误
        private void CloseReceiveSocket()
        {
            CloseSocket(recv.AcceptSocket);
            recv.AcceptSocket = null;
            server.CloseRecv(this);
        }
        #endregion

        #region 发送
        public void Send<T>(byte type, T t)
        {
            try
            {
                //============ 生成数据 ============
                byte[] msg;
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize<T>(ms, t);
                    msg = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(msg, 0, msg.Length);
                }

                ushort total_length = (ushort)(msg.Length + 3);
                byte[] total_length_bytes = BitConverter.GetBytes(total_length);
                //消息体结构：消息体长度 + 消息体
                byte[] data = new byte[total_length];
                total_length_bytes.CopyTo(data, 0);
                data[2] = type;
                msg.CopyTo(data, 3);

                //============ 发送数据 ============
                SocketAsyncEventArgs sendEventArgs = CreateSendEventArgs(data);
                Socket socket = recv.AcceptSocket;
                //投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                if (!socket.SendAsync(sendEventArgs))
                {
                    // 同步发送时处理发送完成事件
                    ProcessSend(sendEventArgs);
                }
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx);
            }

        }
        public void Send(byte type, byte[] msg)
        {
            try
            {
                ushort total_length = (ushort)(msg.Length + 3);
                byte[] total_length_bytes = BitConverter.GetBytes(total_length);
                //消息体结构：消息体长度 + 消息体
                byte[] data = new byte[total_length];
                total_length_bytes.CopyTo(data, 0);
                data[2] = type;
                msg.CopyTo(data, 3);

                //============ 发送数据 ============
                SocketAsyncEventArgs sendEventArgs = CreateSendEventArgs(data);
                Socket socket = recv.AcceptSocket;
                //投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                if (!socket.SendAsync(sendEventArgs))
                {
                    // 同步发送时处理发送完成事件
                    ProcessSend(sendEventArgs);
                }
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx);
            }

        }
        /// <summary>
        /// 发送成功
        /// </summary>
        /// <param name="sendEventArgs"></param>
        private void ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                // 发送完成后回收
                sendEventArgsPool.Push(sendEventArgs);
            }
            else
            {
                this.ProcessSendError(sendEventArgs);
            }
        }

        private SocketAsyncEventArgs CreateSendEventArgs(Byte[] buffer)
        {
            SocketAsyncEventArgs sendEventArgs = sendEventArgsPool.Pop();
            if (sendEventArgs == null)
            {
                sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
            }
            sendEventArgs.AcceptSocket = recv.AcceptSocket;
            sendEventArgs.SetBuffer(buffer, 0, buffer.Length);
            return sendEventArgs;
        }
        #endregion
        #region 发送错误
        // 处理socket错误
        private void ProcessSendError(SocketAsyncEventArgs sendEventArgs)
        {
            Socket socket = sendEventArgs.AcceptSocket;
            IPEndPoint localEndPoint = socket.LocalEndPoint as IPEndPoint;

            Log.Error("[Server] Socket error {0} on endpoint {1} during {2}.", (Int32)sendEventArgs.SocketError, localEndPoint, sendEventArgs.LastOperation);
            CloseSocket(socket);
            sendEventArgs.AcceptSocket = null;
            sendEventArgsPool.Push(sendEventArgs);
        }
        #endregion

        private static void CloseSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                socket.Close();
            }
        }
    }
}
