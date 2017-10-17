using ProtoBuf;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
public class TcpClient
{
    private int BufferSize = 1024;
    private byte[] RecvBuffer;
    public object datas_obj;
    private Socket socket;
    private byte[] buffer;
    public event CallBack<bool> ConnectResultEvent;
    public event CallBack DisConnectEvent;
    private int RunState = 0;
    private int RecvOffset = 0, PackOffset = 0, PackLength = 0;
    public float last_send = -100, Last_recv = -100;
    private bool has_send = false, has_recv = false;
    public ServerHandlers Handlers { get; set; }
    private NetWork parent;
    public TcpClient(ServerHandlers _Handle, NetWork _parent)
    {
        this.buffer = new byte[BufferSize];
        RecvBuffer = new byte[BufferSize];
        Handlers = _Handle;
        parent = _parent;
    }

    #region 连接
    public void ConnectAsync(string host, int port)
    {
        CloseNetwork();
        try
        {
            Debug.Log("开始连接:" + host + "," + port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        }
        catch
        {
            if (ConnectResultEvent != null)
            {
                ConnectResultEvent(false);
            }
            return;
        }
    }
    private void OnConnect(IAsyncResult result)
    {
        try
        {
            Debug.Log("OnConnect");
            socket.EndConnect(result);
            socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ProcessReceive), null);
            if (ConnectResultEvent != null)
            {
                ConnectResultEvent(true);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("OnConnect Error:" + ex.ToString());
            if (ConnectResultEvent != null)
            {
                ConnectResultEvent(false);
            }
            return;
        }
    }
    public void CloseNetwork()
    {
        //Debug.Log("==CloseNetwork==");
        if (socket != null)
        {
            try
            {
                socket.Close();
            }
            catch { }
            socket = null;
        }
    }
    public void Disconnect()
    {
        //Debug.Log("==Disconnect==");
        CloseNetwork();
        if (DisConnectEvent != null)
        {
            DisConnectEvent();
        }
    }
    public void ClearConnEvent()
    {
        ConnectResultEvent = null;
    }
    #endregion

    #region 接收数据
    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="e"></param>
    private void ProcessReceive(IAsyncResult recv)
    {
        //Console.WriteLine("ProcessReceive");
        // 检查远程主机是否关闭连接
        int bytesRead = socket.EndReceive(recv);
        if (bytesRead <= 0)
        {
            Debug.Log("==BytesTransferred等于0退出==" + bytesRead);
            Disconnect();
            return;
        }
        //Socket s = recv.AcceptSocket;
        int total_length = RecvOffset + bytesRead;
        if (total_length > RecvBuffer.Length)
        {//接受的数据超过缓冲区
         //Log.Debug("====接受的数据超过缓冲区====");
            int buff_data_size = RecvOffset - PackOffset;
            int buff_total_size = bytesRead + buff_data_size;
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
        Buffer.BlockCopy(buffer, 0, RecvBuffer, RecvOffset, bytesRead);
        RecvOffset += bytesRead;
        PackLength = RecvOffset - PackOffset;//接受数据的长度
                                             //================解析数据==============
        ushort DataSize, MsgSize;
        byte command;
        while (PackLength >= 3)//接受数据长度必须至少包含2个字节的长度和一个字节的命令
        {
            DataSize = BitConverter.ToUInt16(RecvBuffer, PackOffset);//包长度
            if (DataSize <= PackLength)//包长度大于接受数据长度
            {
                command = RecvBuffer[PackOffset + 2];//命令
                MsgSize = (ushort)(DataSize - 3);//消息体长度
                //Debug.Log("命令：" + command);
                Handlers.Handle(parent, command, RecvBuffer, (ushort)(PackOffset + 3), MsgSize);
                //==========
                PackOffset += DataSize;
                PackLength = RecvOffset - PackOffset;
            }
            else
            {
                break;
            }
        }

        if (RunState != -100)
        {
            try
            {
                socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ProcessReceive), null);
            }
            catch
            {
                Disconnect();
            }
        }
        else
        {
            Debug.Log("断开连接后无法接受数据");
        }
    }
    #endregion

    #region 发送
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
            //============ 发送数据 ============
            socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(ProcessSend), null);
        }
        catch (Exception ex)
        {
            Disconnect();
            UnityEngine.Debug.Log("发送数据错误:" + ex.ToString());
        }
    }
    public void Send(byte type)
    {
        ushort total_length = (ushort)3;
        byte[] total_length_bytes = BitConverter.GetBytes(total_length);
        //消息体结构：消息体长度 + 消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        try
        {
            //============ 发送数据 ============
            socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(ProcessSend), data);
        }
        catch (Exception ex)
        {
            Disconnect();
            UnityEngine.Debug.Log("发送数据错误:" + ex.ToString());
        }
    }
    /// <summary>
    /// 发送成功
    /// </summary>
    /// <param name="sendEventArgs"></param>
    private void ProcessSend(IAsyncResult ar)
    {
        try
        {
            int bytesSent = socket.EndSend(ar);
        }
        catch (Exception e)
        {
            Debug.Log("==ProcessSend Error:" + e.ToString());
            Disconnect();
        }
    }
    #endregion

    public void Update()
    {
        if (has_send) { has_send = false; last_send = Time.time; }
        if (has_recv) { has_recv = false; Last_recv = Time.time; }
        if ((last_send >= 0) && ((Time.time - last_send) >= 3))
        {//没间隔1秒发一个心跳包
         //Debug.Log("=======心跳包=======");
            //byte[] datas = BitConverter.GetBytes(1);
            //Send(0);
            //game.session.Send(game.player_1.name, datas, ConstValue.CSHeartBeatReq, 0);
        }
        if ((Last_recv >= 0) && ((Time.time - Last_recv) >= 7))
        {//居然间隔5秒都没有数据，断线了
            Debug.LogError("=======居然间隔5秒都没有数据，断线了=======");
            Last_recv = -1;
            Disconnect();
        }
    }

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
