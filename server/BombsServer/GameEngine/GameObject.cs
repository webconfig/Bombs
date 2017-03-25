using System.IO;
using System.Collections.Generic;
using GameEngine.Script;
namespace GameEngine
{
    public class GameObject
    {
        public int id;
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
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].Serialization(w);
            }
        }

        public void Update()
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                if (scripts[i].State == ScriptState.none)
                {
                    scripts[i].Start();
                }
                else if (scripts[i].State== ScriptState.destory)
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
        public T AddComponent<T>() where T : ScriptBase, new()
        {
            T t = new T();
            t.State = ScriptState.none;
            t.gameobject = this;
            scripts.Add(t);
            return t;
        }
        public T GetComponent<T>() where T : ScriptBase
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                if(scripts[i] is T)
                {
                    return (T)scripts[i];
                }
            }
            return null;
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
            //Log.Info("==位置:{0},{1},{1}", x,  z);
            //Log.Info("==脚本:");
            //for (int i = 0; i < scripts.Count; i++)
            //{
            //    scripts[i].Show();
            //}
        }
    }

    public enum GameObjectState
    {
        Init,
        Run,
        destory
    }
}
