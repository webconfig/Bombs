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
        public State state = State.Init;
        public Dictionary<int,Session<Player>> players=new Dictionary<int, Session<Player>>();

        /// <summary>
        /// 玩家加入
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public void AddPlayer(Session<Player> player)
        {
            if (state != State.Init) { return; }
            try
            {
                player.PlayerInfo.RoomId = Data.id;
                //===加入====
                if (players.ContainsKey(player.PlayerInfo.Data.Id))
                {
                    players[player.PlayerInfo.Data.Id] = player;
                }
                else
                {
                    players.Add(player.PlayerInfo.Data.Id, player);
                }

                //====向加入的玩家广播所有玩家=====
                Players p_all = new Players();
                foreach(var item in players)
                {
                    p_all.player.Add(item.Value.PlayerInfo.Data);
                }
                player.Send<Players>(22, p_all);

                //====向已经存在的玩家广播已加入的玩家=====
                Players p_add = new Players();
                p_add.player.Add(player.PlayerInfo.Data);
                foreach (var item in players)
                {
                    if (item.Key != player.PlayerInfo.Data.Id)
                    {
                        item.Value.Send<Players>(22, p_add);
                    }
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
            if (state != State.Init) { return; }
            if (!players.ContainsKey(player_id))
            {
                return;
            }
            players[player_id].PlayerInfo.Data.State = 2;
            //====广播xx准备好了=====
            CommResult result = new CommResult();
            result.Result = player_id;
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Send<CommResult>(23, result);
            }

            bool IsAllReady = true;
            foreach(var item in players)
            {
                if(item.Value.PlayerInfo.Data.State!=2)
                {
                    IsAllReady = false;
                    break;
                }
            }
            if(IsAllReady)
            {//所有玩家都准备好了
                state = State.Start;
            }
        }

        /// <summary>
        /// 开始加载游戏
        /// </summary>
        public void GameStartLoading()
        {

        }

        public enum State
        {
            Init,
            Start,
            Running,
            Over
        }
    }
}
