using google.protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public int entity_id = 0;
    public float x = 0;
    public float speed = 10;
    public List<float[]> position_buffer = new List<float[]>();

    public void applyInput(Message msg)
    {
        x += msg.press_time* speed;
        //Debug.Log("press_time:" + msg.press_time+",x:"+x);
    }

}