using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using GameEngine;

public class Skill_Base
{
    /// <summary>
    /// 引用计数器
    /// </summary>
    public int UseNum = 0;
    /// <summary>
    /// 模块ID
    /// </summary>
    public int ID;
    /// <summary>
    /// 标签
    /// </summary>
    public List<int> SkillTags;
    /// <summary>
    /// 标签
    /// </summary>
    public List<string> SkillTagsStrs;
    public SkillState _state;
    /// <summary>
    /// 技能状态
    /// </summary>
    public SkillState State
    {
        get
        {
            return _state;
        }
        set
        {
//#if UNITY_EDITOR
//            StatRunDatas.Add(value.ToString() + "----" + new System.Diagnostics.StackTrace().ToString());
//#endif
            if (_state== SkillState.RealOver)
            {
//#if UNITY_EDITOR
//                //Log.Info("State Set:" + value + ",--" + ",--" + ID+","+test_key);
//                //UnityEditor.EditorApplication.isPaused = true;
//#endif
                return;
            }
            _state = value;
        }
    }
    /// <summary>
    /// 是否添加到列表
    /// </summary>
    public bool AddToList = true;
    public bool Play = false;
    public SkillObj Prev_Data;
    public int class_type;
    public string class_type_str;
    public int TimeAddMul = 1;
    public bool IsHit = false;
    /// <summary>
    /// 释放技能的人
    /// </summary>
    public SkillObj OrigObj;
    /// <summary>
    /// 释放技能的技能
    /// </summary>
    public Skill OrigSkill;

    //====计算过程使用的数据=====
    public float time_run = 0;
    public float time_begin = 0;

    #region 时间控制
    /// <summary>
    /// 延迟
    /// </summary>
    public float Dealy = 0;

    #region 延迟随机
    public bool HasDealy2 = false;
    /// <summary>
    /// 延迟1
    /// </summary>
    [System.NonSerialized]
    public float Dealy1 = 0;
    /// <summary>
    /// 延迟2
    /// </summary>
    [System.NonSerialized]
    public float Dealy2 = 0;
    #endregion

    /// <summary>
    /// 运行时间
    /// </summary>
    [System.NonSerialized]
    public float LifeTime;
    [System.NonSerialized]
    public float CD;
    #endregion

    #region 删除
    public bool Remove = false;
    public int RemoveSkillID = 0;
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(Skill skill, XmlNode data)
    {
        ID = Skill_Manager.GetXmlAttrInt(data, "id");
        Play = false;
        if (data.Attributes["play"].InnerText == "auto")
        {
            Play = true;
        }
        
        if (data.Attributes["skill_tags"] != null)
        {
            SkillTagsStrs = Skill_Manager.GetXmlAttrStrings(data, "skill_tags");
        }
        else
        {
            SkillTagsStrs = new List<string>();
        }
        SkillTagsStrs.Add(class_type_str);
        SkillTags = new List<int>();
        for (int i = 0; i < SkillTagsStrs.Count; i++)
        {
            SkillTags.Add(SkillTagsStrs[i].GetHashCode());
        }

        //====时间===
        Dealy = Skill_Manager.GetXmlAttrFloat(data, "delay");
        if (data.Attributes["delay2"] != null)
        {
            HasDealy2 = true;
            Dealy1 = Dealy;
            Dealy2 = Skill_Manager.GetXmlAttrFloat(data, "delay2");
        }
        else
        {
            HasDealy2 = false;
        }
        LifeTime = Skill_Manager.GetXmlAttrFloat(data, "lifetime");
        CD = Skill_Manager.GetXmlAttrFloat(data, "cd");
    }
    public Skill skill_new;
    public void Run(SkillObj Data, Skill skill)
    {
        skill_new = skill;
        this.RunInit(Data, skill_new);
        if (State != SkillState.Over)
        {
            //===================
            t_e = null;
            if (Dealy <= 0)
            {//马上执行
                if (LifeTime > 0)
                {
                    t_e = CheckLifeTime;
                }
                SkillDealy(skill_new);
            }
            else
            {
                t_e = CheckTimeDealy;
            }
        }
        skill_new = null;
    }

    /// <summary>
    /// 运行
    /// </summary>
    public virtual void RunInit(SkillObj Data, Skill skill)
    {
        skill_new = skill;
        //====时间===
        time_begin = skill.game.time;
        Dealy = Dealy1;
        //if (HasDealy2)
        //{
        //    Dealy = UnityEngine.Random.Range(Dealy1 * 10000.00f, Dealy2 * 10000.00f) / 10000.00f;
        //}
        SetUseNum(1);
        Prev_Data = Data;
        _state = SkillState.Init;
        if (AddToList)
        {
            skill.Runing_Add.Add(this);
        }
    }


    /// <summary>
    /// 延迟到
    /// </summary>
    /// <param name="skill"></param>
    protected virtual void SkillDealy(Skill skill)
    {

    }

    /// <summary>
    /// 更新
    /// </summary>
    public virtual void SkillUpdate(object data, Skill skill)
    {
        if (State != SkillState.Over)
        {
            if (t_e != null) { t_e(skill); }
        }
    }

    /// <summary>
    /// 生命周期到
    /// </summary>
    /// <param name="skill"></param>
    protected virtual void SkillLifeTime(Skill skill)
    {

    }

    private SkillTimeEvent t_e;

    private void CheckTimeDealy(Skill skill)
    {
        //if (skill._stop) { time_begin += skill.game.deltaTime; return; }
        time_begin += OrigSkill.TimeAdd * TimeAddMul;
        time_run = skill.game.time - time_begin;
        if (time_run >= Dealy)
        {//运行
            t_e = null;
            //Profiler.BeginSample("SkillDealy");
            SkillDealy(skill);
            //Profiler.EndSample();
            if (State != SkillState.Over)
            {
                time_begin = skill.game.time;
                time_run = 0;
                //t_e = CheckLifeTime;
                if (LifeTime > 0)
                {
                    t_e = CheckLifeTime;
                }
            }
        }
    }

    private void CheckLifeTime(Skill skill)
    {
        //if (skill._stop) { time_begin += skill.game.deltaTime; return; }
        time_begin += OrigSkill.TimeAdd * TimeAddMul;
        time_run = skill.game.time - time_begin;
        if (time_run >= LifeTime)
        {//超过生命周期
            t_e = null;
            //Profiler.BeginSample("SkillLifeTime");
            SkillLifeTime(skill);
            //Profiler.EndSample();
        }
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public virtual void End(Skill Skill, List<int> NotKillTags)
    {

    }

    public virtual void Frozen(Skill skill)
    {
       
    }
    public virtual void UnFrozen(Skill skill)
    {
       
    }

    public void RealOver()
    {
        //if (State == SkillState.RealOver) { Log.Info("RealOver Error!"); }
        SetUseNum(-1);
        if (UseNum != 0)
        {
            Log.Info(class_type_str + "->" + UseNum + "--" + ID);
            //Log.Info("========================================================");
            //for (int i = 0; i < UseNumStrs.Count; i++)
            //{
            //    Log.Info(UseNumStrs[i]);
            //}
            //Log.Info("========================================================");
            UseNum = 0;
            //UnityEditor.EditorApplication.isPaused = true;
        }
        if (t_e != null)
        {
            t_e = null;
        }
        if (Prev_Data != null)
        {
            Prev_Data = null;
        }
        OrigObj = null;
        OrigSkill = null;
        //Skill_Manager.Instance.BackObj(this);
        State = SkillState.RealOver;
        return;
    }

    /// <summary>
    /// 拷贝数据
    /// </summary>
    /// <param name="data"></param>
    public virtual void Copy(Skill_Base data)
    {
        data.class_type = this.class_type;
        data.class_type_str = this.class_type_str;
        data.ID = this.ID;
        data.Play = this.Play;
        data._state = SkillState.Init;
        data.AddToList = this.AddToList;
        data.SkillTags = this.SkillTags;
        data.SkillTagsStrs = this.SkillTagsStrs;
        //=====重置数值====
        data.UseNum = 0;
        data.Prev_Data = null;
        data.IsHit = false;
        
        //==时间===
        data.Dealy = this.Dealy;
        data.Dealy1 = this.Dealy1;
        data.Dealy2 = this.Dealy2;
        data.HasDealy2 = this.HasDealy2;
        data.LifeTime = this.LifeTime;
        data.CD = this.CD;
        data.TimeAddMul=this.TimeAddMul;
    }

    //public List<string> UseNumStrs = new List<string>();
    public void SetUseNum(int num)
    {
        //UseNumStrs.Add(UseNum.ToString() + " , " + num +","+ new System.Diagnostics.StackTrace().ToString());
        UseNum += num;
        //UseNumStrs.Add("result:"+UseNum.ToString());
    }

//    public Vector3 GetDir(int key, string p, Skill skill, SkillObj player)
//    {
//        if (key == Skill_Manager.user_key)//(string.Equals(p, "user"))
//        {
//            return OrigObj.transform.position;
//        }
//        else if (key == Skill_Manager.self_key)// if (string.Equals(p, "self"))
//        {
//            return player.transform.position;
//        }
//        else if (key == Skill_Manager.user_up_key)//if (string.Equals(p, "user_up"))
//        {
//            return OrigObj.transform.up;
//        }
//        else if (key == Skill_Manager.user_forward_key)//if (string.Equals(p, "user_forward"))
//        {
//            return OrigObj.transform.forward;
//        }
//        else if (key == Skill_Manager.self_up_key)//if (string.Equals(p, "self_up"))
//        {
//            return player.transform.up;
//        }
//        else if (key == Skill_Manager.self_forward_key)//if (string.Equals(p, "self_forward"))
//        {
//            return player.transform.forward;
//        }
//        else if (key == Skill_Manager.camera_key)//if (string.Equals(p, "camera"))
//        {
//            return Skill_Manager.Instance.camera_script.transform.position;// DamagePadManager.Instance.UICamera.transform.position;
//        }
//        else
//        {
//            SkillObj obj = Skill_Manager.Instance.GetObjectByName(Skill_Manager.Instance.GetSaveID(this, p));
//            if (obj!=null)
//            {
//                return obj.transform.position;
//            }
//            return Vector3.zero;
//        }

//    }
//    public SkillObj GetObj(int key, string p, Skill skill)
//    {
//        if (key==Skill_Manager.user_key)//(string.Equals(p, "user"))
//        {
//            return OrigObj;
//        }
//        else if (key==Skill_Manager.self_key)//(string.Equals(p, "self"))
//        {
//           return Prev_Data;
//        }
//        else if (key == Skill_Manager.get_key) //if (string.Equals(p, "get"))
//        {
//           return Prev_Data;
//        }
////        else if (key == Skill_Manager.current_dragon_key) //if (string.Equals(p, "get"))
////        {
////            if(OrigObj.sp.player_type==10)
////            {//正常副本的玩家
////                return PlayerControl.Instance.curDragon.so;
////            }
////            else if (OrigObj.sp.player_type == 20)
////            {//Ai对打的玩家
////                return PvP_AI_Manager.Instance.Player1.curDragon.so;
////            }
////            else if (OrigObj.sp.player_type == 21)
////            {//Ai对打的对手
////                return PvP_AI_Manager.Instance.Player2.curDragon.so;
////            }
////            else if (OrigObj.sp.player_type == 30)
////            {//玩家打AI中的玩家
////                return PvP_Player_Manager.Instance.Player1.curDragon.so;
////            }
////            else if (OrigObj.sp.player_type == 31)
////            {//玩家打AI中的AI
////                return PvP_Player_Manager.Instance.Player2.curDragon.so;
////            }
////            else
////            {
////                return null;
////            } 
////        }
////        else if (key == Skill_Manager.other_key) //if (string.Equals(p, "other"))
////        {
////#if UNITY_EDITOR
////            if (TestSkill.Instance != null)
////            {
////                GameObject go = SaveObjectManager.Instance.GetObjectByName("system", "R_zj_01_ride");
////                if (go != null)
////                {
////                    SkillObj so = go.GetComponent<SkillObj>();
////                    if (so == null) { so = go.AddComponent<SkillObj>(); so.Init(); }
////                    return so;
////                }
////            }
////#endif
////            //if (PlayerControl.Instance != null && PlayerControl.Instance.human != null)
////            //{
////            //    return PlayerControl.Instance.human.so;
////            //}
////            return OrigObj.character.sp_human;
////        }
////        else if (key == Skill_Manager.camera_key) //if (string.Equals(p, "camera"))
////        {
////            return Skill_Manager.Instance.camera_script; 
////        }
//        else
//        {
//            return Skill_Manager.Instance.GetObjectByName(Skill_Manager.Instance.GetSaveID(this, p));
//        }
//    }
}
public enum SkillState
{
    Init=1,
    Start=2,
    OverDelay=3,
    Running=4,
    Waiting=5,
    Over=7,
    Back=8,
    RealOver=9
}
public delegate void SkillTimeEvent(Skill _skill);


