using UnityEngine;
using System.IO;

public class NetEntity : MonoBehaviour
{
    public int id;
    public float x;
    public float x_old;
    public float z;
    public float z_old;
    public float speed = 2;
    public float speed_old;
    public string anim;
    public string anim_old;
    public Animator animator;


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
        animator.SetBool("run", true);
    }
    public void StartRunAnim()
    {
        animator.SetBool("run", false);
    }
    public void StopRunAnim()
    {
        animator.SetBool("run", false);
    }

    public void Lerp(float interpFactor)
    {
        //Debug.Log(x_old + "--" + x + "||||||" + speed_old + "--" + speed);
        x = x + (interpFactor * (x - x_old));
        z = z + (interpFactor * (z - z_old));
        speed = speed + (interpFactor * (speed - speed_old));
    }
    public void SaveOld()
    {
        x_old = x;
        z_old = z;
        speed_old = speed;
    }
    public void render()
    {
        transform.position = new Vector3(x, 0, z);
        if(anim!=anim_old)
        {
            AnimatorStateInfo animatorInfo = animator.GetCurrentAnimatorStateInfo(0);
            if ((!animator.IsInTransition(0)) && animatorInfo.IsName(anim))
            {
               
            }
            else
            {
                animator.CrossFade(anim, 0, 0, 0);
            }
            anim_old = anim;
        }
    }
    public void ServerUpdate(Entity item)
    {
        x = item.x;
        z = item.z;
        speed = item.speed;
        anim = item.anim;
    }

}


