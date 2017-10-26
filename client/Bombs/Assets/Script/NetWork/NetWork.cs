using google.protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetWork {

    public TcpClient tcp;
    public UClient udp;
    public KCPClient kcp = null;
    public ServerHandlers tcp_handle;
    public main main;
 
    public string ip;
    public int port;

    public int type = 2;

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
        kcp.Send<Message>(2, input);
    }
    public void send(Messages input)
    {
        kcp.Send<Messages>(3, input);
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
    public void KcpLoginOk()
    {
        kcp.state = 20;
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
            tcp.Send<LoginRequest>(10, new LoginRequest() { id = 0 });
        }
        else if (tcp.state == 20)
        {//登陆成功
            Debug.Log("==Tcp登陆成功==");
            StartUdp(2);
            tcp.state = 21;
        }
        else if (tcp.state == 30)
        {
            main.GameUpdae();
        }
        tcp.Update();

        if (type == 1)
        {
            if (udp != null)
            {
                if (udp.state == 10)
                {//连接成功，开始登陆
                    Debug.Log("==开始登陆==");
                    udp.state = 11;
                    udp.Send<LoginRequest>(11, new LoginRequest() { id = main.entity_id });
                }
                else if (udp.state == 20)
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
        else if(type==2)
        {
            if (kcp != null)
            {
                if (kcp.state == 10)
                {//连接成功，开始登陆
                    Debug.Log("==开始登陆==");
                    kcp.state = 11;
                    kcp.Send<LoginRequest>(12, new LoginRequest() { id = main.entity_id });
                }
                else if (kcp.state == 20)
                {//登陆成功
                    Debug.Log("==Udp登陆成功==");
                    main.CreateObj(main.entity_id);
                    kcp.state = 30;
                }
                else if (kcp.state == 30)
                {
                    main.GameUpdae();
                }
                kcp.Update();
            }
        }
    }

    public void StartUdp(int type)
    {
        if (type == 1)
        {
            if (udp == null)
            {
                udp = new UClient(this);
                udp.Handlers = tcp_handle;
            }
        }
        else if (type == 2)
        {
            kcp = new KCPClient();
            kcp.Event += Client_Event;
            kcp.Connect("192.168.2.91", 7566, (uint)main.entity_id, main.entity_id);
        }
    }

    private void Client_Event(UdpClientEvents arg1, byte[] buf)
    {
        if (arg1 == UdpClientEvents.Recv)
        {
            ushort DataSize = BitConverter.ToUInt16(buf, 0);//包长度
            byte command = buf[2];//命令
            ushort MsgSize = (ushort)(DataSize - 3);//消息体长度
            byte[] msg_datas = new byte[MsgSize];
            Buffer.BlockCopy(buf, 3, msg_datas, 0, MsgSize);
            tcp_handle.Handle(this, command, msg_datas);
        }
        else if (arg1 == UdpClientEvents.ConnectFail)
        {
            Debug.LogError("连接失败");
            //game.DisConn();
            //state = -1;
        }
        else if (arg1 == UdpClientEvents.ConnectTimeout)
        {
            Debug.LogError("连接超时");
            //game.DisConn();
            //state = -1;
        }
        else if (arg1 == UdpClientEvents.Connected)
        {
            kcp.state = 10;//连接成功
            //state = 2;
            //Debug.Log("Connected");
            //dis_start_time = 0;
            //state = 1;
            ////登陆
            //LoginRequest model_login = new LoginRequest();
            //model_login.UserName = UserName;
            //model_login.Password = UserName;
            //cs.client.Send<LoginRequest>(10, model_login);
        }
        else if (arg1 == UdpClientEvents.Close)
        {//挂了
            Debug.Log("=====================挂了");
            //game.DisConn();
            //state = -1;
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
