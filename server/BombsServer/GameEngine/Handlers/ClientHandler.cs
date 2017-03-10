using GameEngine.Network;
using System.Net.Sockets;
using google.protobuf;
using System.IO;
namespace GameEngine
{
    public partial class ClientHandler : PacketHandlerManager
    {
        /// <summary>
        /// 客户端登陆
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(10)]
        public void Login(Client client, SocketAsyncEventArgs args, byte[] datas,ushort start,ushort length)
        {
            LoginRequest model_login;
            RecvData<LoginRequest>(datas, out model_login, start, length);

            //=====读取数据库========
            client.Info = new PlayerInfo();
            client.Info.Id = IdGenerator.Get();
            client.Info.Name = model_login.UserName;

            client.Send<PlayerInfo>(10, client.Info);
            Log.Debug("用户名：{0},密码：{1}", model_login.UserName, model_login.Password);
        }

        ///// <summary>
        ///// 查询所有房间
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="datas"></param>
        //[PacketHandler(20)]
        //public void GetAllGames(Client client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        //{
            
        //}


        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(21)]
        public void JoinGame(Client client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        {
            GameManager.Instance.JoinGame(client);
        }

        [PacketHandler(40)]
        public void Input(Client client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        {
            Input result = new Input();
            using (MemoryStream memoryStream = new MemoryStream(datas, start, length))
            {
                BinaryReader binaryReader = new BinaryReader(memoryStream);
                result.DeSerialization(binaryReader);
            }
            GameManager.Instance.GameInput(client.GameId, result);
        }
    }
}
