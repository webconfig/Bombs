using System;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;
using ProtoBuf;
using System.Collections.Generic;

public enum UdpClientEvents
{
    Connected = 0,
    ConnectFail = 1,
    Close = 2,
    Recv = 3,
    Send = 4,
    ConnectTimeout = 5
}
/// <summary>
/// 客户端使用服务器生成的conv(index)和key与服务器进行通信。
/// 服务器使用tcp或者其他方式将conv和key告知客户端。
/// 客户端首先通过裸UDP与服务器握手，握手成功后使用KCP与服务端通信。
/// </summary>
public class KCPClient
{
    private static readonly DateTime utc_time = new DateTime(1970, 1, 1);
    //===========
    public int state = 0;
    public static UInt32 iclock()
    {
        return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
    }

    private UdpClient m_UdpClient;
    private IPEndPoint mIPEndPoint;

    public KCP m_Kcp;
    private bool m_NeedUpdateFlag;
    private UInt32 m_NextUpdateTime;

    private SwitchQueue<byte[]> m_RecvQueue;
    private uint m_NetIndex = 0;
    private int m_Key = 0;
    public ClientSessionStatus Status = ClientSessionStatus.InConnect;
    public event Action<UdpClientEvents, byte[]> Event;

    /// <summary>
    /// 最后收到数据的时刻。
    /// 当超过UdpLibConfig.MaxTimeNoData时间没有收到客户端的数据，则可以认为是死链接
    /// </summary>
    internal float m_LastRecvTimestamp;
    /// <summary>
    /// 最后发送数据的时刻。
    /// 当超过UdpLibConfig.MaxTimeNoData时间没有收到客户端的数据，则可以认为是死链接
    /// </summary>
    internal float m_LastSendTimestamp = 0;

    private IPEndPoint server_addr;
    //===========
    private Thread recvThraed;
    private Thread sendThread;
    private AsyncCallback send_back;
    public KCPClient()
    {
        send_back = new AsyncCallback(SendBack);
    }

    /// <summary>
    /// 连接到服务端。
    /// 在update中，状态将会更新。
    /// 如果status为ConnectFail，则需要重新Connect
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="index"></param>
    /// <param name="key"></param>
    public void Connect(string host, UInt16 port, uint index, int key)
    {
        m_RecvQueue = new SwitchQueue<byte[]>(128);

        m_UdpClient = new UdpClient();
        m_UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
        server_addr = new IPEndPoint(IPAddress.Parse(host), port);
        //m_UdpClient.Connect(new IPEndPoint(IPAddress.Parse(host), port));

        Status = ClientSessionStatus.InConnect;
        //init_kcp((UInt32)new Random((int)DateTime.Now.Ticks).Next(1, Int32.MaxValue));
        m_NetIndex = index;
        m_Key = key;
        init_kcp(index);
        m_sw = Stopwatch.StartNew();
        SendHandshake();
        m_LastSendHandshake = m_sw.ElapsedMilliseconds;
        //m_UdpClient.BeginReceive(ReceiveCallback, this);

        recvThraed = new Thread(OnRecievedData);
        recvThraed.Start();


        sendThread = new Thread(ThreadSendAction);
        sendThread.Start();
    }
    private int m_pktCounter = 0;
    //发送握手数据 16字节 8字节头+4字节index+4字节key
    private void SendHandshake()
    {
        if (UdpLibConfig.DebugLevel != (int)UdpLibLogLevel.None)
        {
#if DEBUG
            Console.WriteLine("发送握手数据");
#endif
        }

        Interlocked.Increment(ref m_pktCounter);
        byte[] buf = new byte[UdpLibConfig.HandshakeDataSize + 4];
        Array.Copy(UdpLibConfig.HandshakeHeadData, buf, UdpLibConfig.HandshakeHeadData.Length);
        KCP.ikcp_encode32u(buf, UdpLibConfig.HandshakeHeadData.Length, m_NetIndex);
        KCP.ikcp_encode32u(buf, UdpLibConfig.HandshakeHeadData.Length + 4, (uint)m_Key);
        KCP.ikcp_encode32u(buf, UdpLibConfig.HandshakeHeadData.Length + 8, (uint)m_pktCounter);
        //m_UdpClient.Send(buf, buf.Length);
        m_UdpClient.BeginSend(buf, buf.Length, server_addr, (IAsyncResult iar) => { m_UdpClient.EndSend(iar); }, null);
#if DEV
            string s = string.Format("{0},发送握手数据,{1}", m_NetIndex, m_pktCounter.ToString());
            IRQLog.AppLog.Log(s);
            Console.WriteLine(s);
#endif
    }

    private 

    void init_kcp(UInt32 conv)
    {
        m_Kcp = new KCP(conv, null);
        m_Kcp.SetOutput(SendAction);

        // fast mode.
        m_Kcp.NoDelay(1, 20, 2, 1);
        m_Kcp.WndSize(80, 80);
        m_Kcp.SetMinRTO(10);
        m_Kcp.SetFastResend(1);
    }

    void OnRecievedData()
    {
        while (state >= 0)
        {
            try
            {
                byte[] receiveBytes = m_UdpClient.Receive(ref mIPEndPoint);
                if (null != receiveBytes)
                {
                    m_RecvQueue.Push(receiveBytes);
                }
            }
            catch (SocketException ee)
            {
#if DEBUG
                if (ee.NativeErrorCode == 10054)
                {
                    Console.WriteLine("无法连接到远程服务器");
                }
                else
                {
                    Console.WriteLine(ee.ToString());
                }
#endif
                this.Status = ClientSessionStatus.ConnectFail;
                var e = Event;
                if (e != null)
                {
                    e(UdpClientEvents.ConnectFail, null);
                }
                return;
            }
        }
    }

    private List<byte[]> send_datas = new List<byte[]>();
    private List<byte[]> send_datas_add = new List<byte[]>();
    private bool send_ok = true;
    /// <summary>
    /// 发送数据。发送时，默认在头部加上4字节的key
    /// </summary>
    /// <param name="buf"></param>
    public void Send(byte[] buf)
    {
        byte[] newbuf = new byte[buf.Length + 4];
        //把key附上，服务端合法性检测用
        KCP.ikcp_encode32u(newbuf, 0, (uint)m_Key);
        Array.Copy(buf, 0, newbuf, 4, buf.Length);
        m_LastSendTimestamp = UnityEngine.Time.time;
        m_Kcp.Send(newbuf);
        m_NeedUpdateFlag = true;
        var e = Event;
        if (e != null)
        {
            e(UdpClientEvents.Send, buf);
        }
    }
    private void SendAction(byte[] buf, int size, object user)
    {
        byte[] kkk = new byte[size];
        Buffer.BlockCopy(buf, 0, kkk, 0, size);
        if (send_ok)
        {
            send_ok = false;
            //UnityEngine.Debug.Log("u发送：" + size);
            m_UdpClient.BeginSend(buf, size, server_addr, send_back, null);
        }
        else
        {
            lock(send_datas_add)
            {
                send_datas_add.Add(kkk);
            }
        }
    }
    private void ThreadSendAction()
    {
        while (state >= 0)
        {
            if (send_datas_add.Count > 0)
            {
                lock (send_datas_add)
                {
                    send_datas.AddRange(send_datas_add);
                    send_datas_add.Clear();
                }
            }
            if (send_ok && (send_datas_add.Count > 0))
            {
                send_ok = false;
                UnityEngine.Debug.Log("11111111111");
                m_UdpClient.BeginSend(send_datas_add[0], send_datas_add[0].Length, server_addr, send_back, null);
                send_datas_add.RemoveAt(0);
            }
            Thread.Sleep(1);
        }
    }
    private void SendBack(IAsyncResult iar)
    {
        m_UdpClient.EndSend(iar); send_ok = true;
    }




    public void Update()
    {
        update(iclock());
    }

    public void Close()
    {
        if(recvThraed!=null)
        {
            recvThraed.Abort();
            recvThraed = null;
        }
        if(sendThread!=null)
        {
            sendThread.Abort();
            sendThread = null;
        }
        if (m_UdpClient != null)
        {
            m_UdpClient.Close();
            m_UdpClient = null;
        }
        if (m_Kcp != null)
        {
            m_Kcp = null;
        }
        m_NeedUpdateFlag = false;
    }

    void process_recv_queue()
    {

        m_RecvQueue.Switch();

        while (!m_RecvQueue.Empty())
        {

            if (m_Kcp == null)
            {//退出
                return;
            }

            var buf = m_RecvQueue.Pop();
            if (this.Status == ClientSessionStatus.InConnect)
            {
                //服务端将返回和握手请求相同的响应数据
                uint _index = 0, _key = 0;
                if (HandshakeUtility.IsHandshakeDataRight(buf, 0, buf.Length, out _index, out _key))
                {
                    if (_index == m_NetIndex && _key == m_Key)
                    {
#if DEBUG
                        Console.WriteLine("连接握手成功");
#endif
                        this.Status = ClientSessionStatus.Connected;
                        var e = Event;
                        if (e != null)
                        {
                            m_LastSendTimestamp = UnityEngine.Time.time;
                            m_LastRecvTimestamp = UnityEngine.Time.time;
                            e(UdpClientEvents.Connected, null);
                        }
                        continue;
                    }
                }
            }

            m_Kcp.Input(buf);
            m_NeedUpdateFlag = true;

            for (var size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
            {
                var buffer = new byte[size];
                if (m_Kcp.Recv(buffer) > 0)
                {
                    m_LastRecvTimestamp = UnityEngine.Time.time;
                    var e = Event;
                    if (e != null)
                    {
                        e(UdpClientEvents.Recv, buffer);
                    }
                }
            }
        }
    }


    private long m_LastSendHandshake = 0;
    private Stopwatch m_sw;
    private int m_retryCount = 0;
    void update(UInt32 current)
    {
        switch (this.Status)
        {
            case ClientSessionStatus.InConnect:
                ProcessHandshake();
                break;
            case ClientSessionStatus.ConnectFail:
                return;
            case ClientSessionStatus.Connected:
                if (m_LastSendTimestamp > 0 && UnityEngine.Time.time - m_LastSendTimestamp > 1)
                {//超过1秒没发生数据，发送心跳包
                    m_LastSendTimestamp = UnityEngine.Time.time;
                    Send(200);
                    //UnityEngine.Debug.Log("发送心跳包");
                }
                if (UnityEngine.Time.time - m_LastRecvTimestamp > 5)
                {//超过5秒没收到数据，短线
                    this.Status = ClientSessionStatus.ConnectFail;
                    UnityEngine.Debug.Log("超过5秒没收到数据，短线");
                    Close();
                    if (Event != null)
                    {
                        Event(UdpClientEvents.Close, null);
                    }
                }
                break;
            default:
                break;
        }

        process_recv_queue();

        if (m_Kcp != null)
        {
            if (m_NeedUpdateFlag || current >= m_NextUpdateTime)
            {
                m_Kcp.Update(current);
                m_NextUpdateTime = m_Kcp.Check(current);
                m_NeedUpdateFlag = false;
            }
        }
    }

    private void ProcessHandshake()
    {
        //每隔多久发一下握手信息
        if (m_sw.ElapsedMilliseconds - m_LastSendHandshake > UdpLibConfig.HandshakeDelay)
        {
            m_LastSendHandshake = m_sw.ElapsedMilliseconds;
            SendHandshake();
            m_retryCount++;
            if (m_retryCount >= UdpLibConfig.HandshakeRetry)
            {
                Status = ClientSessionStatus.ConnectFail;
                Event(UdpClientEvents.ConnectTimeout, null);
            }
        }
    }

    //===========
    public static byte[] GetBytes<T>(byte type, T t)
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

            return data;
        }
        catch (Exception hEx)
        {
            Console.WriteLine(hEx);
        }
        return null;
    }
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
        //UnityEngine.Debug.Log("发送：" + data.Length);
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