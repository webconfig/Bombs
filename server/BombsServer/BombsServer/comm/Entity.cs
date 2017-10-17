using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Entity
{
    public int entity_id = 0;
    public float x = 0;
    public float speed = 10;
    public List<long[]> position_buffer = new List<long[]>();

    public void applyInput(Message msg)
    {
        x += msg.press_time * speed;
        Log.Info("press_time:" + msg.press_time + ",x:" + x);
    }

}