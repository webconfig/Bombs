using System.Collections.Generic;
using GameEngine.Network;
using System.IO;
using System;
namespace GameEngine
{
    public class Game
    {
        public List<Client> clients=new List<Client>();           // nth client also has entityId == n
        public Dictionary<int,Entity> entities=new Dictionary<int, Entity>();          // nth entry has entityId n
        public Dictionary<int,int> lastProcessedInputSeqNums=new Dictionary<int, int>(); // last processed input's seq num, by entityId
        public List<Input> messages=new List<Input>();  // server's network (where it receives inputs from clients)
        private int tickRate = 20;
        private int worldStateSeq = 0;

        public void start()
        {
            System.Timers.Timer t = new System.Timers.Timer(1000 / this.tickRate);   //实例化Timer类，设置间隔时间为10000毫秒；   
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
            //client.server = this;
            int entityId = client.Info.Id;
            //client.entityId = entityId; // give the client its entity id so it can identify future state messages
            this.clients.Add(client);

            Entity entity = new Entity(entityId);
            entity.x = 5; // spawn point
            this.entities.Add(entityId,entity);
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
                var msg = receive();
                if (msg == null) break;
                Input input = msg as Input;
                if (input == null) break;
                if (validInput(input))//判断输入是否合法
                {
                    Log.Info("input======:" + input.seqNum);
                    int id = input.entityId;
                    this.entities[id].applyInput(input);
                    this.lastProcessedInputSeqNums[id] = input.seqNum;
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
            List<Entity> ent_datas = new List<Entity>();
            foreach (var item in entities)
            {
                ent_datas.Add(item.Value);
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
            this.sendWorldState();
            _run = false;
        }

        public Input receive()
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
    }
}
