using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.IO;
using ProtoBuf;

public class Client
{
    public TcpClient client;
    public IPEndPoint ip;
    public NetworkStream stream;

    /// <summary>
    /// 上一次活跃的时间
    /// </summary>
    public DateTime ActiveDateTime;
    public int OutMsTimes;

    private int State = 0;

    private int BufferSize = 1024;
    private byte[] StreamBuffer;
    private byte[] RecvBuffer;
    private int RecvOffset = 0;
    private int PackOffset = 0;
    private int PackLength = 0;

    public CallBack<Client, byte, byte[], ushort, ushort> Handle;

    public Client(TcpClient _client, CallBack<Client, byte, byte[], ushort, ushort> _Handle)
    {
        State = 1;
        Handle = _Handle;
        client=_client;
        ip = (IPEndPoint)client.Client.RemoteEndPoint;
        stream = client.GetStream();
        StreamBuffer = new byte[BufferSize];
        RecvBuffer = new byte[BufferSize];
        ActiveDateTime = System.DateTime.Now;
        OutMsTimes = 1;
        BeginRead();
    }

    #region 接收
    private void BeginRead()
    {
        try
        {
            if (this.stream == null || !this.stream.CanRead)
            {
                Log.Error("网络流不可读 关闭");
                close();
                return;
            }

            stream.BeginRead(StreamBuffer, 0, BufferSize, new AsyncCallback(OnReceiveCallback), null);
        }
        catch (Exception ex)
        {
            Log.Error("[Client]: BeginRead() Exception" + ex);
            close();
        }
    }
    private void OnReceiveCallback(IAsyncResult ar)
    {
        int length = 0;
        #region 网络异常判断
        try
        {
            length = stream.EndRead(ar);
        }
        catch
        {
            close();
            return;
        }
        if (length <= 0)
        {
            close();
            return;
        }
        #endregion
        try
        {
            ActiveDateTime = System.DateTime.Now;
            OutMsTimes = 1;

            #region 接收数据
            int total_length = RecvOffset + length;
            if (total_length > RecvBuffer.Length)
            {//接受的数据超过缓冲区
             //Log.Debug("====接受的数据超过缓冲区====");
                int buff_data_size = RecvOffset - PackOffset;
                int buff_total_size = length + buff_data_size;
                if (buff_total_size > BufferSize)
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
            Buffer.BlockCopy(StreamBuffer, 0, RecvBuffer, RecvOffset, length);
            RecvOffset += length;
            PackLength = RecvOffset - PackOffset;//接受数据的长度
            #endregion

            #region 解析数据
            ushort DataSize, MsgSize;
            byte command;
            while (PackLength >= 3)//接受数据长度必须至少包含2个字节的长度和一个字节的命令
            {
                DataSize = BitConverter.ToUInt16(RecvBuffer, PackOffset);//包长度
                if (DataSize <= PackLength)//包长度大于接受数据长度
                {
                    command = RecvBuffer[PackOffset + 2];//命令
                    MsgSize = (ushort)(DataSize - 3);//消息体长度
                    //Log.Info("新命令：" + command);
                    try
                    {
                        Handle(this, command, RecvBuffer, (ushort)(PackOffset + 3), MsgSize);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("处理命令：[" + command + "] Error:" + ex.ToString());
                    }
                    if (State == -100)
                    {//退出
                        return;
                    }
                    //==========
                    PackOffset += DataSize;
                    PackLength = RecvOffset - PackOffset;
                }
                else
                {
                    break;
                }
            }
            #endregion
        }
        catch
        {
            if (State == -100)
            {
                return;
            }
        }

        //Debug.Info("[接受]--Over");
        BeginRead();
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
            stream.Write(data, 0, data.Length);
            stream.Flush();
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
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        catch (Exception ex)
        {
            close();
            Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    #endregion

    #region 关闭
    public void close()
    {
        if (State != -100)
        {
            close_self();
            NetworkFactory.Instance.RemoveClient(this);
        }
    }
    public void close_self()
    {
        Log.Info("【Client】--被动关闭");
        State = -100;
        if (stream != null)
        {
            this.stream.Close();
            this.stream = null;
        }
    }
    #endregion
}

