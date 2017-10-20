using System;
using System.Diagnostics;


class Program
{
    public static Game game;
    static void Main(string[] args)
    {
        try
        {
            Log.LogFile = string.Format("log/{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss"));
            Console.Title = "测试服务器--" + DateTime.Now.ToString();
            TcpManager.GetInstance(5991);
            UdpManager.GetInstance(7566);
            game = new Game();
            game.Run();
            Process.GetCurrentProcess().WaitForExit();
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "An exception occured while starting the server.");
            CliUtil.Exit(1);
        }
    }
}

