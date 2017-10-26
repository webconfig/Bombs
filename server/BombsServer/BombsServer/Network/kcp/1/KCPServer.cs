using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO;
using ProtoBuf;
using google.protobuf;

public delegate void RecvDataHandler(ClientSession session, byte[] data, int offset, int size);
public class KCPServer
{
    private static readonly DateTime utc_time = new DateTime(1970, 1, 1);

    public static UInt32 iclock()
    {
        return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
    }

    private int m_port;
    private UdpClient UDPClient;
    internal Stopwatch m_watch;

    public event RecvDataHandler RecvData;
    /// <summary>
    /// 新的客户端连接事件
    /// </summary>
    public event RecvDataHandler NewClientSession;
    public event Action<ClientSession> CloseClientSession;

    public INewClientSessionProcessor NewSessionProcessor;
    public ArrayPool<byte> BytePool;
    public int state = 0;
    //===========
    private Thread recvThraed;

    public KCPServer(int port)
    {
        m_port = port;

        if (UdpLibConfig.UseBytePool)
        {
            BytePool = ArrayPool<byte>.Create(8 * 1024, 50);
        }
        else
        {
            BytePool = ArrayPool<byte>.System();
        }
    }

    public void Start()
    {
        state = 1;
        m_watch = Stopwatch.StartNew();
        Log.Info("kcp 端口：" + m_port);
        UDPClient = new UdpClient(new IPEndPoint(IPAddress.Any, m_port));
        /// 改代码段设置解决了UDP通讯的10054异常
        uint IOC_IN = 0x80000000;
        uint IOC_VENDOR = 0x18000000;
        uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
        UDPClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);


        recvThraed = new Thread(OnRecievedData);
        recvThraed.Start();
    }

    #region 接受处理数据
    void OnRecievedData()
    {
        while(state>=0)
        {
            try
            {
                IPEndPoint ipendpoint = null;
                byte[] receiveBytes = UDPClient.Receive(ref ipendpoint);
                if (null != receiveBytes)
                {
                    DealData(ipendpoint, receiveBytes);
                }
            }
            catch (Exception ex)
            {
                Log.Error("xxxx  ex:" + ex.ToString());
            }
        }
    }
    private void DealData(IPEndPoint RemoteEndPoint, byte[] buffer)
    {
        try
        {
            uint index = 0, key = 0;
            if (IsHandshake(buffer, 0, buffer.Length, out index, out key))
            {//先握手,使用纯udp进行握手，8字节0xFF+4字节conv+4字节key
                var c = GetSession(index);
                uint cc = 0;
                KCP.ikcp_decode32u(buffer, 16, ref cc);
                if (c == null)
                {
                    bool add = true;
                    //新连接处理，如果返回false，则不予处理，可以用来进行非法连接的初步处理
                    if (NewSessionProcessor != null)
                    {
                        add = NewSessionProcessor.OnNewSession(index, RemoteEndPoint);
                    }
                    if (add == false)
                    {
                        return;
                    }
                    c = AddSession(RemoteEndPoint, index);
                    c.m_KCPServer = this;
                    c.m_LastRecvTimestamp = m_watch.Elapsed;
                    OnNewClientSession(c, buffer, 0, buffer.Length);
                }
                else
                {
                    Log.Info("====重新设置EndPoint====");
                    c.EndPoint = RemoteEndPoint;
                    //如果客户端关闭并且立刻重连，这时候是连不上的，因为KCP中原有的数据不能正确处理
                    //c.ResetKCP();
                }

                c.Status = ClientSessionStatus.Connected;
                //回发握手请求
                UDPClient.BeginSend(buffer, buffer.Length, RemoteEndPoint, (IAsyncResult iar) => { UDPClient.EndSend(iar); }, null);

            }
            else
            {
                try
                {
                    KCP.ikcp_decode32u(buffer, 0, ref index);
                    var c = GetSession(index);
                    if (c != null && c.Status == ClientSessionStatus.Connected)
                    {
                        //Debug.Assert(c.EndPoint.ToString() == RemoteEndPoint.ToString());
                        if (c.EndPoint.ToString() != RemoteEndPoint.ToString())
                        {
                            c.EndPoint = RemoteEndPoint;
                            Log.Error("==ClientSessionStatus.Connected==");
                        }
                        c.AddRecv(buffer);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("==DealData error:" + ex.ToString());

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DealData Error:" + ex.ToString());
        }
    }
    #endregion

    public void Update()
    {
        //添加新的连接
        if (m_clients_add.Count > 0)
        {
            lock (m_clients_add)
            {
                foreach (var item in m_clients_add)
                {
                    m_clients.Add(item.Key, item.Value);
                }
                m_clients_add.Clear();
            }
        }

        //释放连接
        while (m_disposedQueue.Count > 0)
        {
            var e = m_disposedQueue.Dequeue();
            if (m_clients.ContainsKey(e.NetIndex))
            {
                m_clients.Remove(e.NetIndex);
            }

            if (CloseClientSession != null)
            {
                CloseClientSession(e);
            }
        }
        //运行
        foreach (var item in m_clients)
        {
            item.Value.Update();
        }
    }

    private bool IsHandshake(byte[] buffer, int offset, int size, out uint index, out uint key)
    {
        if (HandshakeUtility.IsHandshakeDataRight(buffer, offset, size, out index, out key))
        {
            return IsClientKeyCorrect(index, (int)key);
        }
        else
        {
            return false;
        }
    }

    internal void Send(ClientSession session, byte[] data, int size)
    {
        UDPClient.BeginSend(data, size, session.EndPoint, (IAsyncResult iar) => { UDPClient.EndSend(iar); }, null);
    }
    internal void OnRecvData(ClientSession session, byte[] data, int offset, int size)
    {
        var e = RecvData;
        if (e != null)
        {
            e(session, data, offset, size);
        }
    }
    internal void OnNewClientSession(ClientSession clientSession, byte[] buf, int offset, int size)
    {
        //clientSession.Send("Connect OK");

        var e = NewClientSession;
        if (e != null)
        {
            e(clientSession, buf, offset, size);
        }
    }
    internal void OnCloseClientSession(ClientSession clientSession)
    {
        var e = CloseClientSession;
        if (e != null)
        {
            e(clientSession);
        }
    }

    #region ClientSession
    Dictionary<uint, ClientSession> m_clients = new Dictionary<uint, ClientSession>();
    Dictionary<uint, ClientSession> m_clients_add = new Dictionary<uint, ClientSession>();
    private object LOCK = new object();
    private ClientSession GetSession(uint index)
    {
        ClientSession ret;
        if (!m_clients.TryGetValue(index, out ret))
        {
            m_clients_add.TryGetValue(index, out ret);
        }
        return ret;
    }
    private ClientSession AddSession(IPEndPoint remoteEndPoint, uint index)
    {
        var ret = new ClientSession(index);
        ret.EndPoint = remoteEndPoint;
        lock (m_clients_add)
        {
            m_clients_add.Add(index, ret);
        }
        return ret;
    }
    #endregion

    #region key 
    Dictionary<uint, int> m_clientsKey = new Dictionary<uint, int>();
    public void AddClientKey(uint index, int key)
    {
        if (m_clientsKey.ContainsKey(index))
        {
            m_clientsKey.Remove(index);
        }
        m_clientsKey.Add(index, key);
    }
    public void RmeoveClientKey(uint index)
    {
        m_clientsKey.Remove(index);
    }
    public bool IsClientKeyCorrect(uint index, int key)
    {
        int k;
        bool has = m_clientsKey.TryGetValue(index, out k);
        if (has == false)
        {
            Console.WriteLine("未找到key.Index:" + index.ToString());
        }
        return key == k;
    }
    #endregion

    private Queue<ClientSession> m_disposedQueue = new Queue<ClientSession>();
    internal void AddToDisposedQueue(ClientSession clientSession)
    {
        m_disposedQueue.Enqueue(clientSession);
    }

    public void Close()
    {
        state = -100;
        m_watch.Stop();
        UDPClient.Close();
        lock (m_clients_add)
        {
            m_clients_add.Clear();
        }
        lock (m_clients)
        {
            m_clients.Clear();
        }
    }
}
public class ClientSession
{
    private uint m_netIndex;
    /// <summary>
    /// 客户端连接索引conv(index)
    /// </summary>
    public uint NetIndex
    {
        get
        {
            return m_netIndex;
        }


    }
    ClientSessionStatus m_Status = ClientSessionStatus.InConnect;
    public ClientSessionStatus Status
    {
        get
        {
            return m_Status;
        }
        set
        {
            m_Status = value;
        }
    }
    public int Key;
    public IPEndPoint EndPoint;
    public ClientSessionDisposeReason DisposeReason = ClientSessionDisposeReason.None;
    internal KCPServer m_KCPServer;
    /// <summary>
    /// 最后收到数据的时刻。
    /// 当超过UdpLibConfig.MaxTimeNoData时间没有收到客户端的数据，则可以认为是死链接
    /// </summary>
    internal TimeSpan m_LastRecvTimestamp;
    internal KCP m_Kcp;
    private bool m_NeedUpdateFlag = false;
    private UInt32 m_NextUpdateTime;
    //===========接受数据
    private List<byte[]> recv_datas = new List<byte[]>();
    public ClientSession(uint index)
    {
        init_kcp(index);
        m_netIndex = index;
    }

    void init_kcp(UInt32 conv)
    {
        m_Kcp = new KCP(conv, null);
        m_Kcp.SetOutput((byte[] buf, int size, object user) =>
           {
               m_KCPServer.Send(this, buf, size);
           });

        // fast mode.
        m_Kcp.NoDelay(1, 20, 2, 1);
        m_Kcp.WndSize(128, 128);
    }

    /// <summary>
    /// 和Update同一个线程调用
    /// </summary>
    /// <param name="buf"></param>
    public void Send(byte[] buf)
    {
        m_Kcp.Send(buf);
        m_NeedUpdateFlag = true;
    }

    /// <summary>
    /// 和Update同一个线程调用
    /// </summary>
    public void Send(string str)
    {
        byte[] buf = this.m_KCPServer.BytePool.Rent(32 * 1024);
        int bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(str, 0, str.Length, buf, 0);
        Send(buf);
        this.m_KCPServer.BytePool.Return(buf, false);
    }

    public void Update()
    {
        try
        {
            if (recv_datas.Count > 0)
            {
                lock (recv_datas_obj)
                {
                    for (int i = 0; i < recv_datas.Count; i++)
                    {
                        process_recv_queue(recv_datas[i]);
                    }
                    recv_datas.Clear();
                }
            }
            update(KCPServer.iclock());
            if (m_KCPServer.m_watch.Elapsed - m_LastRecvTimestamp > UdpLibConfig.MaxTimeNoData)
            {
                DisposeReason = ClientSessionDisposeReason.MaxTimeNoData;
                Dispose();
            }
        }
        catch (Exception ex)
        {
            Log.Error("1111111111111111111111111:" + ex.ToString());
        }
    }

    private object recv_datas_obj = new object();
    public void AddRecv(byte[] datas)
    {
        lock (recv_datas_obj)
        {
            recv_datas.Add(datas);
        }
    }

    /// <summary>
    /// 和Update同一个线程调用
    /// </summary>
    internal void process_recv_queue(byte[] datas)
    {
#if DEV
            IRQLog.AppLog.Log(this.m_netIndex.ToString() + ",接收1");
#endif
        if (m_Kcp != null && datas != null)
        {
            m_Kcp.Input(datas);

            m_NeedUpdateFlag = true;

            for (var size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
            {
                byte[] buffer;
                buffer = (UdpLibConfig.UseBytePool ? m_KCPServer.BytePool.Rent(size) : new byte[size]);
                try
                {

                    if (m_Kcp.Recv(buffer) > 0)
                    {
                        m_LastRecvTimestamp = m_KCPServer.m_watch.Elapsed;

                        uint key = 0;
                        KCP.ikcp_decode32u(buffer, 0, ref key);
                        if (m_KCPServer.IsClientKeyCorrect(this.m_netIndex, (int)key) == false)
                        {
#if DEBUG
                            Console.WriteLine("index:{0} key 不对", this.m_netIndex);
#endif
                            m_KCPServer.BytePool.Return(buffer, true);
                            DisposeReason = ClientSessionDisposeReason.IndexKeyError;
                            //key不对
                            Dispose();
                            return;
                        }
#if DEV
                    IRQLog.AppLog.Log(this.m_netIndex.ToString() + ",接收2");
#endif
                        m_KCPServer.OnRecvData(this, buffer, 0, size);
                    }
                }
                finally
                {
                    if (UdpLibConfig.UseBytePool)
                    {
                        m_KCPServer.BytePool.Return(buffer, true);
                    }
                }
            }
        }
    }

    private bool m_Disposed = false;

    private void Dispose()
    {
        if (m_Disposed)
        {
            return;
        }
        m_Disposed = true;
        m_KCPServer.AddToDisposedQueue(this);
    }

    void update(UInt32 current)
    {
        if (m_Status != ClientSessionStatus.Connected)
        {
            return;
        }
        if (m_NeedUpdateFlag || current >= m_NextUpdateTime)
        {
            m_Kcp.Update(current);
            m_NextUpdateTime = m_Kcp.Check(current);
            m_NeedUpdateFlag = false;
        }
    }

    internal void ResetKCP()
    {
        init_kcp(m_netIndex);
    }

    //===========
    //public PlayerInfo Info;
    public int GameId;
    public void Send<T>(byte type, T t)
    {
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
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        msg.CopyTo(data, 3);

        try
        {
            Send(data);
        }
        catch (Exception ex)
        {
            //Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    public void Send(byte type, byte[] msg)
    {
        ushort total_length = (ushort)(msg.Length + 3);
        byte[] total_length_bytes = BitConverter.GetBytes(total_length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        msg.CopyTo(data, 3);

        try
        {
            Send(data);
        }
        catch (Exception ex)
        {
            //Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    public void Send(byte type)
    {
        ushort total_length = 3;
        byte[] total_length_bytes = BitConverter.GetBytes(total_length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        try
        {
            Send(data);
        }
        catch (Exception ex)
        {
            //Log.Error("发送数据错误:" + ex.ToString());
        }
    }
}

public enum ClientSessionDisposeReason
{
    None = 0,
    Normal,
    IndexKeyError,
    MaxTimeNoData
}
/// <summary>
/// 新客户端接口
/// </summary>
public interface INewClientSessionProcessor
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="remoteEndPoint"></param>
    /// <returns>如果为false，则丢弃</returns>
    bool OnNewSession(uint index, EndPoint remoteEndPoint);
}
