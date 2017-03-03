using System.Xml;
using System.IO;
using UnityEngine;
namespace Assets.Script.Config
{
    public class Scene
    {
        public string name;
        private void Init()
        {
            var Xml = new XmlDocument();
            string FilePath = string.Format("{0}/{1}", Application.persistentDataPath, name);
            if (!File.Exists(FilePath))
            {
                string StreamFilePath;
#if UNITY_EDITOR
                StreamFilePath = string.Format(@"Assets/StreamingAssets/{0}", name);
                File.Copy(StreamFilePath, FilePath);
#else
#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + name);
            while (!loadDb.isDone) { }
            File.WriteAllBytes(FilePath, loadDb.bytes);
#endif
#endif
            }
            string str = File.ReadAllText(FilePath);
            Xml.LoadXml(str);
        }
    }
}
