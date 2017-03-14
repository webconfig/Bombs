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

    public void Update()
    {
        this.processServerMessages();
        if (this.Player == null) return; // not connected yet
        if (this.useEntityInterpolation)
        {
            this.interpolateEntities();
        }
        this.processInputs();
    }

    public void processServerMessages()
    {
        while (true)
        {
            if (datas.Count <= 0) { break; }
            WorldState incoming = datas.Dequeue();
            if (incoming == null) { break; }
            for (int i = 0; i < incoming.entities.Count; ++i)
            {
                var entity = incoming.entities[i];
                if (entity.id == Current.Id)
                {//当前用户
                    if (this.Player == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "player";
                        NetEntity script= obj.AddComponent<NetEntity>();
                        this.Player = script;
                        this.Player.transform.position = new Vector3(entity.x, 0, 0);
                        this.entities[entity.id] = this.Player;
                    }

                    this.Player.speed = entity.speed;
                    this.Player.x = entity.x;

                    if (this.useReconciliation)
                    {
                        if (incoming.lastProcessedInputSeqNums.ContainsKey(entity.id))
                        {
                            var lastProcessed = incoming.lastProcessedInputSeqNums[entity.id];
                            if (lastProcessed > 0)
                            {
                                List<Input> result = new List<Input>();
                                for (int j = 0; j < this.pendingInputs.Count; j++)
                                {
                                    if (pendingInputs[j].seqNum > lastProcessed)
                                    {
                                        result.Add(pendingInputs[j]);
                                    }
                                }
                                pendingInputs = result;
                            }
                        }
                        for (int k = 0; k < pendingInputs.Count; k++)
                        {
                            this.Player.applyInput(pendingInputs[i]);
                        }
                        //===============
                        pendingInputs.Clear();
                    }
                }
                else
                {
                    if (entities.ContainsKey(entity.id))
                    {//包含
                        NetEntity item = this.entities[entity.id];
                        item.x = entity.x;
                        item.speed = entity.speed;
                    }
                    else
                    {//新的
                        GameObject obj = new GameObject();
                        obj.name = "Other_"+ entity.id;
                        obj.transform.position = new Vector3(entity.x, 0, 0);
                        NetEntity script = obj.AddComponent<NetEntity>();
                        script.id = entity.id;
                        script.speed = entity.speed;
                        entities.Add(entity.id, script);
                    }
                }
            }
            //================================
            this.prev_Ticks = this.cur_Ticks;
            this.cur_Ticks = System.DateTime.Now.Ticks;
            var enumerator = entities.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.SetOld();
            }
        }
    }

    private void interpolateEntities()
    {
        if (this.prev_Ticks < 0) return;
        if (this.cur_Ticks < 0) return;

        var now = System.DateTime.Now.Ticks;
        var delta = now - cur_Ticks;
        var statesDelta = cur_Ticks - prev_Ticks;
        float interpFactor = 0;
        if (statesDelta == 0) { interpFactor = 0; }
        else
        {
            interpFactor = delta / statesDelta;
            if (interpFactor > 1) interpFactor = 1; // If it'll let us div 0, why not
        }
        var enumerator = entities.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.Lerp(interpFactor);
        }
        //WorldState prev = this.prevWorldState.value;
        //WorldState cur = this.curWorldState.value;
        //for (int i = 0; i < cur.entities.Count; ++i)
        //{
        //    var curEntity = cur.entities[i];
        //    if (curEntity.id == Current.Id) continue; // don't interpolate self
        //    var prevEntity = prev.entities[i]; // assumes the set of entities never changes'
        //    var newEntity = curEntity.copy();
        //    newEntity.x = prevEntity.x + (interpFactor * (curEntity.x - prevEntity.x));
        //    newEntity.speed = prevEntity.speed + (interpFactor * (curEntity.speed - prevEntity.speed));
        //    this.entities[i] = newEntity;
        //}
    }

    private void processInputs()
    {
        if (this.Player == null) return;
        Input input = null;
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            Debug.Log("D");
            input = new Input(this.inputSeqNum++, Time.deltaTime, Current.Id);
        }
        else if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            Debug.Log("A");
            input = new Input(this.inputSeqNum++, Time.deltaTime*-1, Current.Id);
        }
        else
        {
            return;
        }

        var ret = new MemoryStream();
        var w = new BinaryWriter(ret);
        input.Serialization(w);
        byte[] datas = ret.ToArray();
        NetHelp.Send(40, datas, Connection.Client.socket);

        if (this.usePrediction)
        {
            this.Player.applyInput(input);
        }

        if (this.useReconciliation)
        {
            this.pendingInputs.Add(input);
        }
    }
}
