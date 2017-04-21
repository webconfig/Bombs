using System.IO;
using System;
using System.Reflection;

namespace UnityEngine
{
    public class MonoBehaviour
    {
        public ScriptState State = ScriptState.none;
        public GameObject gameObject;
        public Transform transform;
        public MethodInfo _Start, _Update;
        public void Init()
        {
            _Start = this.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            _Update = this.GetType().GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void StartFun()
        {
            _Start.Invoke(this,null);
        }
        public void UpdateFun()
        {
            _Update.Invoke(this, null);
        }
    }
    public enum ScriptState
    {
        none,
        init,
        run,
        destory
    }
}
