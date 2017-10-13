using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer
{
    public class Game
    {
        public Dictionary<int, client> clients;
        public Dictionary<int, Entity> entities;
        public Dictionary<int, int> last_processed_input;

        /// <summary>
        /// 网络
        /// </summary>
        public LagNetwork network;


        public void Update()
        {
            processInputs();
            sendWorldState();
        }
        /// <summary>
        /// 处理输入
        /// </summary>
        public void processInputs()
        {
            // 处理客户端的所有待处理消息
            while (this.network.messages.Count > 0)
            {
                var message = this.network.messages.Dequeue();
                // 更新其输入更新实体的状态
                // 我们只是忽略看起来不合适的输入; 这是阻止客户作弊的原因
                if (this.validateInput(message))
                {
                    var id = message.entity_id;
                    this.entities[id].applyInput(message);
                    this.last_processed_input[id] = message.input_sequence_number;
                }

            }

            #region debug
            //var info = "Last acknowledged input: ";
            //for (var i = 0; i < this.clients.length; ++i)
            //{
            //    info += "Player " + i + ": #" + (this.last_processed_input[i] || 0) + "   ";
            //}
            //this.status.textContent = info;
            #endregion
        }
        /// <summary>
        /// 发送状态
        /// </summary>
        public void sendWorldState()
        {
            // 收集世界的状况。 在一个真实的应用程序中，可以过滤状态以避免泄露数据
            // (例如不可见敌人的位置)
            List<EntityData> world_state = new List<EntityData>();
            foreach (var entity in entities.Values)
            {
                EntityData ed = new EntityData();
                ed.entity_id = entity.entity_id;
                ed.position = entity.x;
                ed.last_processed_input = this.last_processed_input[entity.entity_id];
                world_state.Add(ed);
            }

            //  //将状态广播给所有客户.
            //  for (var i = 0; i<num_clients; i++) {
            //    var client = this.clients[i];
            //        client.network.send(client.lag, world_state);
            //  }
            //}
        }
    }

    public class client
    {

    }
    public class Entity
    {
        public int entity_id = 0;
        public int x = 0;
        public int speed = 2;

        public void applyInput(Message msg)
        {

        }

    }

    public class Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public class LagNetwork
    {
        public Queue<Message> messages;
    }

    public class Message
    {
        public int entity_id;
        public int press_time;
        public int input_sequence_number;
    }
    public class EntityData
    {
        public int entity_id;
        public int position;
        public int last_processed_input;
    }

}
