using System.Collections.Generic;
using GameEngine.Network;
using System.IO;
using System;
using GameEngine.Script;
namespace GameEngine
{
    public class Game
    {
        public List<Client> clients=new List<Client>();           // nth client also has entityId == n
        public Dictionary<int, EntityControl> entities=new Dictionary<int, EntityControl>();          // nth entry has entityId n
        public Dictionary<int,int> lastProcessedInputSeqNums=new Dictionary<int, int>(); // last processed input's seq num, by entityId
        public List<Input> messages=new List<Input>();  // server's network (where it receives inputs from clients)
        private int tickRate = 50;
        private int worldStateSeq = 0;
        private Skill_Manager skill_manager;
        public void start()
        {
            skill_manager = new Skill_Manager();
            skill_manager.InitSkill();

            System.Timers.Timer t = new System.Timers.Timer(1000 / this.tickRate);
            t.Elapsed += new System.Timers.ElapsedEventHandler(update); //到达时间的时候执行事件；   
            t.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；   
        }

        /// <summary>
        /// 客户端加入--OK
        /// </summary>
        /// <param name="client"></param>
        public void connect(Client client)
        {
            int entityId = client.Info.Id;
            this.clients.Add(client);

            GameObject entity = new GameObject(entityId);
            Transfrom transform= entity.AddComponent<Transfrom>();
            EntityControl script= entity.AddComponent<EntityControl>();
            entity.AddComponent<Skill_Pool>();
            SkillObj so= entity.AddComponent<SkillObj>();
            //======初始化技能=====
            new Skill(50005, so, 1, skill_manager, SkillType.Normal);
            transform.x = 5; // 出生点坐标
            this.entities.Add(entityId, script);
        }
        /// <summary>
        /// 验证输入合法性--OK
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool validInput(Input input)
        {
            // Not exactly sure where 1/40 comes from.  I got it from the
            // original code.  The longest possible valid "press" should be
            // 1/client.tickRate (1/60).  But the JS timers are not reliable,
            // so if you use 1/60 below you end up throwing out a lot of
            // inputs that are slighly too long... so maybe that's where 1/40
            // comes from?
            //return System.Math.Abs(input.pressTime) <= 1 / 40;
            return true;
        }
        /// <summary>
        /// 处理输入---OK
        /// </summary>
        public void processInputs()
        {
            while (true)
            {
                var msg = GetClientInput();
                if (msg == null) break;
                if (validInput(msg))//判断输入是否合法
                {
                    Log.Info("input======:" + msg.seqNum);
                    int id = msg.entityId;
                    this.entities[id].applyInput(msg);
                    this.lastProcessedInputSeqNums[id] = msg.seqNum;
                }
                else
                {
                    Log.Error("throwing out input!");
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
        public Input GetClientInput()
        {
            DateTime now = System.DateTime.Now;
            for (int i = 0; i < this.messages.Count; ++i)
            {
                var qm = this.messages[i];
                if (qm.recvTs <= now)
                {
                    messages.RemoveAt(i);
                    return qm;
                }
            }
            return null;
        }
        public void AddMessage(Input message)
        {
            message.recvTs = DateTime.Now.AddSeconds(message.lagMs);
            this.messages.Add(message);
        }
        private bool _run = false;

        //===========命令=============
        public void ShowEntities(int obj_id)
        {
            entities[obj_id].ShowInfo();
        }
    }
}
