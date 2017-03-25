using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;
using System.IO;
public class LockStepManager : MonoBehaviour
{
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
            Connection.Client.Handlers.InputsEvent += Handlers_InputsEvent1;
            Connection.Client.ConnectAsync("127.0.0.1", 7001);
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "加入房间"))
        {
            Client_JoinRomme(-1);
        }
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

    private PlayerInfo Current;
    private bool load_ok = false;
    public Dictionary<int,NetEntity> entities=new Dictionary<int, NetEntity>();
    private int seqNum = 0;
    private List<Input> server_inputs = new List<Input>();
    private List<Input> server_inputs_add = new List<Input>();
    private bool server_run = false;
    private void Handlers_InputsEvent1(int t1, List<Input> t2)
    {
        seqNum = t1;
        server_run = true;
        //Debug.Log("服务器返回：" + seqNum);
        lock(this)
        {
            server_inputs_add.AddRange(t2);
        }
        
    }

    void Update()
    {
        if(server_inputs_add.Count>0)
        {
            lock (this)
            {
                server_inputs.AddRange(server_inputs_add);
                server_inputs_add.Clear();
            }
            for (int i = 0; i < server_inputs.Count; i++)
            {
                switch ((KeyCode)server_inputs[i].keycode)
                {
                    case KeyCode.I:
                        if (server_inputs[i].entityId == Current.Id)
                        {
                            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.name = "player";
                            obj.transform.position = Vector3.zero;
                            NetEntity script = obj.AddComponent<NetEntity>();
                            script.id = Current.Id;
                            entities.Add(server_inputs[i].entityId, script);
                            load_ok = true;
                        }
                        else
                        {
                            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.name = "Other_" + server_inputs[i].entityId;
                            obj.transform.position = Vector3.zero;
                            NetEntity script = obj.AddComponent<NetEntity>();
                            script.id = server_inputs[i].entityId;
                            entities.Add(server_inputs[i].entityId, script);
                        }
                        break;
                    case KeyCode.D:
                        Debug.Log("处理输入:D--"+ seqNum);
                        float x = entities[server_inputs[i].entityId].transform.position.x + server_inputs[i].lagMs * 5;
                        entities[server_inputs[i].entityId].transform.position = new Vector3(x, entities[server_inputs[i].entityId].transform.position.y, entities[server_inputs[i].entityId].transform.position.z);
                        break;
                    case KeyCode.A:
                        Debug.Log("处理输入:A--" + seqNum);
                        float x1 = entities[server_inputs[i].entityId].transform.position.x - server_inputs[i].lagMs * 5;
                        entities[server_inputs[i].entityId].transform.position = new Vector3(x1, entities[server_inputs[i].entityId].transform.position.y, entities[server_inputs[i].entityId].transform.position.z);
                        break;
                }

            }
            server_inputs.Clear();
        }
        if(server_run)
        {
            server_run = false;
            processInputs();
        }
    }

    private void processInputs()
    {
        if (!load_ok) return;
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
        input.seqNum = seqNum;
        input.keycode = code;
        input.lagMs = Time.fixedDeltaTime;
        input.entityId = Current.Id;

        var ret = new MemoryStream();
        var w = new BinaryWriter(ret);
        input.Serialization(w);
        byte[] datas = ret.ToArray();
        NetHelp.Send(40, datas, Connection.Client.socket);
        Debug.Log("上传输入:" + (KeyCode)code+"--"+ seqNum);
    }
}
