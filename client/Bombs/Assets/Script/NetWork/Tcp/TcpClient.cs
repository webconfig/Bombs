﻿using ProtoBuf;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class TcpClient
{
    private int BufferSize = 1024;
    private List<byte[]> RecvBuffer_Add;
    private List<byte> RecvBuffer;
    private Socket socket;
    private byte[] buffer;
    public event CallBack<bool> ConnectResultEvent;
    public event CallBack DisConnectEvent;
    private int RunState = 0;

    private bool has_send = false, has_recv = false;
    private ServerHandlers Handlers { get; set; }
    private NetWork parent;

    //===========
    private Thread recvThraed;
    //===========
    public int state = 0;

    public TcpClient(ServerHandlers _Handle, NetWork _parent)
    {
        this.buffer = new byte[BufferSize];
        RecvBuffer = new List<byte>();
        RecvBuffer_Add = new List<byte[]>();
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
            socket.NoDelay = true;
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
            state = 10;
            recvThraed = new Thread(ProcessReceive);
            recvThraed.Start();
            if (ConnectResultEvent != null)
            {
                ConnectResultEvent(true);
            }
        }
        catch (Exception ex)
        {
            state = -1;
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
        state = -1;
        if (recvThraed != null)
        {
            recvThraed.Abort();
            recvThraed = null;
        }
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
    private void ProcessReceive()
    {
        while (state > 0)
        {
            // 检查远程主机是否关闭连接
            int bytesRead = socket.Receive(buffer);
            if (bytesRead <= 0)
            {
                Debug.Log("==BytesTransferred等于0退出==" + bytesRead);
                Disconnect();
                return;
            }

            //===拷贝数据到缓存===
            byte[] new_data = new byte[bytesRead];
            Buffer.BlockCopy(buffer, 0, new_data, 0, bytesRead);
            lock (RecvBuffer_Add)
            {
                RecvBuffer_Add.Add(new_data);
            }
        }
    }
    /// <summary>
    /// 处理数据
    /// </summary>
    private void DealData()
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
                    Handlers.Handle(parent, command, msgBytes);
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
        socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(send_back), null);
    }
    private void send_back(IAsyncResult ar)
    {
        try
        {
            int bytesSent = socket.EndSend(ar);
        }
        catch
        {

        }
    }

    //public void Send(byte type)
    //{
    //    ushort total_length = (ushort)3;
    //    byte[] total_length_bytes = BitConverter.GetBytes(total_length);
    //    //消息体结构：消息体长度 + 消息体
    //    byte[] data = new byte[total_length];
    //    total_length_bytes.CopyTo(data, 0);
    //    data[2] = type;
    //    send_datas_add.Add(data);
    //}
    #endregion

    public void Update()
    {
        DealData();
        //if (has_send) { has_send = false; last_send = Time.time; }
        //if (has_recv) { has_recv = false; Last_recv = Time.time; }
        //if ((last_send >= 0) && ((Time.time - last_send) >= 3))
        //{//没间隔1秒发一个心跳包
        // //Debug.Log("=======心跳包=======");
        //    //byte[] datas = BitConverter.GetBytes(1);
        //    //Send(0);
        //    //game.session.Send(game.player_1.name, datas, ConstValue.CSHeartBeatReq, 0);
        //}
        //if ((Last_recv >= 0) && ((Time.time - Last_recv) >= 7))
        //{//居然间隔5秒都没有数据，断线了
        //    Debug.LogError("=======居然间隔5秒都没有数据，断线了=======");
        //    Last_recv = -1;
        //    Disconnect();
        //}
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
public class BuffItem
{
    public byte[] datas;
    public bool Over;
    public int total_length;
}
