using System;
using Comm.Util;
using BombsServer.Network;
namespace BombsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Scene.Instance.LoadScene("scene_demo.xml");
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
