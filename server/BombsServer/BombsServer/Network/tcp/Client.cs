using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.IO;
using ProtoBuf;
using System.Threading;

public class Client
{
    public TcpClient client;
    public IPEndPoint ip;

    /// <summary>
    /// 上一次活跃的时间
    /// </summary>
    public DateTime ActiveDateTime;
    public int OutMsTimes;

    public int State = 0;

    private int BufferSize = 1024;
    private List<byte[]> RecvBuffer_Add;
    private List<byte> RecvBuffer;
    private byte[] buffer;
    public CallBack<Client, byte, byte[]> Handle;
    //===========
    private Thread recvThraed;
    //===========

    public Client(TcpClient _client, CallBack<Client, byte, byte[]> _Handle)
    {
        State = 1;
        Handle = _Handle;
        client=_client;
        ip = (IPEndPoint)client.Client.RemoteEndPoint;
        RecvBuffer = new List<byte>();
        RecvBuffer_Add = new List<byte[]>();
        this.buffer = new byte[BufferSize];
        ActiveDateTime = System.DateTime.Now;
        OutMsTimes = 1;
        recvThraed = new Thread(ProcessReceive);
        recvThraed.Start();
    }

    #region 接收
    private void ProcessReceive()
    {
        while (State >= 0)
        {
            // 检查远程主机是否关闭连接
            int length = client.Client.Receive(buffer);
            if (length <= 0)
            {
                Log.Error("==BytesTransferred等于0退出==" + length);
                close();
                return;
            }

            ActiveDateTime = System.DateTime.Now;
            OutMsTimes = 1;
            //Log.Info("===接受到：" + length);
            //===拷贝数据到缓存===
            byte[] new_data = new byte[length];
            Buffer.BlockCopy(buffer, 0, new_data, 0, length);
            lock (RecvBuffer_Add)
            {
                RecvBuffer_Add.Add(new_data);
            }
        }
    }
    /// <summary>
    /// 处理数据
    /// </summary>
    public void DealData()
    {
        if (RecvBuffer_Add.Count > 0)
        {
            lock (RecvBuffer_Add)
            {
                for (int i = 0; i < RecvBuffer_Add.Count; i++)
                {
                    for (int j = 0; j < RecvBuffer_Add[i].Length; j++)
                    {
                        RecvBuffer.Add(RecvBuffer_Add[i][j]);
                    }
                }
                RecvBuffer_Add.Clear();
            }
        }
        if (RecvBuffer.Count >= 3)
        {
            Log.Info("===处理数据====");
            int DataSize = 0, MsgSize;
            byte command;
            while (RecvBuffer.Count >= 3)//接受数据长度必须至少包含2个字节的长度和一个字节的命令
            {
                DataSize = 0;
                BytesToInt(RecvBuffer, 0, ref DataSize);//包长度
                //Debug.Log("包长度:" + DataSize);
                if (DataSize <= RecvBuffer.Count)//包长度大于接受数据长度
                {
                    command = RecvBuffer[2];//命令
                    MsgSize = (ushort)(DataSize - 3);//消息体长度

                    byte[] msgBytes = new byte[MsgSize];
                    RecvBuffer.CopyTo(3, msgBytes, 0, msgBytes.Length);
                    RecvBuffer.RemoveRange(0, DataSize);
                    try
                    {
                        Handle(this, command, msgBytes);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("处理命令：[" + command + "] Error:" + ex.ToString());
                    }
                    if (State == -100)
                    {//退出
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
    public void BytesToInt(List<byte> data, int offset, ref int num)
    {
        for (int i = offset; i < offset + 1; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
    }
    #endregion

    #region 发送
    /// <summary>
    /// 发送心跳包
    /// </summary>
    public void SendHeart()
    {
        if (State == -100) { return; }
        Send(100);
    }
    public void Send<T>(byte type, T t)
    {
        if (State == -100) { return; }
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
        try
        {
            //Log.Info("发送：" + data.Length);
            client.Client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback),null);
        }
        catch (Exception ex)
        {
            close();
            Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    public void Send(byte type)
    {
        if (State == -100) { return; }

        ushort total_length = (ushort)3;
        byte[] total_length_bytes = BitConverter.GetBytes(total_length);
        //消息体结构：消息体长度 + 消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        //============ 发送数据 ============
        try
        {
            client.Client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), null);
        }
        catch (Exception ex)
        {
            close();
            Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            client.Client.EndSend(ar);
        }
        catch (SocketException ex)
        { }
    }
    #endregion

    #region 关闭
    public void close()
    {
        if (State != -100)
        {
            close_self();
            TcpManager.Instance.RemoveClient(this);
        }
    }
    public void close_self()
    {
        Log.Info("【Client】--被动关闭");
        State = -100;
        client.Close();
    }
    #endregion
}

