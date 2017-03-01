using System.Collections.Generic;
using System.Xml;

namespace BombsServer.Game
{
    public class Scene
    {
        private static Scene _instance;
        public static Scene Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Scene();
                }
                return _instance;
            }
        }
        public List<List<int>> maps;
        public List<Entity> Modules = new List<Entity>();
        public List<Entity> Modules_Add = new List<Entity>();

        public void AddModule(Entity item)
        {
            item.Start();
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


        public void LoadScene(string filePath)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filePath);
            XmlNode root = xml.LastChild;
            foreach (XmlNode item in root.ChildNodes)
            {
                switch (item.Name)
                {
                    case "map":
                        string str = item.InnerText;
                        string[] strs = str.Split('\n');
                        maps = new List<List<int>>();
                        for (int i = 1; i < strs.Length - 1; i++)
                        {
                            List<int> k = new List<int>();
                            string[] values = strs[i].Trim('\r').Split(' ');
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(values[j]))
                                {
                                    k.Add(int.Parse(values[j]));
                                }
                            }
                            maps.Add(k);
                        }
                        break;
                    case "objs":
                        foreach (XmlNode item_obj in item.ChildNodes)
                        {
                            Entity entity = new Entity();
                        }
                        break;
                }
            }
        }
    }
}
