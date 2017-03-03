using System;
using Comm.Util;
using BombsServer.Game;

namespace BombsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GameServer.Instance.Run();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "An exception occured while starting the server.");
                CliUtil.Exit(1);
            }
        }
    }
}
