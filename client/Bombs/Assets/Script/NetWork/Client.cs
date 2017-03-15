using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using google.protobuf;

public  class Client
{
    private int BufferSize = 1024;
    private byte[] RecvBuffer;
    private Dictionary<int, Queue<Packet>> datas;
    public object datas_obj;
    public Socket socket;
    private byte[] buffer;
    public ConnectionState State { get; private set; }
    public event CallBack ConnectOkEvent;
    public LoginServerHandlers Handlers { get; set; }
    private ushort CurrentOffset=0,CurrentLength;
    public Client()
    {
        this.datas = new Dictionary<int, Queue<Packet>>();
        this.buffer = new byte[BufferSize];
        Handlers = new LoginServerHandlers();
        Handlers.AutoLoad();
        RecvBuffer = new byte[BufferSize];
    }

    #region 连接

    public bool Connect(string host, int port)
    {
        if (this.State == ConnectionState.Connected)
            this.Disconnect();

        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.State = ConnectionState.Connecting;

        var success = false;

        try
        {
            this.socket.Connect(host, port);
            //this.HandShake();
            this.BeginReceive();
            success = true;
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if (!success)
            this.State = ConnectionState.Disconnected;
        else
            this.State = ConnectionState.Connected;

        return success;
    }

    public void ConnectAsync(string host, int port)
    {
        if (this.State == ConnectionState.Connected)
            this.Disconnect();
        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.buffer = new byte[BufferSize];
        this.State = ConnectionState.Connecting;
        this.socket.BeginConnect(host, port, this.OnConnect, null);
    }

    private void OnConnect(IAsyncResult result)
    {
        var success = false;

        try
        {
            this.socket.EndConnect(result);
            //this.HandShake();
            this.BeginReceive();
            success = true;
            if(ConnectOkEvent!=null)
            {
                ConnectOkEvent();
                ConnectOkEvent = null;
            }
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if (!success)
            this.State = ConnectionState.Disconnected;
        else
            this.State = ConnectionState.Connected;
    }
    public void Disconnect()
    {
        if (this.socket == null)
            return;

        try
        {
            this.socket.Disconnect(false);
        }
        catch { }

        try
        {
            this.socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        try
        {
            this.socket.Close();
        }
        catch { }

        this.State = ConnectionState.Disconnected;
    }
    #endregion

    #region 接收数据
    private void BeginReceive()
    {
        this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
    }
    private int RecvOffset = 0,PackOffset=0, PackLength=0;
    private void OnReceive(IAsyncResult result)
    {
        try
        {
            int bytesReceived = this.socket.EndReceive(result);
            if (bytesReceived == 0)
            {
                this.State = ConnectionState.Disconnected;
                return;
            }

            int total_length = RecvOffset + bytesReceived;
            if (total_length > RecvBuffer.Length)
            {//接受的数据超过缓冲区
                //Log.Debug("====接受的数据超过缓冲区====");
                int buff_data_size = RecvOffset - PackOffset;
                int buff_total_size = bytesReceived + buff_data_size;
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
            Buffer.BlockCopy(buffer, 0, RecvBuffer, RecvOffset, bytesReceived);
            RecvOffset += bytesReceived;
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
                    Handlers.Handle(this, command, RecvBuffer, (ushort)(PackOffset + 3), MsgSize);
                    //==========
                    PackOffset += DataSize;
                    PackLength = RecvOffset - PackOffset;
                }
                else
                {
                    break;
                }
            }
            this.BeginReceive();
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    public List<Packet> GetPacketsFromQueue(int entity_id)
    {
        lock (this.datas_obj)
        {
            if (datas.ContainsKey(entity_id))
            {
                var result = new List<Packet>();
                result.AddRange(datas[entity_id]);
                datas[entity_id].Clear();
                return result;
            }
            return null;
        }
    }

    public string GetLocalIp()
    {
        if (socket == null)
            return "?";

        return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
    }
}

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
}

