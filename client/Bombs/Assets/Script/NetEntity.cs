using UnityEngine;
using System.IO;

public class NetEntity : MonoBehaviour
{
    public int id;
    public float x;
    public float x_old;
    public float speed = 2;
    public float speed_old;
    public void applyInput(Input input)
    {
        x+=input.pressTime * this.speed;
        transform.position = new Vector3(x, 0, 0);
    }
    public void Lerp(float k)
    {
        x= x + (k * (x - x_old));
        speed = speed + (k * (speed - speed_old));
    }
    public void SetOld()
    {
        x_old = x;
        speed_old = speed;
    }
}


