using Humper.Responses;
using UnityEngine;
using Humper;
using System.Collections.Generic;
using google.protobuf;
using System;

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
            data.scripts.AddRange(GetScriptData());
        }

        #region 脚本
        /// <summary>
        /// 逻辑脚本
        /// </summary>
        public List<IEntityAction> items = new List<IEntityAction>();
        public void LockUpdate()
        {
            gameObject.transform.position = new Vector3(box.X + box.Width / 2.0f, 0, box.Y + +box.Height / 2.0f);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ActionUpdate();
            }
        }
        public T AddComponent<T>() where T : MonoBehaviour, new()
        {
            T t = gameObject.AddComponent<T>();
            if (t is IEntityAction)
            {
                items.Add(t as IEntityAction);
            }
            return t;
        }

        public List<ScriptData> GetScriptData()
        {
            List<ScriptData> result = new List<ScriptData>();
            for (int i = 0; i < items.Count; i++)
            {
                ScriptData sd = new ScriptData();
                sd.id = ((items[i].GetType().GetCustomAttributes(typeof(ScriptAttribute), false))[0] as ScriptAttribute).id;
                sd.datas = items[i].GetDatas();
                result.Add(sd);
            }
            return result;
        }
        #endregion
    }
}
