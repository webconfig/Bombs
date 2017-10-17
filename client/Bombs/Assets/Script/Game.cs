using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class game : MonoBehaviour
{
    public int id=1;
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
        _instance = this;
    }
    // Use this for initialization
    void Start()
    {
        main = new main();
        main.Init(1, 15,"127.0.0.1",5991);
    }

    // Update is called once per frame
    void Update()
    {
        main.Update();

    }

    private void OnGUI()
    {
        main.OnGUI();
    }
}
