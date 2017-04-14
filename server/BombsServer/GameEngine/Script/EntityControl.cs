using System.IO;
using GameEngine.Network;

namespace GameEngine.Script
{
    public class EntityControl : ScriptBase
    {
        public Client client;
        public EntityState _myState = EntityState.idle;
        public Skill_Pool sp;
        public float speed = 2;
        public string anim = "";
        public Transfrom transform;

        public override void Start()
        {
            sp = gameobject.GetComponent<Skill_Pool>();
            transform = gameobject.GetComponent<Transfrom>();
        }

        public void applyInput(Input input)
        {
            switch ((KeyCode)input.keycode)
            {
                case KeyCode.D:
                    transform.x += input.lagMs * speed;
                    break;
                case KeyCode.A:
                    transform.x -= input.lagMs * speed;
                    break;
                case KeyCode.W:
                    transform.z += input.lagMs * speed;
                    break;
                case KeyCode.S:
                    transform.z -= input.lagMs * speed;
                    break;
                case KeyCode.J:
                    if (_myState != EntityState.idle && 
                        _myState != EntityState.none && 
                        _myState != EntityState.hit&&
                        _myState != EntityState.rudderopen && 
                        _myState != EntityState.rudderfailure) { return; }
                    if (!sp.skills[0].IsInCD)
                    {
                        sp.skills[0].DownUse();
                    }
                    break;
            }
        }

        public override void Serialization(BinaryWriter w)
        {
            w.Write((byte)20);
            w.Write(speed);
            w.Write(anim);
        }

        //==========调试=============
        public void ShowInfo()
        {
            gameobject.Show();
        }
    }
    public enum EntityState
    {
        idle,
        Walk,
        isatk,
        noAction,
        die,
        hit,
        win,
        none,
        /// <summary>
        /// 关闭方向舵
        /// </summary>
        rudderfailure,
        /// <summary>
        /// 启用方向舵
        /// </summary>
        rudderopen,
        /// <summary>
        /// 只能走
        /// </summary>
        onlyrun
    }
}
