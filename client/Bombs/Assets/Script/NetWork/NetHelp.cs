using System;
using System.IO;
using ProtoBuf;
using System.Net.Sockets;
using System.Collections.Generic;

public class NetHelp
{

    public static void Send<T>(byte type, T t, Socket socket)
    {
        byte[] msg;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, t);
            msg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(msg, 0, msg.Length);
        }

        ushort total_length = (ushort)(msg.Length +3);
        byte[] total_length_bytes = BitConverter.GetBytes(total_length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[total_length];
        total_length_bytes.CopyTo(data, 0);
        data[2] = type;
        msg.CopyTo(data, 3);

        try
        {
            socket.Send(data);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("发送数据错误:" + ex.ToString());
        }
    }




    public static void RecvData<T>(byte[] data, out T t)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(data, 0, data.Length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static int BytesToInt(List<byte> data, int offset, ref int num)
    {
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static int BytesToInt(List<byte> data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
}

