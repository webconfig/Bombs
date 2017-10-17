using google.protobuf;
using System.Net.Sockets;
using UnityEngine;

public partial class ServerHandlers : PacketHandlerManager
{
    [PacketHandler(1)]
    public void LoginResult(NetWork client, byte[] datas, ushort start, ushort length)
    {
        Debug.Log("登陆成功");
        CommResult ResponseModel;
        RecvData<CommResult>(datas, out ResponseModel, start, length);
        client.state = 2;
        client.main.LoginOk();
    }
    [PacketHandler(2)]
    public void MsgBack(NetWork client, byte[] datas, ushort start, ushort length)
    {
        WorlData ResponseModel;
        RecvData<WorlData>(datas, out ResponseModel, start, length);
        client.AddMsg(ResponseModel);
    }
}

