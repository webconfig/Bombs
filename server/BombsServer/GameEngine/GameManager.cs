using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine.Network;
using GameEngine.Commands;
namespace GameEngine
{
    public class GameManager
    {
        public static readonly GameManager Instance = new GameManager();
        private ServerIOCP server;
        public Dictionary<int, Game> games = new Dictionary<int, Game>();

        public void Run()
        {
            Game game = new Game();
            game.Start();
            games.Add(0, game);

            //开启服务器
            ClientHandler Handlers = new ClientHandler();
            Handlers.AutoLoad();
            server = new ServerIOCP(2000, 1024, Handlers);
            server.Start("127.0.0.1", 7001);
            //GM操作
            var commands = new GMCommands();
            commands.Wait();
        }

        public void JoinGame(int game_id, Client client)
        {
            games[game_id].PlayrJoin(client);
        }

        public void GameInput(int game_id, int client_id,int index, int input)
        {
            games[game_id].AddMessage(client_id, index,input);
        }
    }
}
