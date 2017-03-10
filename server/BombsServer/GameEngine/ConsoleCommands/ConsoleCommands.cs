using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameEngine.Commands
{
    public class ConsoleCommands : CommandManager<ConsoleCommand, ConsoleCommandFunc>
    {
        public ConsoleCommands()
        {
            _commands = new Dictionary<string, ConsoleCommand>();

            this.Add("help", "显示帮助", HandleHelp);
            this.Add("exit", "关闭 程序/服务器", HandleExit);
            this.Add("state", "显示程序状态", HandleStatus);
        }

        #region 添加命令
        public void Add(string name, string description, ConsoleCommandFunc handler)
        {
            this.Add(name, "", description, handler);
        }
        public void Add(string name, string usage, string description, ConsoleCommandFunc handler)
        {
            _commands[name] = new ConsoleCommand(name, usage, description, handler);
        }
        #endregion

        /// <summary>
        /// 循环执行命令
        /// </summary>
        public void Wait()
        {
            // Just wait if not running in a console
            if (!CliUtil.UserInteractive)
            {
                var reset = new ManualResetEvent(false);
                reset.WaitOne();
                return;
            }

            Log.Info("Type 'help' for a list of console commands.");

            while (true)
            {
                var line = Console.ReadLine();
                var args = this.ParseLine(line);
                if (args.Count == 0)
                    continue;

                var command = this.GetCommand(args[0]);
                if (command == null)
                {
                    Log.Info("Unknown command '{0}'", args[0]);
                    continue;
                }

                var result = command.Func(line, args);
                if (result == CommandResult.Break)
                {
                    break;
                }
                else if (result == CommandResult.Fail)
                {
                    Log.Error("Failed to run command '{0}'.", command.Name);
                }
                else if (result == CommandResult.InvalidArgument)
                {
                    Log.Info("Usage: {0} {1}", command.Name, command.Usage);
                }
            }
        }

        protected virtual CommandResult HandleHelp(string command, IList<string> args)
        {
            var maxLength = _commands.Values.Max(a => a.Name.Length);

            Log.Info("Available commands");
            foreach (var cmd in _commands.Values.OrderBy(a => a.Name))
                Log.Info("  {0,-" + (maxLength + 2) + "}{1}", cmd.Name, cmd.Description);

            return CommandResult.Okay;
        }

        protected virtual CommandResult HandleStatus(string command, IList<string> args)
        {
            Log.Status("Memory Usage: {0:N0} KB", Math.Round(GC.GetTotalMemory(false) / 1024f));

            return CommandResult.Okay;
        }

        protected virtual CommandResult HandleExit(string command, IList<string> args)
        {
            CliUtil.Exit(0, false);

            return CommandResult.Okay;
        }
    }

    public class ConsoleCommand : Command<ConsoleCommandFunc>
    {
        public ConsoleCommand(string name, string usage, string description, ConsoleCommandFunc func)
            : base(name, usage, description, func)
        {
        }
    }

    public delegate CommandResult ConsoleCommandFunc(string command, IList<string> args);
}
