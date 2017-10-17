using System.Net.Sockets;
using google.protobuf;
using System;
using System.IO;
using System.Collections.Generic;

public partial class ClientHandler : PacketHandlerManager
{
    /// <summary>
    /// 客户端登陆
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    [PacketHandler(1)]
    public void Login(Client client, byte[] datas, ushort start, ushort length)
    {
        Log.Info("==客户端登陆==");
        LoginRequest request_login;
        RecvData<LoginRequest>(datas, out request_login, start, length);
        CommResult response = new CommResult();
        Program.game.ClientLogin(request_login.id, client);
        client.Send<CommResult>(1, response);
    }

    /// <summary>
    /// 上传数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    [PacketHandler(2)]
    public void UpMessage(Client client, byte[] datas, ushort start, ushort length)
    {
        Message request;
        RecvData<Message>(datas, out request, start, length);
        Program.game.AddMessage(request);
    }
}

