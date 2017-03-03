using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;

public class test : MonoBehaviour
{
    private Roooms rooms;
    private Player Current;
    private List<Player> players=new List<Player>();
    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "连接"))
        {
            Connection.Client.ConnectOkEvent += Client_ConnectOkEvent;
            Connection.Client.ConnectAsync("127.0.0.1", 12000);
            Connection.Client.Handlers.LoginEvent += Handlers_LoginEvent;
            Connection.Client.Handlers.QueryEvent += Handlers_QueryEvent;
            Connection.Client.Handlers.JoinRoomEvent += Handlers_JoinRoomEvent;
            Connection.Client.Handlers.PlayerReadyEvent += Handlers_PlayerReadyEvent;
            Connection.Client.Handlers.OtherPlayerJoinEvent += Handlers_OtherPlayerJoinEvent;
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "查询房间"))
        {
            Client_QueryRomme();
        }
        if(rooms!=null&&rooms.datas.Count>0)
        {
            for (int i = 0; i < rooms.datas.Count; i++)
            {
                if (GUI.Button(new Rect(100+ i*100, 200, 100, 50), rooms.datas[i].name))
                {
                    Client_JoinRomme(rooms.datas[i].id);
                }
            }
        }
        if (players != null && players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (GUI.Button(new Rect(100 + i * 100, 250, 100, 50), players[i].Name+"-"+ players[i].State))
                {
                    Client_Ready(players[i].Id);
                }
            }
        }
    }

    private void Handlers_PlayerReadyEvent(int player_id)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].Id== player_id)
            {
                players[i].State = 2;
                return;
            }
        }
    }

    private void Handlers_JoinRoomEvent(int t)
    {
        if (t == 1)
        {
            players.Add(Current);
        }
    }

    private void Handlers_LoginEvent(Player t)
    {
        Current = t;
    }

    private void Handlers_OtherPlayerJoinEvent(PlayerJoin t)
    {
        for (int i = 0; i < t.player.Count; i++)
        {
            bool has = false;
            for (int j = 0; j < players.Count; j++)
            {
               if(t.player[i].Id== players[j].Id)
                {
                    players[j] = t.player[i];
                    has = true;
                    break;
                }
            }
            if(!has)
            {
                players.Add(t.player[i]);
            }
        }
    }

    private void Handlers_QueryEvent(Roooms t)
    {
        rooms = t;
    }

    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");

        LoginRequest model_login = new LoginRequest();
        model_login.UserName = "u1";
        model_login.Password = "54321";
        NetHelp.Send<LoginRequest>(10, model_login, Connection.Client.socket);
    }

    void Client_QueryRomme()
    {
        NetHelp.Send(20,  Connection.Client.socket);
    }

    void Client_JoinRomme(int room_id)
    {
        JoinRoomRequest request = new JoinRoomRequest();
        request.room_id = room_id;
        NetHelp.Send<JoinRoomRequest>(21, request, Connection.Client.socket);
    }

    void Client_Ready(int player_id)
    {
        NetHelp.Send(23, Connection.Client.socket);
    }
}
