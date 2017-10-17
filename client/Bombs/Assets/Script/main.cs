using google.protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class main
{
    public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

    public int entity_id;
    //===配置项=====
    public bool client_side_prediction = true;
    public bool server_reconciliation = true;
    public bool entity_interpolation = true;
    //=============
    private int input_sequence_number;
    public List<Message> pending_inputs = new List<Message>();

    //==================
    private long last_ts = 0;

    //=================
    public NetWork network;
    public int server_update_rate = 10;


    public void Init(int _id, int _server_update_rate, string server_ip, int server_port)
    {
        entity_id = _id;
        server_update_rate = _server_update_rate;

        network = new NetWork(this, server_ip, server_port);

    }

    public void Update()
    {
        if (network.state == 2)
        {
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
        }

        //// Render the World.
        //renderWorld(this.canvas, this.entities);

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
            if (message == null || message.Count == 0)
            {
                break;
            }

            //世界状态是实体状态的列表。
            for (var i = 0; i < message.Count; i++)
            {
                var state = message[i];

                //如果这是我们第一次看到这个实体，创建一个本地表示。
                if (!this.entities.ContainsKey(state.entity_id))
                {
                    Entity entity1 = new Entity();
                    entity1.entity_id = state.entity_id;
                    this.entities[state.entity_id] = entity1;
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
                                //已经处理 其影响已被考虑到世界更新
                                // 我们刚刚得到，所以我们可以放弃它。
                                this.pending_inputs.RemoveAt(j);
                            }
                            else
                            {
                                //尚未由服务器处理。 重新申请
                                entity.applyInput(input);
                                j++;
                            }
                        }
                    }
                    else
                    {
                        //对帐被禁用，所以删除所有保存的输入
                        this.pending_inputs = new List<Message>();
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
                        entity.position_buffer.Add(new long[] { timestamp, (long)(state.position * 1000) });
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
        float dt_sec = (now_ts - last_ts) / 10000000.00f;
        this.last_ts = now_ts;


        Message input = new Message();
        if (Input.GetKey(KeyCode.A))
        {
            input = new Message();
            input.press_time = dt_sec;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            input = new Message();
            input.press_time = dt_sec * -1;
        }
        else
        {
            // Nothing interesting happened.
            return;
        }

        //将输入发送到服务器.
        input.input_sequence_number = this.input_sequence_number++;
        input.entity_id = this.entity_id;
        this.network.send(input);

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
        float render_timestamp = now - (1000.0f / server_update_rate);
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
            if (buffer.Count >= 2 && render_timestamp >= buffer[0][0] && render_timestamp <= buffer[1][0])
            {
                var x0 = buffer[0][1] / 1000.00f;
                var t0 = buffer[0][0];

                var x1 = buffer[1][1] / 1000.00f;
                var t1 = buffer[1][0];
                float x = entity.x;
                entity.x = x0 + (x1 - x0) * (render_timestamp - t0) / (t1 - t0);
                Debug.Log("inter:" + x0 + "," + x1 + "--" + t0 + "," + t1 + "----" + x + "--" + entity.x);
            }
        }
    }

    public void LoginOk()
    {
        entity_id = game.Instance.id;

        Entity entity = new Entity();
        entity.entity_id = entity_id;
        entities.Add(entity_id, entity);
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(100, 100, 300, 100), "Non-acknowledged inputs: " + this.pending_inputs.Count);
    }
}

