using System;
using UnityEngine;
public class EntityObj:MonoBehaviour
{
    public Entity entity;

    public void Update()
    {
        if(entity!=null)
        {
            transform.position = new Vector3(entity.x, 0, 0);
        }
    }
}

