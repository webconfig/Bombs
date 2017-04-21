using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;
using System.IO;
public class LockStepManager : MonoBehaviour
{
    private PlayerInfo Current;
    private Game game;
    void Start()
    {
        Application.targetFrameRate = 30;
        game = new Game();
        game.Start();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "连接"))
        {
            Connection.Client.ConnectOkEvent += Client_ConnectOkEvent;
            Connection.Client.Handlers.LoginEvent += Handlers_LoginEvent;
            Connection.Client.Handlers.InputsEvent += Handlers_InputsEvent;
            Connection.Client.Handlers.CreateWorldEvent += Handlers_CreateWorldEvent;
            Connection.Client.ConnectAsync("127.0.0.1", 7001);
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "加入房间"))
        {
            RequestWorld(-1);
        }
    }

    private void Handlers_CreateWorldEvent(GameData t)
    {
        game.CreateWorld(t);
    }

    /// <summary>
    /// 链接到服务器
    /// </summary>
    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");
        LoginRequest model_login = new LoginRequest();
        model_login.UserName = "u1";
        model_login.Password = "54321";
        NetHelp.Send<LoginRequest>(10, model_login, Connection.Client.socket);
    }

    /// <summary>
    /// 登陆结果
    /// </summary>
    /// <param name="t"></param>
    private void Handlers_LoginEvent(PlayerInfo t)
    {
        Current = t;
    }
    void RequestWorld(int room_id)
    {
        NetHelp.Send(21,Connection.Client.socket);
    }

    void Update()
    {
        game.Update();
    }

    private void Handlers_InputsEvent(Msgs msg)
    {
        //Debug.Log("服务器返回：" + seqNum);
        game.AddMessage(msg);

    }
    void OnDrawGizmos()
    {
        if (game != null)
        {
            game.OnDrawGizmos();
        }
    }
}
