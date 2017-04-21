using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    [System.Serializable]
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ScriptAttribute : Attribute
    {
        public int id;
        public ScriptAttribute(int _id)
        {
            this.id = _id;
        }
    }
}
