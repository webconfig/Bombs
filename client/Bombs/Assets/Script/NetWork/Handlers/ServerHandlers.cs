using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class LoginServerHandlers : PacketHandlerManager
{
    [PacketHandler(10)]
    public void LoginResult(Client client, byte[] datas, ushort start, ushort length)
    {
        Player result;
        RecvData<Player>(datas, out result, start, length);
        if(result!=null)
        {
            UnityEngine.Debug.Log("登陆返回结果：true");
        }
        if (LoginEvent != null)
        {
            LoginEvent(result);
        }
    }


    [PacketHandler(20)]
    public void QueryRoom(Client client, byte[] datas, ushort start, ushort length)
    {
        Roooms result;
        RecvData<Roooms>(datas, out result, start, length);
        if(QueryEvent!=null)
        {
            QueryEvent(result);
        }
    }

    [PacketHandler(21)]
    public void JoinRoom(Client client, byte[] datas, ushort start, ushort length)
    {
        CommResult result;
        RecvData<CommResult>(datas, out result, start, length);
        UnityEngine.Debug.Log("加入房间返回结果：" + result.Result);
        if(JoinRoomEvent!=null)
        {
            JoinRoomEvent(result.Result);
        }
    }

    [PacketHandler(22)]
    public void OtherPlayerJoinRoom(Client client, byte[] datas, ushort start, ushort length)
    {
        PlayerJoin result;
        RecvData<PlayerJoin>(datas, out result, start, length);
        if (OtherPlayerJoinEvent != null)
        {
            OtherPlayerJoinEvent(result);
        }
    }

    [PacketHandler(23)]
    public void PlayerReady(Client client, byte[] datas, ushort start, ushort length)
    {
        CommResult result;
        RecvData<CommResult>(datas, out result, start, length);
        if (PlayerReadyEvent != null)
        {
            PlayerReadyEvent(result.Result);
        }
    }
    public event CallBack<Player> LoginEvent;
    public event CallBack<Roooms> QueryEvent;
    public event CallBack<int> JoinRoomEvent;
    public event CallBack<int> PlayerReadyEvent;
    public event CallBack<PlayerJoin> OtherPlayerJoinEvent;
}
