using Comm.ConsoleCommands;
using System.Collections.Generic;


namespace GateServer
{
    public class GMCommands : ConsoleCommands
    {
        public GMCommands()
        {
            this.Add("shutdown", "<seconds>", "Orders all servers to shut down", HandleShutDown);
            this.Add("auth", "<account> <level>", "Changes authority level of account", HandleAuth);
            this.Add("passwd", "<account> <password>", "Changes password of account", HandlePasswd);
        }

        private CommandResult HandleShutDown(string command, IList<string> args)
        {
            if (args.Count < 2)
                return CommandResult.InvalidArgument;

            //if (LoginServer.Instance.ChannelClients.Count == 0)
            //{
            //    Log.Error("There are no channel servers currently running.");
            //    return CommandResult.Okay;
            //}

            //// Get time
            //int time;
            //if (!int.TryParse(args[1], out time))
            //    return CommandResult.InvalidArgument;

            //time = Math2.Clamp(60, 1800, time);

            //Send.ChannelShutdown(time);
            //Log.Info("Shutdown request sent to all channel servers.");

            return CommandResult.Okay;
        }

        private CommandResult HandleAuth(string command, IList<string> args)
        {
            if (args.Count < 3)
                return CommandResult.InvalidArgument;

            //int level;
            //if (!int.TryParse(args[2], out level))
            //    return CommandResult.InvalidArgument;

            //if (!LoginServer.Instance.Database.ChangeAuth(args[1], level))
            //{
            //    Log.Error("Failed to change auth. (Does the account exist?)");
            //    return CommandResult.Okay;
            //}

            //Log.Info("Changed auth successfully.");

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
