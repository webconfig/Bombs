using System;
namespace GameEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GameManager.Instance.Run();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "An exception occured while starting the server.");
                CliUtil.Exit(1);
            }
        }
    }
}
