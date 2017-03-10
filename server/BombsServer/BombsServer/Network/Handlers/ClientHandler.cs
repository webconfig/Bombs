using google.protobuf;
using Comm.Util;
using Comm.Network.Iocp;
using BombsServer.Game;
using BombsServer.Network;
using System.Net.Sockets;

namespace BombsServer
{
    public partial class ClientHandler : PacketHandlerManager<Player>
    {
        /// <summary>
        /// 客户端登陆
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(10)]
        public void Login(Session<Player> client, SocketAsyncEventArgs args, byte[] datas,ushort start,ushort length)
        {
            LoginRequest model_login;
            RecvData<LoginRequest>(datas, out model_login, start, length);

            client.PlayerInfo = new Player();
            //=====读取数据库========
            client.PlayerInfo.Data = new PlayerInfo();
            client.PlayerInfo.Data.Id = IdGenerator.Get();
            client.PlayerInfo.Data.Name = model_login.UserName;

            client.Send<PlayerInfo>(10, client.PlayerInfo.Data);
            Log.Debug("用户名：{0},密码：{1}", model_login.UserName, model_login.Password);
        }

        /// <summary>
        /// 查询所有房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(20)]
        public void GetAllRoom(Session<Player> client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        {
            client.Send<Roooms>(20, GameServer.Instance.RoomMag.GetAllRoomMsg());
            client.Send<Roooms>(20, GameServer.Instance.RoomMag.GetAllRoomMsg());
        }


        /// <summary>
        /// 查询所有房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(21)]
        public void JoinRoom(Session<Player> client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        {
            JoinRoomRequest data;
            RecvData<JoinRoomRequest>(datas, out data, start, length);
            GameServer.Instance.RoomMag.Join(data.room_id, client);
            Log.Debug("房间：{0},加入玩家：{1}", data.room_id, client.PlayerInfo.Data.Name);
        }

        /// <summary>
        /// 玩家准备好
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(23)]
        public void GameMesage(Session<Player> client, SocketAsyncEventArgs args, byte[] datas, ushort start, ushort length)
        {
            GameServer.Instance.RoomMag.PlayerReady(client.PlayerInfo.RoomId, client.PlayerInfo.Data.Id);
            Log.Debug("房间：{0},玩家准备好：{1}", client.PlayerInfo.RoomId, client.PlayerInfo.Data.Name);
        }
    }
}
