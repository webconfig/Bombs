using google.protobuf;
using LiteNetLib;

public partial class UDPClientHandler : PacketHandlerManager<NetPeer>
{
    /// <summary>
    /// 客户端登陆
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    [PacketHandler(11)]
    public void Login(NetPeer client, byte[] datas)
    {
        Log.Info("==udp客户端登陆==");
        LoginRequest request_login;
        RecvData<LoginRequest>(datas, out request_login);
        CommResult response = new CommResult();
        Program.game.UdpLogin(request_login.id, client);
        client.Send<CommResult>(11, response);
    }

    /// <summary>
    /// 上传数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="datas"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    [PacketHandler(2)]
    public void UpMessage(NetPeer client, byte[] datas)
    {
        Message request;
        RecvData<Message>(datas, out request);
        Program.game.AddMessage(request);
    }
}

