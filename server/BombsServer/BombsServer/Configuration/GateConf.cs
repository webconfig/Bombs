using Comm.Util;
using System;

namespace GateServer
{
    public sealed class GateConf : BaseConf
    {
        /// <summary>
        /// channel.conf
        /// </summary>
        public GateConfFile Gate { get; private set; }

        public GateConf()
        {
            this.Gate = new GateConfFile();
        }

        public override void Load()
        {
            this.Gate.Load();
        }
    }
    public class GateConfFile : ConfFile
    {
        public int Port { get; protected set; }

        public void Load()
        {
            this.Require("game.conf");
            this.Port = this.GetInt("port", 11000);
        }
    }
}
