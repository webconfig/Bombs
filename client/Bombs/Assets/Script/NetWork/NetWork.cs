using google.protobuf;
using LiteNetLib;
using LiteNetLib.Utils;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetWork {

    public TcpClient tcp;
    public UdpClient udp;
    public ServerHandlers tcp_handle;
    public main main;
 
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

    public void TcpLoginOk(int id)
    {
        main.entity_id = id;
        tcp.state = 20;
    }

    public void UdpLoginOk()
    {
        udp.state = 20;
    }

    public void Update()
    {
        if (tcp.state == -1)
        {//断线，重新连接
            tcp.ConnectAsync(ip, port);
            tcp.state = 0;
        }
        else if (tcp.state == 10)
        {//连接成功，开始登陆
            tcp.state = 11;
            Debug.Log("==Tcp连接成功，开始登陆==");
            tcp.Send<LoginRequest>(10, new LoginRequest() { id = game.Instance.id });
        }
        else if (tcp.state == 20)
        {//登陆成功
            Debug.Log("==Tcp登陆成功==");
            if (udp==null)
            {
                udp = new UdpClient(this);
                udp.Handlers = tcp_handle;
            }
            tcp.state = 21;
        }
        else if (tcp.state == 30)
        {
            main.GameUpdae();
        }
        tcp.Update();

        if (udp != null)
        {
            if (udp.state == 10)
            {//连接成功，开始登陆
                Debug.Log("==开始登陆==");
                udp.state = 11;
                udp.Send<LoginRequest>(11, new LoginRequest() { id = main.entity_id });
            }
            else if(udp.state==20)
            {//登陆成功
                Debug.Log("==Udp登陆成功==");
                main.CreateObj(main.entity_id);
                udp.state = 30;
            }
            else if (udp.state == 30)
            {
                main.GameUpdae();
            }
            udp.Update();
        }

    }

    public void FixedUpdate()
    {
        if (udp != null)
        {
            udp.FixedUpdate();
        }
    }
}
