using System.Collections.Generic;
using System.Net.Sockets;
using System;

internal class ClientManager
{
    private static ClientManager Instance = new ClientManager();
    public List<Client> Clients = new List<Client>();
    public object clients_obj = new object();
    static ClientManager()
    {
    }

    public ClientManager(){}

    public static ClientManager GetInstance()
    {
        return Instance;
    }

}
