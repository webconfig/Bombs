using System.Xml;
using GameEngine;

namespace BombsServer.Game
{
    public class PlayerControl:Script
    {
        public int test1;
        public override void Init(XmlNode node)
        {
            base.Init(node);
            test1 = GameManager.GetXmlAttrInt(node, "test1");
        }
        public override Script Create()
        {
            PlayerControl pc = new PlayerControl();
            pc.test1 = this.test1;
            return pc;
        }
    }
}
