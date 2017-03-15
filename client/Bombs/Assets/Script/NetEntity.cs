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
        this.x += input.pressTime * this.speed;
    }
    public void Lerp(float interpFactor)
    {
        //Debug.Log(x_old + "--" + x + "||||||" + speed_old + "--" + speed);
        x = x + (interpFactor * (x - x_old));
        speed = speed + (interpFactor * (speed - speed_old));
    }
    public void SaveOld()
    {
        x_old = x;
        speed_old = speed;
    }
    public void render()
    {
        transform.position = new Vector3(x, 0, 0);
    }
}


