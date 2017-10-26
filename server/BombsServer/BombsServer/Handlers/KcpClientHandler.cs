using google.protobuf;
using LiteNetLib;

public partial class KcpClientHandler : PacketHandlerManager<ClientSession>
{
    /// <summary>
    /// 客户端登陆
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    [PacketHandler(12)]
    public void Login(ClientSession client, byte[] datas)
    {
        Log.Info("==kcp客户端登陆==");
        LoginRequest request_login;
        RecvData<LoginRequest>(datas, out request_login);
        CommResult response = new CommResult();
        Program.game.KcpLogin(request_login.id, client);
        client.Send<CommResult>(12, response);
    }

    /// <summary>
    /// 上传数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    [PacketHandler(2)]
    public void UpMessage(ClientSession client, byte[] datas)
    {
        Message request;
        RecvData<Message>(datas, out request);
        Program.game.AddMessage(request);
    }

    /// <summary>
    /// 上传数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    [PacketHandler(3)]
    public void UpMessages(ClientSession client, byte[] datas)
    {
        Messages request;
        RecvData<Messages>(datas, out request);
        Program.game.AddMessage(request);
    }
}

