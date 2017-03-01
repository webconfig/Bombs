using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Connection
{
    public static Client Client = new Client();

    //public static string AccountName;
    //public static long SessionKey;
    //public static List<ServerInfo> Servers = new List<ServerInfo>();
    //public static List<CharacterInfo> Characters = new List<CharacterInfo>();
    //public static CharacterInfo SelectedCharacter;
    //public static long ControllingEntityId;
    //public static Dictionary<long, Entity> Entities = new Dictionary<long, Entity>();

    public static void Reset()
    {
        Client.Disconnect();

        //AccountName = null;
        //SessionKey = 0;
        //Servers.Clear();
        //Characters.Clear();
    }
}