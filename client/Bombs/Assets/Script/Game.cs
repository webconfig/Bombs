using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class game : MonoBehaviour
{
    public int id=1;
    public GameObject prefab;
    public PressEventTrigger btn_lefg, btn_right;
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
}
