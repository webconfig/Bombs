using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Script
{
    public  class ScriptBase
    {
        public int Id;
        public ScriptState State;
        public GameObject entity;
        public virtual void Start()
        {

        }
        public virtual void Update()
        {

        }
        public virtual void Destory()
        {

        }

        //==================
        public virtual void Show()
        {

        }
    }
    public enum ScriptState
    {
        init,
        run,
        destory
    }
}
