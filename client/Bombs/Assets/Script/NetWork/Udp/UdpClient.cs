using LiteNetLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LiteNetLib.Utils;

public class UClient: INetEventListener
{
    private NetManager client;
    private NetPeer udp_server;
    //====================
    private List<byte[]> RecvBuffer_Add=new List<byte[]>();
    private List<byte> RecvBuffer=new List<byte>();
    //====================
    public ServerHandlers Handlers { get; set; }
    //====================
    private NetWork parent;
    //===========
    public int state = 0;

    public UClient(NetWork _parent)
    {
        parent = _parent;
        client = new NetManager(this,"111");
        client.Start();
        client.Connect("192.168.2.91", 7566);
        //state = 10;
        Debug.Log("==Udp开始连接==");
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
        udp_server.Send(data, SendOptions.ReliableOrdered);
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

    public void FixedUpdate()
    {
        client.PollEvents();
    }

    public void Update()
    {
        DealData();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("udp连接成功！");
        state = 10;
        udp_server = peer;
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.LogError("udp连接断开");
        state = -1;
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        throw new NotImplementedException();
    }

    public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
    {
        //===拷贝数据到缓存===
        lock (RecvBuffer_Add)
        {
            RecvBuffer_Add.Add(reader.Data);
        }
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        //throw new NotImplementedException();
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        //throw new NotImplementedException();
    }
}
