using UnityEngine;
using System.IO;

public class NetEntity : MonoBehaviour
{
    public int id;
    public float x;
    public float x_old;
    public float y;
    public float y_old;
    public float z;
    public float z_old;
    public float speed = 2;
    public float speed_old;
    public void applyInput(Input input)
    {
        switch((KeyCode)input.keycode)
        {
            case KeyCode.D:
                this.x += input.lagMs * this.speed;
                break;
            case KeyCode.A:
                this.x -= input.lagMs * this.speed;
                break;
            case KeyCode.W:
                this.z += input.lagMs * this.speed;
                break;
            case KeyCode.S:
                this.z -= input.lagMs * this.speed;
                break;
        }
    }
    public void Lerp(float interpFactor)
    {
        //Debug.Log(x_old + "--" + x + "||||||" + speed_old + "--" + speed);
        x = x + (interpFactor * (x - x_old));
        y = y + (interpFactor * (y - y_old));
        z = z + (interpFactor * (z - z_old));
        speed = speed + (interpFactor * (speed - speed_old));
    }
    public void SaveOld()
    {
        x_old = x;
        y_old = y;
        z_old = z;
        speed_old = speed;
    }
    public void render()
    {
        transform.position = new Vector3(x, y, z);
    }
    public void ServerUpdate(Entity item)
    {
        x = item.x;
        y = item.y;
        z = item.z;
        speed = item.speed;
    }

}


