using google.protobuf;
using System.Collections.Generic;
using Comm.Network.Iocp;
using BombsServer.Network;
using System;

namespace BombsServer.Game
{
    public class Room
    {
        public RooomItem Data;

        public List<Session<Player>> players=new List<Session<Player>>();

        /// <summary>
        /// 玩家加入
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public void AddPlayer(Session<Player> player)
        {
            try
            {
                player.PlayerInfo.RoomId = Data.id;
                //==获取所有玩家
                Players ps = new Players();
                bool has = false;
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].PlayerInfo.Data.Id==player.PlayerInfo.Data.Id)
                    {
                        players[i] = player;
                        has = true;
                    }
                    else
                    {
                        ps.player.Add(players[i].PlayerInfo.Data);
                    }
                }
                if(!has)
                {
                    ps.player.Add(player.PlayerInfo.Data);
                    players.Add(player);
                }

                //==广播
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Send<Players>(22,ps);

                }
            }
            catch (Exception ex)
            {
                Comm.Util.Log.Error("AddPlayer Error:" + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 准备好
        /// </summary>
        /// <param name="player_id"></param>
        /// <returns></returns>
        public void PlayerReady(int player_id)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if(players[i].PlayerInfo.Data.Id==player_id)
                {
                    players[i].PlayerInfo.Data.State = 2;
                }
            }
            //====广播xx准备好了=====
            CommResult result = new CommResult();
            result.Result = player_id;
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Send<CommResult>(23, result);
            }
        }
    }
}
