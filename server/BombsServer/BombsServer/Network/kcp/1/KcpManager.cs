using ProtoBuf;
using System;
using System.IO;
using System.Threading;

public class KcpManager
{
    public KCPServer server;
    public KcpClientHandler Handlers;

    public KcpManager(int port)
    {

        Handlers = new KcpClientHandler();
        Handlers.AutoLoad();

        UdpLibConfig.MaxTimeNoData = new TimeSpan(0, 0, 5);

        server = new KCPServer(port);
        server.NewClientSession += server_NewClientSession;
        server.CloseClientSession += server_CloseClientSession;
        server.RecvData += server_RecvData;
        for (int i = 0; i < 10; i++)
        {
            server.AddClientKey((uint)i, i);
        }
        server.Start();

    }

    public void Update()
    {
        //Log.Info("111111");
       server.Update();
    }

    void server_RecvData(ClientSession session, byte[] data, int offset, int size)
    {
        //Log.Info("===接受到：" + data.Length);
        offset += 4;
        ushort DataSize = BitConverter.ToUInt16(data, offset);//包长度
        byte command = data[offset + 2];//命令
        ushort MsgSize = (ushort)(DataSize - 3);//消息体长度
                                                //Log.Info("命令:" + command + ",消息体长度" + MsgSize);

        byte[] msgBytes = new byte[MsgSize];
        Buffer.BlockCopy(data, offset+3, msgBytes, 0, msgBytes.Length);
        Handlers.Handle(session,  command, msgBytes);
    }

    void server_NewClientSession(ClientSession session, byte[] data, int offset, int size)
    {
        Console.WriteLine("新客户端:" + session.NetIndex.ToString() + " " + session.EndPoint.ToString());// + " data:" + d.ToString());
    }

    /// <summary>
    /// 客户端掉线
    /// </summary>
    /// <param name="session"></param>
    void server_CloseClientSession(ClientSession session)
    {
        Console.WriteLine("客户端短线:" + session.NetIndex.ToString());
        //lock (this)
        //{
        //    if (session.Info != null)
        //    {
        //        if (clients.ContainsKey(session.Info.Id))
        //        {
        //            clients.Remove(session.Info.Id);
        //        }
        //        if (players.ContainsKey(session.Info.Id))
        //        {
        //            players[session.Info.Id].SetState(-1);
        //        }
        //    }
        //}
    }

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

    //==================
    public static KcpManager Instance;
    public static KcpManager GetInstance(int _port)
    {
        return (Instance != null) ? Instance : Instance = Instance = new KcpManager(_port);
    }
}

