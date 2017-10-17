using google.protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetWork {

    public TcpClient tcp;
    public ServerHandlers tcp_handle;
    public main main;
    public int state = 0;
    public string ip;
    public int port;

    public NetWork(main _main, string _ip,int _port)
    {
        main = _main;
        ip = _ip;
        port = _port;
        tcp_handle = new ServerHandlers();
        tcp_handle.AutoLoad();
        tcp = new TcpClient(tcp_handle, this);
        tcp.ConnectAsync(ip, port);
        tcp.ConnectResultEvent += Tcp_ConnectResultEvent;
        tcp.DisConnectEvent += Tcp_DisConnectEvent;
    }

    private void Tcp_DisConnectEvent()
    {
        state = -1;
    }

    private void Tcp_ConnectResultEvent(bool t)
    {
        if (t)
        {
            Debug.Log("连接成功,开始登陆");
            tcp.Send<LoginRequest>(1, new LoginRequest() { id = game.Instance.id });
            state = 1;
        }
        else
        {
            Debug.Log("连接失败");
            state = -1;
        }
    }

    public List<EntityData> messages=new List<EntityData>();

    public void AddMsg(WorlData data)
    {
        lock (messages)
        {
            messages.AddRange(data.datas);
        }
    }

    public List<EntityData> receive()
    {
        List<EntityData> result=null;
        if (messages.Count > 0)
        {
            lock (messages)
            {
                result = new List<EntityData>(messages);
                messages.Clear();
            }
        }
        return result;
    }

    public void send(Message input)
    {
        tcp.Send<Message>(2, input);
    }

    public void Update()
    {
        if (state == -1)
        {//断线，重新连接
            tcp.ConnectAsync(ip, port);
            state = 0;
        }
        else if(state==1)
        {
            tcp.Update();
        }
    }

}
