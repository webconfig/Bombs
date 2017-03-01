using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class LoginServerHandlers : PacketHandlerManager
{
    [PacketHandler(0)]
    public void LoginResult(Client client, byte[] datas, ushort start, ushort length)
    {
        ClientResult model_login;
        RecvData<ClientResult>(datas, out model_login, start, length);
        UnityEngine.Debug.Log("登陆返回结果：" + model_login.Result);
    }


    [PacketHandler(1)]
    public void CreateRoom(Client client, byte[] datas, ushort start, ushort length)
    {
        CreateGameResult result;
        RecvData<CreateGameResult>(datas, out result, start, length);
        UnityEngine.Debug.Log("创建房间：" + result.RoomId);
    }
    //[PacketHandler(Op.Client.CreateRoom)]
    //public void CreateRoomResult(Client client, byte[] datas)
    //{
    //    CreateGameResult model_login;
    //    NetHelp.RecvData<CreateGameResult>(datas, out model_login);
    //    UnityEngine.Debug.Log("创建房间结果：" + model_login.RoomId);
    //}

    //[PacketHandler(Op.Client.QueryRoom)]
    //public void QueryRoom(Client client, byte[] datas)
    //{
    //    QueryRoomResult model_query_result;
    //    NetHelp.RecvData<QueryRoomResult>(datas, out model_query_result);
    //    for (int i = 0; i < model_query_result.result.Count; i++)
    //    {
    //        UnityEngine.Debug.Log(string.Format("房间名:{0}",  model_query_result.result[i].RoomName));
    //    }
    //    if(QueryEvent!= null)
    //    {
    //        QueryEvent(model_query_result.result);
    //    }

    //}

    //[PacketHandler(Op.Client.JoinRoom)]
    //public void JoinRoom(Client client, byte[] datas)
    //{
    //    ClientResult model_join_result;
    //    NetHelp.RecvData<ClientResult>(datas, out model_join_result);
    //    UnityEngine.Debug.Log("加入房间返回结果：" + model_join_result.Result);
    //}


    public event CallBack<List<QueryRoomResult.QueryRoomResultItem>> QueryEvent;
}
