using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer.Game
{
    public class Script
    {
        public ScriptState state;
        public virtual void Init() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void End() { }
    }

    public enum ScriptState
    {
        Enable,
        Disable,
        Destory
    }
}
