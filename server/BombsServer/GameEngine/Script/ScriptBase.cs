using System.IO;
namespace GameEngine.Script
{
    public  class ScriptBase
    {
        public ScriptState State= ScriptState.none;
        public GameObject gameobject;
        public virtual void Start()
        {

        }
        public virtual void Update()
        {

        }
        public virtual void Destory()
        {

        }


        public virtual void Serialization(BinaryWriter w)
        {
           
        }

        //==================
        public virtual void Show()
        {

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
