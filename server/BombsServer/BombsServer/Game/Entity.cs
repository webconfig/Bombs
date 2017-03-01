using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer.Game
{
    public class Entity
    {
        public string Name;
        public List<Script> Scripts = new List<Script>();
        public List<Script> ScriptsAdd = new List<Script>();
        public EntityState state;
        public void Init()
        {
            state = EntityState.Init;
        }

        public void AddScript(Script item)
        {
            item.Start();
            ScriptsAdd.Add(item);
        }

        public void Start()
        {
            state = EntityState.Running;
        }

        public void Enable()
        {
            state = EntityState.Running;
        }

        public void Update()
        {
            if (ScriptsAdd.Count > 0)
            {
                Scripts.AddRange(ScriptsAdd);
                ScriptsAdd.Clear();
            }
            Script item;
            for (int i = 0; i < Scripts.Count; i++)
            {
                item = Scripts[i];
                if (item.state == ScriptState.Destory)
                {
                    Scripts.RemoveAt(i);
                    i--;
                    break;
                }
                else if (item.state == ScriptState.Enable)
                {
                    item.Update();
                }
            }
        }

        public void Disable()
        {
            state = EntityState.Disable;
        }

        public void Destory()
        {
            state = EntityState.Destory;
        }
    }
    public enum EntityState
    {
        None,
        Init,
        Running,
        Disable,
        Destory
    }
}
