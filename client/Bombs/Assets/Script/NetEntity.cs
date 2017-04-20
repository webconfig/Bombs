//using UnityEngine;
//using System.IO;

//public class EntityControl : MonoBehaviour
//{
//    public Game game;
//    public Box box;
//    public EntityState _myState = EntityState.idle;
//    public Skill_Pool sp;
//    public float speed = 2;
//    public string anim = "";
//    public Transfrom transform;

//    public void Start()
//    {
//        sp = gameobject.GetComponent<Skill_Pool>();
//        transform = gameobject.GetComponent<Transfrom>();
//    }

//    public void Init(Game _game, float x, float y, float width, float height, Tags tag)
//    {
//        game = _game;
//        box = game.world.Create(x, y, width, height).AddTags(tag);
//    }

//    public void applyInput(int input)
//    {
//        switch (input)
//        {
//            case 1:
//                transform.x += game.delatime * speed;
//                break;
//            case 2:
//                transform.x -= game.delatime * speed;
//                break;
//            case 3:
//                transform.z += game.delatime * speed;
//                break;
//            case 4:
//                transform.z -= game.delatime * speed;
//                break;
//            case 5:
//                if (_myState != EntityState.idle &&
//                    _myState != EntityState.none &&
//                    _myState != EntityState.hit &&
//                    _myState != EntityState.rudderopen &&
//                    _myState != EntityState.rudderfailure) { return; }
//                if (!sp.skills[0].IsInCD)
//                {
//                    sp.skills[0].DownUse();
//                }
//                break;
//        }
//    }

//    public override void Serialization(BinaryWriter w)
//    {
//        w.Write((byte)20);
//        w.Write(speed);
//        w.Write(anim);
//    }

//    //==========调试=============
//    public void ShowInfo()
//    {
//        gameobject.Show();
//    }
//}
//public enum EntityState
//{
//    idle,
//    Walk,
//    isatk,
//    noAction,
//    die,
//    hit,
//    win,
//    none,
//    /// <summary>
//    /// 关闭方向舵
//    /// </summary>
//    rudderfailure,
//    /// <summary>
//    /// 启用方向舵
//    /// </summary>
//    rudderopen,
//    /// <summary>
//    /// 只能走
//    /// </summary>
//    onlyrun
//}


