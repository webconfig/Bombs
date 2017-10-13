using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer
{
    public class Client
    {

        public Dictionary<int,Entity> entities = new Dictionary<int, Entity>();

        public bool key_left;
        public bool key_right;

        public int entity_id;
        //===配置项=====
        public bool client_side_prediction = true;
        public bool server_reconciliation = true;
        public bool entity_interpolation = true;
        //=============
        private int input_sequence_number;
        public List<Message> pending_inputs;

        //==================
        private long last_ts=0;
        public void Update()
        {
            // Listen to the server.
            this.processServerMessages();

            if (this.entity_id == null)
            {
                return;  // Not connected yet.
            }

            // Process inputs.
            this.processInputs();

            // Interpolate other entities.
            if (this.entity_interpolation)
            {
                this.interpolateEntities();
            }

            //// Render the World.
            //renderWorld(this.canvas, this.entities);

            //// Show some info.
            //var info = "Non-acknowledged inputs: " + this.pending_inputs.length;
            //this.status.textContent = info;
        }
        /// <summary>
        /// 处理来自服务器的所有消息，即世界更新。
        /// 如果启用，请执行服务器对帐。( do server reconciliation.)
        /// </summary>
        public void processServerMessages()
        {
            while (true)
            {
                var message = this.network.receive();
                if (!message)
                {
                    break;
                }

                // World state is a list of entity states.
                for (var i = 0; i < message.length; i++)
                {
                    var state = message[i];

                    // If this is the first time we see this entity, create a local representation.
                    if (!this.entities[state.entity_id])
                    {
                        var entity = new Entity();
                        entity.entity_id = state.entity_id;
                        this.entities[state.entity_id] = entity;
                    }

                    var entity = this.entities[state.entity_id];

                    if (state.entity_id == this.entity_id)
                    {
                        //获得了该客户实体的权威位置.
                        entity.x = state.position;

                        if (this.server_reconciliation)
                        {
                            // Server Reconciliation. 重新应用服务器尚未处理的所有输入
                            var j = 0;
                            while (j < this.pending_inputs.Count)
                            {
                                var input = this.pending_inputs[j];
                                if (input.input_sequence_number <= state.last_processed_input)
                                {
                                    // Already processed. Its effect is already taken into account into the world update
                                    // we just got, so we can drop it.
                                    this.pending_inputs.splice(j, 1);
                                }
                                else
                                {
                                    // Not processed by the server yet. Re-apply it.
                                    entity.applyInput(input);
                                    j++;
                                }
                            }
                        }
                        else
                        {
                            // Reconciliation is disabled, so drop all the saved inputs.
                            this.pending_inputs = [];
                        }
                    }
                    else
                    {
                        // 收到除本客户以外的实体的位置.
                        if (!this.entity_interpolation)
                        {
                            //实体插值被禁用 - 只需接受服务器的位置.
                            entity.x = state.position;
                        }
                        else
                        {
                            //将其添加到位置缓冲区.
                            var timestamp = DateTime.Now.Ticks;
                            entity.position_buffer.Add(new long[] { timestamp, state.position });
                        }
                    }
                }
            }
        }

        public void processInputs()
        {

            long now_ts = DateTime.Now.Ticks;
            if (last_ts == 0)
            {
                last_ts = now_ts;
            }
            var dt_sec = (now_ts - last_ts) / 1000.0;
            this.last_ts = now_ts;


            Message input=new Message();
            if (this.key_right)
            {
                input = new Message();
                input.press_time = (int)(DateTime.Now.Ticks - last_ts);
            }
            else if (this.key_left)
            {
                input = new Message();
                input.press_time = (int)(DateTime.Now.Ticks - last_ts)*-1;
            }
            else
            {
                // Nothing interesting happened.
                return;
            }

            //将输入发送到服务器.
            input.input_sequence_number = this.input_sequence_number++;
            input.entity_id = this.entity_id;
            this.server.network.send(this.lag, input);

            //做客户端预测.
            if (this.client_side_prediction)
            {
                this.entities[this.entity_id].applyInput(input);
            }

            // 保存此输入用于以后的对帐.
            this.pending_inputs.Add(input);
        }

        public void interpolateEntities()
        {
            // 计算渲染时间戳.
            long now = DateTime.Now.Ticks;
            float render_timestamp = now - (1000.0 / server.update_rate);
            foreach (var entity in entities.Values)
            {
                //插入这个客户端的实体没有任何意义.
                if (entity.entity_id == this.entity_id)
                {
                    continue;
                }

                //找到围绕渲染时间戳的两个权威位置.
                var buffer = entity.position_buffer;

                //放弃旧位置.
                while (buffer.Count >= 2 && buffer[1][0] <= render_timestamp)
                {
                    buffer.RemoveAt(0);
                }

                //在两个周边的权威位置之间插入.
                if (buffer.Count >= 2 && buffer[0][0] <= render_timestamp && render_timestamp <= buffer[1][0])
                {
                    var x0 = buffer[0][1];
                    var t0 = buffer[0][0];

                    var x1 = buffer[1][1];
                    var t1 = buffer[1][0];

                    entity.x = x0 + (x1 - x0) * (render_timestamp - t0) / (t1 - t0);
                }
            }
        }
    }
}
