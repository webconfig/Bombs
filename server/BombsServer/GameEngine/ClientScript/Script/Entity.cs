using Humper.Responses;
using UnityEngine;
using Humper;
using System.Collections.Generic;
using google.protobuf;

namespace GameEngine.Script
{
    public class Entity:MonoBehaviour
    {
        public int Id;
        public Game game;
        public Box box;
        public void Init( Game _game,int _id, float x, float y, float width, float height, Tags tag)
        {
            game = _game;
            Id = _id;
            box = game.world.Create(x, y, width, height).AddTags(tag);
            gameObject.transform.position = new Vector3(box.X + box.Width / 2.0f, 0, box.Y + +box.Height / 2.0f);
        }

        public virtual void GetValue(EntityData data)
        {
            data.Id = Id;
            data.x = box.X;
            data.y = box.Y;
            data.width = box.Width;
            data.height = box.Height;
            //data.tag = (int)box.tags;
        }

        #region 脚本
        public List<MonoBehaviour> items = new List<MonoBehaviour>();

        public void LockUpdate()
        {
            gameObject.transform.position = new Vector3(box.X + box.Width / 2.0f, 0, box.Y + +box.Height / 2.0f);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ActionUpdate();
            }
        }
        #endregion
    }
}
