using System.Xml;

namespace GameEngine
{
    public class Script
    {
        public ScriptState state;
        public virtual void Init(XmlNode node) { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void End() { }

        public virtual Script Create() { return null; }
        public virtual void Copy(Script script) { }
    }

    public enum ScriptState
    {
        Enable,
        Disable,
        Destory
    }
}
