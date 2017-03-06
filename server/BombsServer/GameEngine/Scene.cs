using System.Collections.Generic;
using System.Xml;

namespace GameEngine
{
    public class Scene
    {
        //=======xml 数据====
        public int Id;
        public List<List<int>> maps;
        public List<Entity> Modules;
        //==================
        private List<Entity> Modules_Add;

        public void Init(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "map":
                        maps = new List<List<int>>();
                        foreach (XmlNode line in item.ChildNodes)
                        {
                            List<int> datas = GameManager.GetXmlInts(line);
                            maps.Add(datas);
                        }
                        break;
                    case "prefabs":
                        Modules = new List<Entity>();
                        foreach (XmlNode item_obj in item.ChildNodes)
                        {
                            Entity entity = new Entity();
                            entity.Init(item_obj);
                            Modules.Add(entity);
                        }
                        break;
                }
            }
        }

        public void AddModule(Entity item)
        {
            Modules_Add.Add(item);
        }

        public void Update()
        {
            if (Modules_Add.Count > 0)
            {
                Modules.AddRange(Modules_Add);
                Modules_Add.Clear();
            }
            Entity item;
            for (int i = 0; i < Modules.Count; i++)
            {
                item = Modules[i];
                if (item.state == EntityState.Destory)
                {
                    Modules.RemoveAt(i);
                    i--;
                    break;
                }
                else if (item.state == EntityState.Running)
                {
                    item.Update();
                }
            }
        }

        //====深拷贝====
        public void Copy(Scene scene)
        {
            scene.Id = this.Id;
            scene.maps = new List<List<int>>();
            for (int i = 0; i < maps.Count; i++)
            {
                scene.maps.Add(new List<int>(maps[i]));
            }

            scene.Modules = new List<Entity>();
            for (int i = 0; i < Modules.Count; i++)
            {
                Entity entity = new Entity();
                Modules[i].Copy(entity);
                scene.Modules.Add(entity);
            }
        }
    }
}
