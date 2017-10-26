using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

internal class TcpManager
{
    public static TcpManager Instance;
    private static TcpListener NetworkListener;
    private int port;
    private ClientHandler Handlers;
    public List<Client> Clients = new List<Client>();
    public List<Client> Clients_Add = new List<Client>();

    public static TcpManager GetInstance(int _port)
    {
        return (Instance != null) ? Instance : Instance = Instance = new TcpManager(_port);
    }

    public TcpManager(int _port)
    {
        port = _port;
        new Thread(new ThreadStart(NetworkStart)).Start();
    }


    private void NetworkStart()
    {
        try
        {
            //开启服务器
            Handlers = new ClientHandler();
            Handlers.AutoLoad();

            NetworkListener = new TcpListener(new System.Net.IPEndPoint(0, port));
            NetworkListener.Start();
            Log.Info(string.Format("tcp 端口:{0}", port));
            NetworkListener.BeginAcceptTcpClient(new AsyncCallback(BeginAcceptTcpClient), (object)null);
        }
        catch (Exception ex)
        {
            Log.Error("NetworkStart:" + ex);
        }
    }

    private void BeginAcceptTcpClient(IAsyncResult ar)
    {
        try
        {
            TcpClient tcpClient = NetworkListener.EndAcceptTcpClient(ar);
            Client client = new Client(tcpClient, Handlers.Handle);
            Log.Info("【ClientManager】--添加客户端:" + client.ip.Address.ToString() + ":" + client.ip.Port.ToString());
            lock (Clients_Add)
            {
                Clients_Add.Add(client);
            }
        }
        catch
        {

        }
        NetworkListener.BeginAcceptTcpClient(new AsyncCallback(this.BeginAcceptTcpClient), (object)null);
    }

    public void RemoveClient(Client item)
    {
        item.State = -100;
    }


    public void Update()
    {
        if (Clients_Add.Count > 0)
        {
            lock (Clients_Add)
            {
                Clients.AddRange(Clients_Add);
                Clients_Add.Clear();
            }
        }


        for (int i = 0; i < Clients.Count; i++)
        {
            if (Clients[i].State == -100)
            {
                Clients.RemoveAt(i);
                i--;
            }
            else
            {
                Clients[i].DealData();
            }
        }

    }

}
