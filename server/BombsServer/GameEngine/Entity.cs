using System;
using System.Collections.Generic;
using System.Xml;

namespace GameEngine
{
    public class Entity
    {
        //=======xml 数据====
        public int Id;
        public Vector3 Position;
        public string Name;
        public List<Script> Scripts;
        //==================
        public List<Script> ScriptsAdd=new List<Script>();
        public EntityState state;

        public virtual void Init(XmlNode node)
        {
            Id = GameManager.GetXmlAttrInt(node, "id");
            Name = node.Attributes["name"].InnerText;
            Scripts = new List<Script>();
            XmlNodeList nodes = node.SelectSingleNode("script").ChildNodes;
            foreach(XmlNode script_node in nodes)
            {
                Script sb = GameManager.Instance.CreateScript(script_node.Name);
                sb.Init(script_node);
                Scripts.Add(sb);
            }
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

        //====深拷贝====
        public void Copy(Entity entity)
        {
            entity.Id = this.Id;
            entity.Position = this.Position;
            entity.Name = this.Name;

            entity.Scripts = new List<Script>();
            for (int i = 0; i < Scripts.Count; i++)
            {
                Script script = Scripts[i].Create();
                entity.Scripts.Add(script);
            }
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
