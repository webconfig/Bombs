using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Game
{
    public Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
    public Dictionary<int, int> last_processed_input = new Dictionary<int, int>();
    /// <summary>
    /// 客户端上传的操作
    /// </summary>
    public Queue<Message> messages = new Queue<Message>();
    public int update_rate = 10;

    public void Run()
    {
        RunTime();
    }

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
        while (messages.Count > 0)
        {
            var message = messages.Dequeue();
            // 更新其输入更新实体的状态
            // 我们只是忽略看起来不合适的输入; 这是阻止客户作弊的原因
            //if (this.validateInput(message))
            //{
            this.entities[message.entity_id].applyInput(message);
            this.last_processed_input[message.entity_id] = message.input_sequence_number;
            //}

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
        WorlData world = new WorlData();
        foreach (var entity in entities.Values)
        {
            EntityData ed = new EntityData();
            ed.entity_id = entity.entity_id;
            ed.position = entity.x;
            ed.last_processed_input = this.last_processed_input[entity.entity_id];
            world.datas.Add(ed);
        }

        foreach(var item in clients)
        {
            item.Value.Send<WorlData>(2, world);
        }
    }

    #region 网络消息
    /// <summary>
    /// 登陆
    /// </summary>
    /// <param name="id"></param>
    /// <param name="client"></param>
    public void ClientLogin(int id, Client client)
    {
        if (clients.ContainsKey(id))
        {
            clients[id] = client;
            Log.Info("客户端重新连接：" + id);
        }
        else
        {
            Entity entity = new Entity();
            entity.entity_id = id;
            entities.Add(id, entity);
            //==========
            last_processed_input.Add(id, 0);
            //===========
            clients.Add(id, client);
            Log.Info("新客户端加入：" + id);
        }
    }
    /// <summary>
    /// 消息
    /// </summary>
    /// <param name="msg"></param>
    public void AddMessage(Message msg)
    {
        messages.Enqueue(msg);
    }
    #endregion

    #region 时间
    MmTimer timer1;
    private void RunTime()
    {

        timer1 = new MmTimer();
        timer1.Mode = MmTimerMode.Periodic;
        timer1.Interval = 1000 / this.update_rate;
        timer1.Tick += Timer1_Tick;
        timer1.Start();

    }

    private void Timer1_Tick(object sender, EventArgs e)
    {
        Update();
    }
    #endregion
}

