using System;
using System.Collections.Generic;
using google.protobuf;
using Humper;
using UnityEngine;
using GameEngine.Script;
public class Game
{
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
    public List<Msgs> msg_intput = new List<Msgs>();
    public List<Msgs> msg_intput_add = new List<Msgs>();
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
    private GameData world_data;
    private int GameState = 0;
    private bool can_input = false;
    public void Start()
    {
        #region 客户端
        world = new World(Width, Height, cellSize);//初始化物理引擎
        #endregion
        //#region 服务器
        //System.Timers.Timer t = new System.Timers.Timer(1000 / this.tickRate);
        //t.Elapsed += new System.Timers.ElapsedEventHandler(Update); //到达时间的时候执行事件；   
        //t.AutoReset = true;
        //t.Enabled = true;
        //#endregion
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    public void Update()
    {
        switch (GameState)
        {
            case 1://开始游戏初始化
                for (int i = 0; i < 5; i++)
                {
                    if (world_data.entitys.Count > 0)
                    {
                        var entity = world_data.entitys[0];
                        GameObject gameobject = new GameObject();
                        PlayerControl script = gameobject.AddComponent<PlayerControl>();
                        script.Init(this, entity.Id, entity.x, entity.y, entity.width, entity.height, Tags.Group1);
                        entities.Add(entity.Id, script);
                        players.Add(entity.Id, script);
                        world_data.entitys.RemoveAt(0);
                    }
                    else
                    {//完成加载实体
                        GameState = 2;
                        Index = world_data.index;
                        NetHelp.Send(22, Connection.Client.socket);
                        msg_intput.AddRange(world_data.msgs);
                        break;
                    }
                }
                break;
            case 2://执行以前的输入
                Msg item;
                if (msg_intput.Count > 0)
                {
                    Index = msg_intput[0].index;
                    #region 处理一次输入
                    for (int j = 0; j < msg_intput[0].items.Count; j++)
                    {
                        item = msg_intput[0].items[j];
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
                    #endregion
                    //逻辑更新
                    EntityUpdate();
                    msg_intput.RemoveAt(0);
                }
                else
                {
                    GameState = 3;
                }

                break;
            case 3:
                can_input = false;
                if (msg_intput_add.Count > 0)
                {
                    can_input = true;
                    lock (msg_intput_add)
                    {
                        msg_intput.AddRange(msg_intput_add);
                        msg_intput_add.Clear();
                    }
                }
                Msg item2;
                if (msg_intput.Count > 0)
                {
                    Index = msg_intput[0].index;
                    #region 处理一次输入
                    for (int j = 0; j < msg_intput[0].items.Count; j++)
                    {
                        item2 = msg_intput[0].items[0];
                        Debug.Log("客户端输入：" + item2.type + "---" + Index);
                        switch (item2.type)
                        {
                            case 0://玩家加入
                                GameObject gameobject = new GameObject();
                                PlayerControl script = gameobject.AddComponent<PlayerControl>();
                                script.Init(this, item2.client_id, 0, 0, 1, 1, Tags.Group1);
                                entities.Add(item2.client_id, script);
                                players.Add(item2.client_id, script);
                                break;
                            default:
                                if (players.ContainsKey(item2.client_id))
                                {
                                    players[item2.client_id].applyInput(item2.type);
                                }
                                break;
                        }
                    }
                    #endregion
                    //逻辑更新
                    EntityUpdate();
                    msg_intput.RemoveAt(0);
                }
                if (can_input)
                {
                    can_input = false;
                    if (msg_intput.Count == 0)
                    {
                        this.SendMsg();//获取输入上传
                    }
                }
                break;
        }
    }

    public void EntityUpdate()
    {
        foreach (var item in entities)
        {
            item.Value.LockUpdate();
        }
    }


    /// <summary>
    /// 返回所有操作
    /// </summary>
    public void SendMsg()
    {
        google.protobuf.Input input = new google.protobuf.Input();
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            input.type = 1;
        }
        if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            input.type = 2;
        }
        if (UnityEngine.Input.GetKey(KeyCode.W))
        {
            input.type = 3;
        }
        if (UnityEngine.Input.GetKey(KeyCode.S))
        {
            input.type = 4;
        }
        if (input.type == 0) { return; }
        input.index = Index;
        NetHelp.Send<google.protobuf.Input>(30, input, Connection.Client.socket);
        //Debug.Log("上传输入:" + (KeyCode)input.type + "--" + Index);
    }


    #region 新消息
    public void AddMessage(Msgs datas)
    {
        lock (msg_intput_add)
        {
            msg_intput_add.Add(datas);
        }
    }
    public void CreateWorld(GameData data)
    {
        world_data = data;
        GameState = 1;
    }
    #endregion

    public Box CreateObj(float x, float y, float width, float height, string name, Tags tag, out GameObject obj)
    {
        Box box = this.world.Create(x, y, width, height).AddTags(tag);
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = new Vector3(x + width / 2.0f, 0, y + height / 2.0f);
        obj.transform.localScale = new Vector3(width, 1, height);
        return box;
    }

    #region 测试
    public void OnDrawGizmos()
    {
        if (this.world != null)
        {
            var b = this.world.Bounds;
            this.world.DrawDebug((int)b.X, (int)b.Y, (int)b.Width, (int)b.Height, DrawCell, DrawBox, DrawString);
        }
    }
    private void DrawCell(float x, float y, float w, float h, float alpha)
    {
        Draw.DrawRect(new Rect(x, y, w, h), Color.green);
    }

    private void DrawBox(Box box)
    {
        var b = box.Bounds;
        Draw.DrawRect(new Rect(b.X, b.Y, b.Width, b.Height), Color.red);
    }
    private void DrawString(string message, float x, float y, float alpha)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(new Vector3(x, 0, y), message);
#endif
    }

    #endregion
}
