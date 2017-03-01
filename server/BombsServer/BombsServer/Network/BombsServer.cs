using GateServer;
using System.Net;
using System;
using Comm.Network.Iocp;
using Comm.Util;

namespace BombsServer.Network
{
    public class GameServer
    {
        public static readonly GameServer Instance = new GameServer();
        private bool _running = false;
        public ServerIOCP<Session> Server;
        public GateConf Conf { get; private set; }
        private GameServer()
        {
            this.Server = new ServerIOCP<Session>();
            Server.Handlers = new ClientHandler();
            Server.Handlers.AutoLoad();
        }
        public void Run()
        {
            if (_running)
                throw new Exception("服务器正在运行...");
            //标题栏
            CliUtil.WriteHeader("GateServer " + DateTime.Now.ToString(), ConsoleColor.Magenta);
            CliUtil.LoadingTitle();
            //配置文件
            this.LoadConf(this.Conf = new GateConf());

            //// Database
            //this.InitDatabase(this.Database = new LoginDb(), this.Conf);

            //// Check if there are any updates
            //this.CheckDatabaseUpdates();

            //// Data
            //this.LoadData(DataLoad.LoginServer, false);

            //// Localization
            //this.LoadLocalization(this.Conf);

            //// Web API
            //this.LoadWebApi();

            //// Scripts
            //this.LoadScripts();

            //开启服务器
            Server.Start(IPAddress.Any, this.Conf.Gate.Port);

            CliUtil.RunningTitle();
            _running = true;

            //GM操作
            var commands = new GMCommands();
            commands.Wait();
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="conf"></param>
        public void LoadConf(BaseConf conf)
        {
            Log.Info("读取配置...");

            try
            {
                conf.Load();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "读取错误. ({0})", ex.Message);
                CliUtil.Exit(1);
            }
        }
    }
}
