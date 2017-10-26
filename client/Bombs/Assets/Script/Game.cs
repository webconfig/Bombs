using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class game : MonoBehaviour
{
    public GameObject prefab;
    public PressEventTrigger btn_lefg, btn_right;
    public Text info;
    public string server= "127.0.0.1";
    public int port= 5991;
    private main main;
    private static game _instance;
    public static game Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        _instance = this;

    }
    // Use this for initialization
    void Start()
    {
        main = new main();
        main.Init(1, 15, server, port);
        Add("Start");
    }

    // Update is called once per frame
    void Update()
    {
        main.Update();

    }

    private void FixedUpdate()
    {
        main.FixedUpdate();
    }

    private void OnGUI()
    {
        main.OnGUI();
    }


    public List<string> strs = new List<string>();
    public int index = 0;
    public void Add(string str)
    {
        if(strs.Count>30)
        {
            strs.RemoveAt(0);
        }
        strs.Add(index.ToString() + "-->:" + str);

        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        for(int i=0;i<strs.Count;i++)
        {
            stringBuilder.Append(strs[i] + "\n");
        }
        index++;
        info.text = stringBuilder.ToString();
    }


}
