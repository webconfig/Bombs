using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

public class UdpManager
{
    public static UdpManager Instance;
    private Thread pollEventThread;
    private NetManager server;
    private bool running = true;
    public UDPClientHandler Handlers;
    public CallBack<NetPeer> onPeerConnected;
    public CallBack<NetPeer> onPeerDisconnected;
    public List<NetPeer> clients = new List<NetPeer>();
    public List<NetPeer> clients_add = new List<NetPeer>();

    public static UdpManager GetInstance(int _port)
    {
        return (Instance != null) ? Instance : Instance = Instance = new UdpManager(_port);
    }

    public UdpManager(int port)
    {
        //开启服务器
        Handlers = new UDPClientHandler();
        Handlers.AutoLoad();

        EventBasedNetListener listener = new EventBasedNetListener();
        server = new NetManager(listener, 100 , "111");
        server.Start(port);
        Log.Info("upd 端口:" + port);

        listener.PeerConnectedEvent += peer =>
        {
            peer.Handle = Handlers.Handle;
            Log.Info("新udp连接：{0}", peer.EndPoint);
            lock (clients_add)
            {
                clients_add.Add(peer);
            }
            onPeerConnected?.Invoke(peer);
        };
        listener.NetworkReceiveEvent += (fromPeer, dataReader) =>
        {
            lock(fromPeer.RecvBuffer_Add)
            {
                fromPeer.RecvBuffer_Add.Add(dataReader.Data);
            }
        };
        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine("peer disconnectd ===>", peer.EndPoint); // Show peer ip
            onPeerDisconnected?.Invoke(peer);
        };

        pollEventThread = new Thread(pollEvent);
        pollEventThread.Start();
    }


    public void Update()
    {
        if(clients_add.Count>0)
        {
            lock(clients_add)
            {
                clients.AddRange(clients_add);
                clients_add.Clear();
            }
        }

        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].Update();
        }
    }

    private void pollEvent()
    {
        while (running)
        {
            server.PollEvents();



            Thread.Sleep(1);
        }
    }
}
