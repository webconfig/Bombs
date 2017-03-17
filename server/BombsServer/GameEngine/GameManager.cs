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
        public Dictionary<int, Game2> games = new Dictionary<int, Game2>();

        public void Run()
        {
            Game2 game = new Game2();
            game.start();
            games.Add(0, game);


            ////开启服务器
            ClientHandler Handlers = new ClientHandler();
            Handlers.AutoLoad();
            server = new ServerIOCP(2000, 1024, Handlers);
            server.Start("127.0.0.1", 7001);
            //GM操作
            var commands = new GMCommands();
            commands.Wait();
        }

        public void JoinGame(Client client)
        {
            client.GameId = 0;
            games[0].connect(client);
        }

        public void GameInput(int game_id,Input input)
        {
            games[game_id].AddMessage(input);
        }
    }
}
