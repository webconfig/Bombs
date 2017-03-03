using Comm.Util;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Comm.Network.Iocp
{
    public class ServerIOCP<C>
    {
        // 监听Socket，用于接受客户端的连接请求
        private Socket listenSocket;

        // 用于服务器执行的互斥同步对象
        private static Mutex mutex = new Mutex();

        // 用于每个I/O Socket操作的缓冲区大小
        private Int32 bufferSize;

        // 服务器上连接的客户端总数
        private Int32 numConnectedSockets;

        // 服务器能接受的最大连接数量
        private Int32 numConnections;

        // 完成端口上进行投递所用的连接对象池
        private Pools<SocketAsyncEventArgs> acceptEventArgsPool;
        private Pools<Session<C>> receiveEventArgsPool;
        //=================================
        public PacketHandlerManager<C> Handlers { get; set; }
        //==================================

        /// <summary>
        /// 构造函数，建立一个未初始化的服务器实例
        /// </summary>
        /// <param name="numConnections">服务器的最大连接数据</param>
        /// <param name="bufferSize"></param>
        public ServerIOCP(Int32 numConnections, Int32 bufferSize, PacketHandlerManager<C> _Handlers)
        {
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.bufferSize = bufferSize;
            this.Handlers = _Handlers;
            acceptEventArgsPool = new Pools<SocketAsyncEventArgs>(numConnections);
            receiveEventArgsPool = new Pools<Session<C>>(numConnections);
            Session<C>.sendEventArgsPool = new Pools<SocketAsyncEventArgs>(numConnections);

            // 为连接池预分配 SocketAsyncEventArgs 对象
            for (Int32 i = 0; i < this.numConnections; i++)
            {
                SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                Session<C> session = new Session<C>();
                session.Init(this, eventArg,bufferSize, Handlers.Handle);
                receiveEventArgsPool.Push(session);
            }
        }

        public void Start(string ip, Int32 port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            // 创建监听socket
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.ReceiveBufferSize = this.bufferSize;
            this.listenSocket.SendBufferSize = this.bufferSize;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // 配置监听socket为 dual-mode (IPv4 & IPv6) 
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                this.listenSocket.Bind(localEndPoint);
            }
            Log.Info("服务器---〉{0}：{1},开始监听.", localEndPoint.Address.ToString(), port.ToString());
            // 开始监听
            this.listenSocket.Listen(this.numConnections);

            // 在监听Socket上投递一个接受请求。
            this.StartAccept(null);

            // Blocks the current thread to receive incoming messages.
            mutex.WaitOne();
        }

        // 停止服务
        public void Stop()
        {
            listenSocket.Close();
            mutex.ReleaseMutex();
        }

        #region 连接
        // 从客户端开始接受一个连接操作
        private void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = CreateAcceptEventArgs();
            }
            else
            {
                // 重用前进行对象清理
                acceptEventArgs.AcceptSocket = null;
            }

            if (!listenSocket.AcceptAsync(acceptEventArgs))
            {
                ProcessAccept(acceptEventArgs);
            }
        }
        // 获得或创建一个用于 Accept 的 EventArgs
        private SocketAsyncEventArgs CreateAcceptEventArgs()
        {
            SocketAsyncEventArgs acceptEventArgs = acceptEventArgsPool.Pop();
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                acceptEventArgs.AcceptSocket = null;
            }
            return acceptEventArgs;
        }
        // accept 操作完成时回调函数
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }
        // 监听Socket接受处理
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            //Console.WriteLine("ProcessAccept");
            Socket socket = acceptEventArgs.AcceptSocket;
            // 是否连接失败
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                Console.WriteLine("[Server] Failed to accept");
                StartAccept(null);
                // Destroy this socket, since it could be bad.
                acceptEventArgs.AcceptSocket.Close();
                // Put the SAEA back in the pool.
                acceptEventArgsPool.Push(acceptEventArgs);
                return;
            }

            try
            {
                Session<C> session = this.receiveEventArgsPool.Pop();
                //Console.WriteLine("ProcessAccept:eventArg: {0}", eventArg.GetHashCode());
                if (session != null)
                {
                    session.StartRecv(socket);
                    // 从接受的客户端连接中取数据配置ioContext
                    Interlocked.Increment(ref this.numConnectedSockets);
                    Console.WriteLine("[Server] Client connection accepted. There are {0} clients connected to the server", this.numConnectedSockets);

                   
                }
                else
                {
                    //已经达到最大客户连接数量，在这接受连接，发送“连接已经达到最大数”，然后断开连接
                    //s.Send(Encoding.Default.GetBytes("连接已经达到最大数!"));
                    Console.WriteLine("[Server] Max client connections");
                    socket.Close();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[Server] Error when processing data received from {0}: {1}", acceptEventArgs.AcceptSocket.RemoteEndPoint, ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server] Exception: {0}", ex.ToString());
            }
            finally
            {
                // 投递下一个接受请求
                StartAccept(acceptEventArgs);
            }
        }
        #endregion


        #region 异常
        public void CloseRecv(Session<C> session)
        {
            Interlocked.Decrement(ref this.numConnectedSockets);
            receiveEventArgsPool.Push(session);
        }
        #endregion
    }
}
