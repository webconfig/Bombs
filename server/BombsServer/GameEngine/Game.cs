using System.Collections.Generic;
using GameEngine.Network;
using GameEngine.Script;
using Humper;
using google.protobuf;
using UnityEngine;
namespace GameEngine
{
    public class Game
    {
        #region 服务器代码
        private bool _run = false;
        /// <summary>
        /// 玩家集合
        /// </summary>
        public Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public Dictionary<int, Client> clients_add = new Dictionary<int, Client>();
        private int tickRate = 20;
        private GameData old_data;
        #endregion

        #region 客户端代码
        /// <summary>
        /// 玩家集合
        /// </summary>
        public Dictionary<int, PlayerControl> players = new Dictionary<int, PlayerControl>();
        /// <summary>
        /// 场景里面所有物体
        /// </summary>
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        #region 输入
        public List<Msg> msg_intput = new List<Msg>();
        public List<Msg> msg_intput_add = new List<Msg>();
        #endregion
        #region 物理引擎
        public World world;
        private int Width = 20;
        private int Height = 20;
        private int cellSize = 2;
        #endregion
        private int Index = 0;
        public float deltaTime = 0.02f;
        #endregion

        public void Start()
        {
            #region 客户端
            world = new World(Width, Height, cellSize);//初始化物理引擎
            #endregion
            #region 服务器
            old_data = new GameData();
            old_data.index = 0;
            System.Timers.Timer t = new System.Timers.Timer(1000 / this.tickRate);
            t.Elapsed += new System.Timers.ElapsedEventHandler(Update); //到达时间的时候执行事件；   
            t.AutoReset = true;
            t.Enabled = true;
            #endregion
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void Update(object source, System.Timers.ElapsedEventArgs e)
        {
            if (_run) { return; }
            _run = true;
            Index++;
            this.ProcessInputs();
            this.EntityUpdate();
            this.SendMsg();
            if (Index/ (tickRate*5)==0)
            {//每隔5秒创建一个新快照
                old_data = new GameData();
                old_data.index = Index;
                foreach(var enti in entities)
                {
                    EntityData data = new EntityData();
                    enti.Value.GetValue(data);
                    old_data.entitys.Add(data);
                }
            }
            else
            {//保存到快照里面
                Msgs msgs = new Msgs();
                msgs.index = Index;
                msgs.items.AddRange(msg_intput);
                old_data.msgs.Add(msgs);
            }
            msg_intput.Clear();
            _run = false;
        }

        public void EntityUpdate()
        {
            foreach (var item in entities)
            {
                item.Value.ActionUpdate();
            }
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        public void ProcessInputs()
        {
            if(clients_add.Count>0)
            {//有新的玩家加入

                lock (clients_add)
                {
                    foreach(var v in clients_add)
                    {
                        v.Value.Send<GameData>(40, old_data);
                        clients.Add(v.Key, v.Value);
                    }
                    clients_add.Clear();
                }
            }

            if (msg_intput_add.Count > 0)
            {
                lock (msg_intput_add)
                {
                    msg_intput.AddRange(msg_intput_add);
                    msg_intput_add.Clear();
                }
            }
            Msg item;
            for (int i = 0; i < msg_intput.Count; i++)
            {
                item = msg_intput[i];
                switch (item.type)
                {
                    case 0://玩家加入
                        GameObject gameobject = new GameObject();
                        PlayerControl script = gameobject.AddComponent<PlayerControl>();
                        script.Init(this, item.client_id, 0, 0, 1, 1, Tags.Group1);
                        entities.Add(item.client_id, script);
                        players.Add(item.client_id, script);
                        break;
                    default:
                        if (players.ContainsKey(item.client_id))
                        {
                            players[item.client_id].applyInput(item.type);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 返回所有操作
        /// </summary>
        public void SendMsg()
        {
            Msgs datas = new Msgs();
            datas.index = Index;
            datas.items.AddRange(msg_intput);
            foreach (var item in entities)
            {
                clients[item.Key].Send<Msgs>(31, datas);
            }
        }


        #region 新消息
        public void AddMessage(int client_id, int _index, int type)
        {
            //if (_index != 0 && _index != Index) { return; }//可能是客户端太卡
            lock (msg_intput_add)
            {

                Msg msg = new Msg();
                msg.client_id = client_id;
                msg.type = type;
                msg_intput_add.Add(msg);
            }
        }

        /// <summary>
        /// 玩家加入房间
        /// </summary>
        /// <param name="client"></param>
        public void PlayrJoin(Client client)
        {
            lock (clients_add)
            {
                clients_add.Add(client.Id, client);
            }
            //AddMessage(client.Id, Index, 0);
        }
        public void PlayrJoinOk(Client client)
        {
            AddMessage(client.Id, Index, 0);
        }
        #endregion
    }


}
