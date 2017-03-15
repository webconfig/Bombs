using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;
using System.IO;
public class GameManager : MonoBehaviour
{
    private PlayerInfo Current;

    private List<PlayerInfo> players=new List<PlayerInfo>();

    void Start()
    {
        Application.targetFrameRate = 30;
       
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "连接"))
        {
            Connection.Client.ConnectOkEvent += Client_ConnectOkEvent;
            Connection.Client.Handlers.LoginEvent += Handlers_LoginEvent;
            Connection.Client.Handlers.WorldStateEvent += Handlers_WorldStateEvent;
            Connection.Client.ConnectAsync("127.0.0.1", 7001);
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "加入房间"))
        {
            Client_JoinRomme(-1);
        }
    }

    private void Handlers_WorldStateEvent(WorldState t)
    {
        datas.Enqueue(t);
    }

    private void Handlers_LoginEvent(PlayerInfo t)
    {
        Current = t;
    }

    void Client_ConnectOkEvent()
    {
        Debug.Log("ConnectOk");

        LoginRequest model_login = new LoginRequest();
        model_login.UserName = "u1";
        model_login.Password = "54321";
        NetHelp.Send<LoginRequest>(10, model_login, Connection.Client.socket);
    }

    void Client_JoinRomme(int room_id)
    {
        JoinRoomRequest request = new JoinRoomRequest();
        request.room_id = room_id;
        NetHelp.Send<JoinRoomRequest>(21, request, Connection.Client.socket);
    }
    //=============
    public Queue<WorldState> datas = new Queue<WorldState>();
    //==============================
    public NetEntity Player;
    public Dictionary<int,NetEntity> entities=new Dictionary<int, NetEntity>();
    public long lastUpdateTs = -1;
    public int inputSeqNum = 0;
    public List<Input> pendingInputs=new List<Input>();
    private long cur_Ticks=-1,prev_Ticks = -1;
    //===================
    private bool usePrediction = true;
    private bool useReconciliation = true;
    private bool useEntityInterpolation = true;

    public void FixedUpdate()
    {
        this.processServerMessages();//得到目标位置
        if (this.Player == null) return; // not connected yet
        if (this.useEntityInterpolation)
        {
            this.interpolateEntities();//当前位子到目标位置进行插值
        }
        this.processInputs();//获取输入
        this.render();//更新当前位置
    }

    public void processServerMessages()
    {
        while (true)
        {
            if (datas.Count <= 0) { break; }
            WorldState incoming = datas.Dequeue();
            if (incoming == null) { break; }

            #region 更新
            for (int i = 0; i < incoming.entities.Count; ++i)
            {
                var entity = incoming.entities[i];
                if (entity.id == Current.Id)
                {//当前玩家
                    #region 当前玩家
                    if (this.Player == null)
                    {
                        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.name = "player";
                        NetEntity script = obj.AddComponent<NetEntity>();
                        script.id = Current.Id;
                        this.Player = script;
                    }
                    this.Player.ServerUpdate(entity);
                    if (this.useReconciliation)
                    {
                        int lastProcessed = -1;
                        if (incoming.lastProcessedInputSeqNums.ContainsKey(entity.id))
                        {
                            lastProcessed = incoming.lastProcessedInputSeqNums[entity.id];
                        }

                        for (int j = 0; j < this.pendingInputs.Count; j++)
                        {
                            if (pendingInputs[j].seqNum > lastProcessed)
                            {//服务器没处理过
                                this.Player.applyInput(pendingInputs[j]);
                            }
                            else
                            {//服务器已经处理过的
                                pendingInputs.RemoveAt(j);
                            }
                        }
                    }
                    else
                    {
                        pendingInputs.Clear();
                    }
                    #endregion
                }
                else
                {
                    #region 其他网络玩家
                    if (!entities.ContainsKey(entity.id))
                    {//创建新的
                        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.name = "Other_" + entity.id;
                        NetEntity script = obj.AddComponent<NetEntity>();
                        script.id = entity.id;
                        entities.Add(entity.id, script);
                    }
                    NetEntity item = this.entities[entity.id];
                    item.ServerUpdate(entity);
                    #endregion
                }
            }
            #endregion

            this.prev_Ticks = this.cur_Ticks;
            this.cur_Ticks = System.DateTime.Now.Ticks;
            //==========插值=============
            var enumerator = entities.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.SaveOld();
            }
            //Debug.Log("服务器更新：" + cur_Ticks + "----" + prev_Ticks);
        }
    }

    private void interpolateEntities()
    {
        if (this.prev_Ticks < 0) return;
        if (this.cur_Ticks < 0) return;

        //======计算======
        var now = System.DateTime.Now.Ticks;
        var delta = now - cur_Ticks;//当前时间和上一次服务器更新的时间差
        var statesDelta = cur_Ticks - prev_Ticks;//服务器的时间差
        //Debug.Log("更新：" + delta + "---" + cur_Ticks +"--"+ prev_Ticks +"--"+ statesDelta);
        float interpFactor = 0;
        if (statesDelta == 0) { interpFactor =1; Debug.Log("2222:" + interpFactor); }
        else
        {
            interpFactor = delta*1.00f / statesDelta;
            //Debug.Log("interpFactor:" + interpFactor);
            if (interpFactor < 0) { Debug.Log("wwww:" + interpFactor); interpFactor = 0; }
            if (interpFactor > 1) interpFactor = 1; // If it'll let us div 0, why not
        }


        //=======插值======
        var enumerator = entities.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.Lerp(interpFactor);
        }
    }

    private void processInputs()
    {
        if (this.Player == null) return;
        Input input = null;
        int code = 0;
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            code = (int)KeyCode.D;
        }
        if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            code = (int)KeyCode.A;
        }
        if (UnityEngine.Input.GetKey(KeyCode.W))
        {
            code = (int)KeyCode.W;
        }
        if (UnityEngine.Input.GetKey(KeyCode.S))
        {
            code = (int)KeyCode.S;
        }
        if (code == 0) { return; }

        input = new Input();//this.inputSeqNum++, Time.fixedDeltaTime, Current.Id
        input.seqNum = this.inputSeqNum++;
        input.keycode = code;
        input.lagMs = Time.fixedDeltaTime;
        input.entityId = Player.id;

        var ret = new MemoryStream();
        var w = new BinaryWriter(ret);
        input.Serialization(w);
        byte[] datas = ret.ToArray();
        NetHelp.Send(40, datas, Connection.Client.socket);

        if (this.usePrediction)
        {
            //Debug.Log("本地输入：" + input.seqNum + "," + input.pressTime);
            this.Player.applyInput(input);
        }

        if (this.useReconciliation)
        {
            this.pendingInputs.Add(input);
        }
    }

    private void render()
    {
        Player.render();
        var enumerator = entities.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.render();
        }
    }
}
