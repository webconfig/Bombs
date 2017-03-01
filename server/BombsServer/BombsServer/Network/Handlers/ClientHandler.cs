using Comm.Network;
using google.protobuf;
using Comm.Util;
using System;
using Comm.Network.Iocp;

namespace GateServer
{
    public partial class ClientHandler : PacketHandlerManager
    {
        /// <summary>
        /// 客户端登陆
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(0)]
        public void Login(Session client, byte[] datas,ushort start,ushort length)
        {
            ClientLogin model_login;
            RecvData<ClientLogin>(datas, out model_login, start, length);


            ClientResult result = new ClientResult();
            result.Result = true;
            client.Send<ClientResult>(0, result);

            //client.t = new ClientData();
            //client.t.playerId = Guid.NewGuid();
            Log.Debug("用户名：{0},密码：{1}", model_login.UserName, model_login.Password);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(1)]
        public void CreateRoom(Session client, byte[] datas, ushort start, ushort length)
        {
            CreateGameRequest model_create_room;
            RecvData<CreateGameRequest>(datas, out model_create_room, start, length);

            CreateGameResult result = new CreateGameResult();
            result.RoomId = Guid.NewGuid().ToString();
            Log.Debug("用户名：{0},创建房间:{1}", model_create_room.UserName, result.RoomId);

            client.Send<CreateGameResult>(1, result);
        }

        ///// <summary>
        ///// 查询所有房间
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="datas"></param>
        //[PacketHandler(Op.Client.QueryRoom)]
        //public async void QueryRoom(BaseClient<ClientData> client, byte[] datas)
        //{
        //    //QueryRoomRequest model_query_room;
        //    //NetHelp.RecvData<QueryRoomRequest>(datas, out model_query_room);
        //    //Log.Debug("用户名：{0},查询房间", model_query_room.RoomId);

        //    //var grain = GrainClient.GrainFactory.GetGrain<IPairingGrain>(0);
        //    //PairingSummary[] kkk=await grain.GetGames();

        //    //QueryRoomResult result = new QueryRoomResult();
        //    //for (int i = 0; i < kkk.Length; i++)
        //    //{
        //    //    QueryRoomResult.QueryRoomResultItem item = new QueryRoomResult.QueryRoomResultItem();
        //    //    item.RoomId = kkk[i].GameId.ToByteArray();
        //    //    item.RoomName = kkk[i].Name;
        //    //    result.result.Add(item);
        //    //}
        //    //client.Send<QueryRoomResult>(Op.Client.QueryRoom, result);
        //}


        ///// <summary>
        ///// 加入房间
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="datas"></param>
        //[PacketHandler(Op.Client.JoinRoom)]
        //public void JoinRoom(BaseClient<ClientData> client, byte[] datas)
        //{
        //    //JoinRoomRequest model_join_room;
        //    //NetHelp.RecvData<JoinRoomRequest>(datas, out model_join_room);
        //    //Guid RoooId = new Guid(model_join_room.RoomId);

        //    //Log.Debug("房间id：{0}", RoooId.ToString());

        //    //var player = GrainClient.GrainFactory.GetGrain<IPlayerGrain>(client.t.playerId);
        //    //GameState state = await player.JoinRoom(RoooId);

        //    //ClientResult result = new ClientResult();
        //    //result.Result = true;
        //    //client.Send<ClientResult>(Op.Client.JoinRoom, result);
        //}
    }
}
