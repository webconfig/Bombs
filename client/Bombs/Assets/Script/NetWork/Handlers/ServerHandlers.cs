using google.protobuf;
using System.IO;
using System.Collections.Generic;
public partial class LoginServerHandlers : PacketHandlerManager
{
    [PacketHandler(10)]
    public void LoginResult(Client client, byte[] datas, ushort start, ushort length)
    {
        PlayerInfo result;
        RecvData<PlayerInfo>(datas, out result, start, length);
        if(result!=null)
        {
            UnityEngine.Debug.Log("登陆返回结果：true");
        }
        if (LoginEvent != null)
        {
            LoginEvent(result);
        }
    }
    [PacketHandler(21)]
    public void CreateWorld(Client client, byte[] datas, ushort start, ushort length)
    {
        GameData result;
        RecvData<GameData>(datas, out result, start, length);
        if (CreateWorldEvent != null) { CreateWorldEvent(result); }
    }

    [PacketHandler(22)]
    public void Ready(Client client, byte[] datas, ushort start, ushort length)
    {
        CommResult result;
        RecvData<CommResult>(datas, out result, start, length);
        UnityEngine.Debug.Log("加入房间返回结果：" + result.Result);
    }
    [PacketHandler(31)]
    public void Commands(Client client, byte[] datas, ushort start, ushort length)
    {
        Msgs result;
        RecvData<Msgs>(datas, out result, start, length);
        if (InputsEvent != null) { InputsEvent(result); }
    }

    public event CallBack<PlayerInfo> LoginEvent;
    public event CallBack<Msgs> InputsEvent;
    public event CallBack<GameData> CreateWorldEvent;
}
