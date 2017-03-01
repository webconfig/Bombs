using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;

public class test : MonoBehaviour
{
    private List<QueryRoomResult.QueryRoomResultItem> rooms;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "连接"))
        {
            Connection.Client.ConnectOkEvent += Client_ConnectOkEvent;
            Connection.Client.ConnectAsync("127.0.0.1", 12000);
            Connection.Client.Handlers.QueryEvent += Handlers_QueryEvent;
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "创建房间"))
        {
            Client_CreateRomme();
        }
        if (GUI.Button(new Rect(300, 150, 100, 50), "查询房间"))
        {
            //Client_QueryRomme();
        }
        if(rooms!=null&&rooms.Count>0)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (GUI.Button(new Rect(100+ i*100, 250, 100, 50), rooms[i].RoomName))
                {
                    //Client_JoinRomme(rooms[i].RoomId);
                }
            }
        }
    }

    private void Handlers_QueryEvent(List<QueryRoomResult.QueryRoomResultItem> t)
    {
        rooms = t;
    }

    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");
        ClientLogin model_login = new ClientLogin();
        model_login.UserName = "kkk";
        model_login.Password = "54321";
        NetHelp.Send<ClientLogin>(0, model_login, Connection.Client.socket);
    }

    void Client_CreateRomme()
    {
        CreateGameRequest model_create_room = new CreateGameRequest();
        model_create_room.UserName = "kkk";
        NetHelp.Send<CreateGameRequest>(1, model_create_room, Connection.Client.socket);
    }

    //void Client_QueryRomme()
    //{
    //    QueryRoomRequest model_query_room = new QueryRoomRequest();
    //    model_query_room.RoomId = "kkk";
    //    NetHelp.Send<QueryRoomRequest>(Op.Client.QueryRoom, model_query_room, Connection.Client.socket);
    //}

    //void Client_JoinRomme(byte[]  roomid)
    //{
    //    JoinRoomRequest model_join_room = new JoinRoomRequest();
    //    model_join_room.RoomId = roomid;
    //    NetHelp.Send<JoinRoomRequest>(Op.Client.JoinRoom, model_join_room, Connection.Client.socket);
    //}
}
