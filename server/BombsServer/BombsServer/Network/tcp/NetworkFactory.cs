using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

internal class NetworkFactory
{
    public static NetworkFactory Instance;
    private static TcpListener NetworkListener;
    private int port;
    private ClientHandler Handlers;
    public List<Client> Clients = new List<Client>();
    public object clients_obj = new object();
    public NetworkFactory(int _port)
    {
        port = _port;
        new Thread(new ThreadStart(NetworkStart)).Start();
    }

    public static NetworkFactory GetInstance(int _port)
    {
        return (Instance != null) ? Instance : Instance = Instance = new NetworkFactory(_port);
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
            Log.Info(string.Format("开始监听:{0}", port));
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
            lock (clients_obj)
            {
                Clients.Add(client);
            }
        }
        catch
        {

        }
        NetworkListener.BeginAcceptTcpClient(new AsyncCallback(this.BeginAcceptTcpClient), (object)null);
    }

    public void RemoveClient(Client item)
    {
        lock (clients_obj)
        {
            if (Clients.Contains(item))
            {
                Clients.Remove(item);
            }
        }
    }


    public List<Client> CopyClients()
    {
        List<Client> result = new List<Client>();
        lock(clients_obj)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                result.Add(Clients[i]);
            }
        }
        return result;
    }

}
