using google.protobuf;
using System.Net.Sockets;
using UnityEngine;

public partial class ServerHandlers : PacketHandlerManager
{
    [PacketHandler(10)]
    public void TcpLoginResult(NetWork client, byte[] datas)
    {
        Debug.Log("Tcp登陆成功");
        CommResult ResponseModel;
        RecvData<CommResult>(datas, out ResponseModel);
        client.TcpLoginOk(ResponseModel.Result);
    }
    [PacketHandler(11)]
    public void UcpLoginResult(NetWork client, byte[] datas)
    {
        Debug.Log("Udp登陆成功");
        CommResult ResponseModel;
        RecvData<CommResult>(datas, out ResponseModel);
        client.UdpLoginOk();
    }
    [PacketHandler(12)]
    public void KcpLoginResult(NetWork client, byte[] datas)
    {
        Debug.Log("Udp登陆成功");
        CommResult ResponseModel;
        RecvData<CommResult>(datas, out ResponseModel);
        client.KcpLoginOk();
    }
    [PacketHandler(2)]
    public void MsgBack(NetWork client, byte[] datas)
    {
        //Debug.Log("==WorlData==");
        WorlData ResponseModel;
        RecvData<WorlData>(datas, out ResponseModel);
        client.AddMsg(ResponseModel);
    }
}

