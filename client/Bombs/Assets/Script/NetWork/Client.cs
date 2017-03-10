using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using google.protobuf;

public  class Client
{
    private const int BufferSize = 1024;
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
            if(bytesReceived>1000)
            {
                int pppp = 1;
            }
            int total_length = CurrentOffset + bytesReceived;
            if (total_length > RecvBuffer.Length)
            {//接受的数据超过缓冲区
                Debug.Log("==接受的数据超过缓冲区==");
                Byte[] newBuffer = new Byte[total_length];
                Buffer.BlockCopy(RecvBuffer, CurrentOffset, newBuffer, 0, CurrentLength);
                CurrentOffset = 0;
                RecvBuffer = newBuffer;
            }

            //===拷贝数据到缓存===
            Buffer.BlockCopy(buffer, 0, RecvBuffer, CurrentOffset, bytesReceived);
            Debug.Log("add---"+CurrentOffset + "-" + bytesReceived);
            //CurrentOffset += (ushort)bytesReceived;
            CurrentLength += (ushort)bytesReceived;
            ushort DataSize, MsgSize;
            byte command;
            while (CurrentLength > 3)
            {
                DataSize = BitConverter.ToUInt16(RecvBuffer, CurrentOffset);
                if(DataSize == 0)
                {
                    Debug.Log("===============recv error=============");
                    break;
                }
                if (DataSize <= CurrentLength)
                {
                    command = RecvBuffer[CurrentOffset+2];//命令
                    MsgSize = (ushort)(DataSize - 3);
                    Handlers.Handle(this, command, RecvBuffer, (ushort)(DataSize + 3), MsgSize);
                    //---删除----
                    CurrentOffset += DataSize;
                    CurrentLength -= DataSize;
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

