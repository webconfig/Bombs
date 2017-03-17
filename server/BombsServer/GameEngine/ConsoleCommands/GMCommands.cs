using System.Collections.Generic;
namespace GameEngine.Commands
{
    public class GMCommands : ConsoleCommands
    {
        public int CurrentGameId;
        public GMCommands()
        {
            this.Add("game", "<seconds>", "查看游戏", HandleShutDown);
            this.Add("list", "<account> <level>", "列出当前GameObject", HandleAuth);
            this.Add("passwd", "<account> <password>", "Changes password of account", HandlePasswd);
        }

        private CommandResult HandleShutDown(string command, IList<string> args)
        {
            if (args.Count < 1)
                return CommandResult.InvalidArgument;
            int CurrentGameId = int.Parse(args[1]);
            int ObjId= int.Parse(args[2]);
            GameManager.Instance.games[CurrentGameId].ShowEntities(ObjId);
            return CommandResult.Okay;
        }

        private CommandResult HandleAuth(string command, IList<string> args)
        {
            
            return CommandResult.Okay;
        }

        private CommandResult HandlePasswd(string command, IList<string> args)
        {
            if (args.Count < 3)
            {
                return CommandResult.InvalidArgument;
            }

            //var accountName = args[1];
            //var password = args[2];

            //if (!LoginServer.Instance.Database.AccountExists(accountName))
            //{
            //    Log.Error("Please specify an existing account.");
            //    return CommandResult.Fail;
            //}

            //LoginServer.Instance.Database.SetAccountPassword(accountName, password);

            //Log.Info("Password change for {0} complete.", accountName);

            return CommandResult.Okay;
        }
    }
}
