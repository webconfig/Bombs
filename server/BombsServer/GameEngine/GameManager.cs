using System;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
namespace GameEngine
{
    public class GameManager
    {
        public static readonly GameManager Instance = new GameManager();
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="filePath"></param>
        public void Init(string ConfigPath, Assembly assembly)
        {
            AutoLoadScript(assembly);
            XmlDocument xml = new XmlDocument();
            xml.Load(ConfigPath);
            XmlNode root = xml.LastChild;
            InitScenes(root.SelectSingleNode("scens"));
        }

        #region 场景
        private Dictionary<int, Scene> Scenes = new Dictionary<int, Scene>();

        private void InitScenes(XmlNode node)
        {
            foreach (XmlNode obj in node.ChildNodes)
            {
                Scene scene = new Scene();
                scene.Init(obj);
                Scenes.Add(scene.Id, scene);
            }
        }

        /// <summary>
        /// 创建一个副本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Scene CreateScene(int id)
        {
            Scene item = new Scene();
            Scenes[id].Copy(item);
            return item;
        }
        #endregion

        #region 脚本集合
        private Dictionary<string, Type> Scripts = new Dictionary<string, Type>();
        private void AutoLoadScript(Assembly assembly)
        {
            var types = assembly.GetTypes();
            Type baseType = typeof(Script);
            foreach (var t in types)
            {
                var tmp = t.BaseType;
                while (tmp != null)
                {
                    if (tmp == baseType)
                    {
                        Scripts.Add(t.Name, t);
                        break;
                    }
                    else
                    {
                        tmp = tmp.BaseType;
                    }
                }
            }
        }
        public Script CreateScript(string name)
        {
            return Activator.CreateInstance(Scripts[name]) as Script;
        }
        #endregion

        #region xml 读取
        private static int XmlNullValu = 0;
        public static List<int> GetXmlInts(XmlNode node)
        {
            int num = 0;
            List<int> result = new List<int>();
            string str = node.InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string item in strs)
                {
                    if (int.TryParse(item, out num))
                    {
                        result.Add(num);
                    }
                }
            }
            return result;
        }
        public static int GetXmlAttrInt(XmlNode node, string key)
        {
            if (node.Attributes[key] != null)
            {
                string str = node.Attributes[key].InnerText;
                if (!string.IsNullOrEmpty(str))
                {
                    return int.Parse(str);
                }
            }
            return XmlNullValu;
        }
        public static float GetXmlAttrFloat(XmlNode node, string key)
        {
            if (node.Attributes[key] != null)
            {
                string str = node.Attributes[key].InnerText;
                if (!string.IsNullOrEmpty(str))
                {
                    return float.Parse(str);
                }
            }
            return XmlNullValu;
        }
        //public static Vector3 GetXmlAttrVector(XmlNode node, string key)
        //{
        //    //Debug.Log(key);
        //    if (node.Attributes[key] != null)
        //    {
        //        string str = node.Attributes[key].InnerText;
        //        if (!string.IsNullOrEmpty(str))
        //        {
        //            return GetVector(str);
        //        }
        //    }
        //    return Vector3.zero;
        //}
        //public static Color GetXmlAttrColor(XmlNode node, string key)
        //{
        //    if (node.Attributes[key] != null)
        //    {
        //        string str = node.Attributes[key].InnerText;
        //        if (!string.IsNullOrEmpty(str))
        //        {
        //            string[] strs = str.Split(',');
        //            if (strs.Length == 4)
        //            {
        //                return new Color(float.Parse(strs[0]) / 255.00f, float.Parse(strs[1]) / 255.00f, float.Parse(strs[2]) / 255.00f, float.Parse(strs[3]) / 255.00f);
        //            }
        //            else
        //            {
        //                return new Color(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        //            }
        //        }
        //    }
        //    return Color.white;
        //}
        //public static List<Vector3> GetXmlAttrVectors(XmlNode node, string key)
        //{
        //    List<Vector3> result = new List<Vector3>();
        //    if (node.Attributes[key] != null)
        //    {
        //        string str = node.Attributes[key].InnerText;
        //        if (!string.IsNullOrEmpty(str))
        //        {
        //            string[] datas = str.Split('|');

        //            foreach (string pp in datas)
        //            {
        //                string[] strs = pp.Split(',');

        //                result.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
        //            }
        //        }
        //    }
        //    return result;
        //}
        public static List<int> GetXmlAttrInts(XmlNode node, string key)
        {

            if (node.Attributes[key] == null) { return new List<int>(); }

            //Debug.Log(key);
            int num = 0;
            List<int> result = new List<int>();
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string item in strs)
                {
                    if (int.TryParse(item, out num))
                    {
                        result.Add(num);
                    }
                }
            }
            return result;
        }
        public static List<float> GetXmlAttrFloats(XmlNode node, string key)
        {

            if (node.Attributes[key] == null) { return new List<float>(); }

            //Debug.Log(key);
            float num = 0;
            List<float> result = new List<float>();
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string item in strs)
                {
                    if (float.TryParse(item, out num))
                    {
                        result.Add(num);
                    }
                }
            }
            return result;
        }
        public static List<string> GetXmlAttrStrings(XmlNode node, string key)
        {
            if (node.Attributes[key] != null)
            {
                List<string> result = new List<string>();
                string str = node.Attributes[key].InnerText;
                if (!string.IsNullOrEmpty(str))
                {
                    string[] strs = str.Split(',');
                    foreach (string item in strs)
                    {
                        result.Add(item);
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        public static List<List<int>> GetXmlAttrIntss(XmlNode node, string key)
        {
            //Debug.Log(key);
            int num = 0;
            List<List<int>> result = new List<List<int>>();
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string item in strs)
                {
                    List<int> r = new List<int>();
                    string[] items = item.Split('|');
                    foreach (string s in items)
                    {
                        if (int.TryParse(s, out num))
                        {
                            r.Add(num);
                        }
                    }
                    result.Add(r);
                }
            }
            return result;
        }
        public static int GetXmlInt(XmlNode node)
        {
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.InnerText))
                {
                    return int.Parse(node.InnerText);
                }
            }
            return XmlNullValu;
        }
        //public static Vector3 GetVector(string str)
        //{
        //    string[] strs = str.Split(',');
        //    if (strs.Length == 1)
        //    {
        //        return new Vector3(float.Parse(strs[0]), float.Parse(strs[0]), float.Parse(strs[0]));
        //    }
        //    else
        //    {
        //        return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        //    }
        //}

        //public static string ToString(Vector3 v)
        //{
        //    if (v != Vector3.zero)
        //    {
        //        return v.x + "," + v.y + "," + v.z;
        //    }
        //    return "";
        //}
        public static string ToString(bool b)
        {
            return b ? "yes" : "";
        }
        public static string ToString(List<int> datas)
        {
            if (datas == null || datas.Count <= 0) { return ""; }
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            for (int i = 0; i < datas.Count; i++)
            {
                str.Append(datas[i] + ",");
            }
            return str.ToString().TrimEnd(',');
        }
        public static string ToString(List<string> datas)
        {
            if (datas == null || datas.Count <= 0) { return ""; }
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            for (int i = 0; i < datas.Count; i++)
            {
                str.Append(datas[i] + ",");
            }
            return str.ToString().TrimEnd(',');
        }
        #endregion
    }
}
