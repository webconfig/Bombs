using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Entity
{
    public int entity_id = 0;
    public float x = 0;
    public float speed = 5;
    public List<long[]> position_buffer = new List<long[]>();
    private int index = 0,k=1;
    public void applyInput(Message msg)
    {
        x += msg.press_time * speed;
    }
    public void Update()
    {
        //index += k;
        //if (index == 10)
        //{
        //    k = -1;
        //    speed = -5;
        //}
        //else if (index == -10)
        //{
        //    k = 1;
        //    speed = 5;
        //}
        //x += 0.02f * speed;
    }
}