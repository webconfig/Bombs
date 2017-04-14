using System.Collections.Generic;
using GameEngine.Network;
using System.IO;
using System;
using GameEngine.Script;
using Humper;
namespace GameEngine
{
    public class Game
    {
        private bool _run = false;
        /// <summary>
        /// 玩家集合
        /// </summary>
        public Dictionary<int, EntityControl> entities = new Dictionary<int, EntityControl>();
        #region 客户端输入
        public List<Msg> msg_intput = new List<Msg>();
        public List<Msg> msg_intput_add = new List<Msg>();
        #endregion
        /// <summary>
        /// 输出到客户端
        /// </summary>
        public List<Msg> msg_output = new List<Msg>();
        private int tickRate = 50;
        private int Index = 0;
        #region 物理引擎
        private World world;
        private int Width = 20;
        private int Height = 20;
        private int cellSize = 2;
        #endregion
        public void start()
        {
            //skill_manager = new Skill_Manager();
            //skill_manager.InitSkill();
            world = new World(Width, Height, cellSize);//初始化物理引擎
            System.Timers.Timer t = new System.Timers.Timer(1000 / this.tickRate);
            t.Elapsed += new System.Timers.ElapsedEventHandler(update); //到达时间的时候执行事件；   
            t.AutoReset = true;
            t.Enabled = true;
        }

        /// <summary>
        /// 处理输入---OK
        /// </summary>
        public void processInputs()
        {

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
                switch (item.Data.Type)
                {
                    case 0://玩家加入
                        GameObject entity = new GameObject(item.client.Id);
                        Transfrom transform = entity.AddComponent<Transfrom>();
                        EntityControl script = entity.AddComponent<EntityControl>();
                        script.client = item.client;
                        entity.AddComponent<Skill_Pool>();
                        SkillObj so = entity.AddComponent<SkillObj>();
                        ////======初始化技能=====
                        //new Skill(50005, so, 1, skill_manager, SkillType.Normal);
                        //transform.x = 5; // 出生点坐标
                        //this.entities.Add(entityId, script);
                        break;
                }
            }
        }

        /// <summary>
        /// 返回当前状态--OK
        /// </summary>
        public void sendWorldState()
        {
            List<GameObject> ent_datas = new List<GameObject>();
            foreach (var item in entities)
            {
                ent_datas.Add(item.Value.gameobject);
            }
            var msg = new WorldState(
                this.worldStateSeq++,
                ent_datas,
                this.lastProcessedInputSeqNums
            );
            var ret = new MemoryStream();
            var w = new BinaryWriter(ret);
            msg.Serialization(w);
            byte[] datas = ret.ToArray();
            foreach (var client in clients)
            {
                client.Send(30, datas);
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void update(object source, System.Timers.ElapsedEventArgs e)
        {
            if (_run) { return; }
            _run = true;
            this.processInputs();
            this.entityUpdate();
            this.sendWorldState();
            _run = false;
        }


        public void entityUpdate()
        {
            foreach (var item in entities)
            {
                item.Value.gameobject.Update();
            }
        }

        public void AddMessage(Client client, MsgData message)
        {
            lock (msg_intput_add)
            {
                msg_intput_add
                    .Add(new Msg(client,message));
            }
        }
    }

    public class Msg
    {
        public Client client;
        public MsgData Data;
        public Msg(Client _client, MsgData _msg)
        {
            client = _client;
            Data = _msg;
        }
    }
    public class MsgData
    {
        public byte Type;
        public byte[] datas;
    }

}
