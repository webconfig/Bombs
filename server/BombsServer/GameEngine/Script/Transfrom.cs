using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Script
{
    public  class Transfrom:ScriptBase
    {
        public float x = 0;
        public float z = 0;

        public override void Serialization(BinaryWriter w)
        {
            w.Write((byte)10);
            w.Write(x);
            w.Write(z);
        }
    }
}
