using System.IO;
using System.Collections.Generic;
using GameEngine.Script;
namespace GameEngine
{
    public class GameObject
    {
        public int id;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float speed = 2;
        public GameObjectState State = GameObjectState.Init;
        /// <summary>
        /// 父节点
        /// </summary>
        public GameObject parent;
        /// <summary>
        /// 儿子节点
        /// </summary>
        public List<GameObject> childs = new List<GameObject>();
        public GameObject(int id)
        {
            this.id = id;
        }
        public GameObject()
        {
        }

        public void Serialization(BinaryWriter w)
        {
            w.Write(id);
            w.Write(x);
            w.Write(y);
            w.Write(z);
            w.Write(speed);
        }
        public void DeSerialization(BinaryReader r)
        {
            id = r.ReadInt32();
            x = r.ReadSingle();
            y = r.ReadSingle();
            z = r.ReadSingle();
            speed = r.ReadSingle();
        }

        public void Update()
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                if(scripts[i].State== ScriptState.destory)
                {
                    scripts.RemoveAt(i);
                    i--;
                }
                else
                {
                    scripts[i].Update();
                }
            }
            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i].State == GameObjectState.destory)
                {
                    childs.RemoveAt(i);
                    i--;
                }
                else
                {
                    childs[i].Update();
                }
            }
        }

        #region 脚本
        private List<ScriptBase> scripts = new List<ScriptBase>();
        public void AddScript(ScriptBase sb)
        {
            sb.entity = this;
            scripts.Add(sb);
        }
        #endregion

        public void AddChild(GameObject obj)
        {
            obj.parent = this;
            childs.Add(obj);
        }


        //==============调试=============
        public void Show()
        {
            Log.Info("==位置:{0},{1},{1}", x, y, z);
            Log.Info("==脚本:");
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].Show();
            }
        }
    }

    public enum GameObjectState
    {
        Init,
        Run,
        destory
    }
}
