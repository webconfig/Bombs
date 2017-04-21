using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GameEngine;
using UnityEngine;

public class Skill
{
    #region 基础属性
    /// <summary>
    /// 技能当前等级
    /// </summary>
    public int Level = 1;
    /// <summary>
    /// 技能名称
    /// </summary>
    public string Name;

    /// <summary>
    /// 射程
    /// </summary>
    public float Range;

    public List<int> Elements;

    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillID = -100;
    /// <summary>
    /// 时间缩放
    /// </summary>
    public float TimeAdd = 0f;


    public SkillObj _owner;
    /// <summary>
    /// 技能拥有者
    /// </summary>
    public SkillObj owner
    {
        get
        {
            if (_owner == null)
            {
                Log.Info("=============skill owner is null=============");
            }
            return _owner;

        }
        set { _owner = value; }
    }
    public SkillRunningState _Skill_Runnging_State = SkillRunningState.RealOver;
    public SkillRunningState Skill_Runnging_State
    {
        get
        {
            return _Skill_Runnging_State;
        }
        set
        {
            //Log.Info("111111111111");
            _Skill_Runnging_State = value;
        }
    }
    /// <summary>
    /// 等级
    /// </summary>
    public int rank;

    /// <summary>
    /// 出发类型
    /// </summary>
    public SkillRunType TriggerType;

    /// <summary>
    /// 鼠标抬起触发的id
    /// </summary>
    public List<int> up_ids;
    public SkillType Skill_Type= SkillType.Normal;
    #endregion

    #region CD
    public bool cd_auto;
    /// <summary>
    /// 技能CD
    /// </summary>
    public float CD;
    /// <summary>
    /// 技能CD2
    /// </summary>
    [System.NonSerialized]
    public float CD1;
    /// <summary>
    /// 技能CD2
    /// </summary>
    [System.NonSerialized]
    public float CD2;
    /// <summary>
    /// 技能上次使用的时间
    /// </summary>
    [System.NonSerialized]
    public float LastUse_Time = 0;

    public float cd_now = 0;
    /// <summary>
    /// 技能现在所剩的CD时间
    /// </summary 
    public float Cd_Time_Now
    {
        get
        {
            if (LastUse_Time == 0) { return 0; }
            cd_now = CD - (game.time - LastUse_Time);
            if (cd_now < 0) { cd_now = 0; }
            return cd_now;
        }
    }

    public bool iscd = false;
    /// <summary>
    /// 技能现在是否CD,true表示技能无法使用
    /// </summary>
    public bool IsInCD
    {
        get
        {
            if (LastUse_Time == 0) { iscd = false; }
            else
            {
                iscd = game.time - LastUse_Time < CD;
            }
            return iscd;
        }
    }
    #endregion

    #region 技能容器
    private Skill_Pool _skillpool;
    public Skill_Pool SkillPool
    {
        get
        {
            if (_skillpool == null)
            {
                //_skillpool = owner.sp;
                if (_skillpool == null)
                {
                    Log.Info("error!:" + owner.gameObject);
                }
            }
            return _skillpool;
        }
        set
        {
            _skillpool = value;
        }
    }
    #endregion

    #region 模块
    /// <summary>
    /// 运行中的模块
    /// </summary>
    public List<Skill_Base> Runing_Add;
    /// <summary>
    /// 运行中的模块
    /// </summary>
    public List<Skill_Base> Runing;
    #endregion

    public bool down;
    /// <summary>
    /// 是否鼠标抬起后，执行相关模块
    /// </summary>
    public bool CanUp = true;
    public bool AiUseScript = false;
    public List<int> datas_start;
    public int skill_index=0;
    public Game game;


    public Skill()
    {
        Runing = new List<Skill_Base>();
        Runing_Add = new List<Skill_Base>();
    }

    public Skill(int id, SkillObj obj, int level, SkillType type,int _skill_index)
    {
        skill_index = _skill_index;
        SkillID = id;
        game.skill_manager.SkillCopyData(this);
         CD = CD1;
        owner = obj;
        Level = level;

        Runing = new List<Skill_Base>();
        Runing_Add = new List<Skill_Base>();
        TimeAdd = 0;
        Skill_Type = type;
        SkillPool.Add(this, type);
        datas_start = new List<int>();
        List<Skill_Base> datas = game.skill_manager.GetSkillDataList(SkillID);
        if (datas == null) { return; }
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].Play)
            {
                datas_start.Add(datas[i].ID);
            }
        }
        datas = null;
    }

    public void CopyData(Skill data)
    {
        data.Level = this.Level;
        data.Name = this.Name;
        data.CD = this.CD;
        data.CD1 = this.CD1;
        data.CD2 = this.CD2;
        data.Range = this.Range;
        data.rank = this.rank;
        data.TriggerType = this.TriggerType;
        data.up_ids = this.up_ids;
        data.cd_auto = this.cd_auto;
        data.Elements = this.Elements;
    }

    public bool Use()
    {
        //if (owner.only_use_skill_id > 0 && owner.only_use_skill_id != SkillID) { return false; }
        if (TriggerType !=SkillRunType.click) { return false ; }
        return  RealUse(true);
    }

    public bool DownUse()
    {
        //if (owner.only_use_skill_id > 0 && owner.only_use_skill_id != SkillID) { return false; }
        if (TriggerType != SkillRunType.down) { return false; }
        down = true;
        CanUp = true;
        if(!RealUse(true))
        {
            CanUp = false;
            down = false;
            return false;
        }
        return true;
    }

    public bool UpUse()
    {
        //if (owner.only_use_skill_id > 0 && owner.only_use_skill_id != SkillID) { return false; }
        if (TriggerType != SkillRunType.down) { return false; }
        if (down)
        {
            down = false;
            if (!CanUp) { return false; }
            //===运行模块====
            for (int i = 0; i < up_ids.Count; i++)
            {
                //game.skill_manager.RunModule(up_ids[i], owner, this,this, true);
            }
            return true;
        }
        return false;
    }

    public bool RealUse(bool check)
    {
        //if (owner.only_use_skill_id > 0 && owner.only_use_skill_id != SkillID) { return false; }
        //if (SkillID == 50140)
        //{
        //    Log.Info("使用技能：" + SkillID+"---"+owner.gameObject.name);
        //}
        //Log.Info("使用技能：" + SkillID+"---"+owner.gameObject.name);
        if (!game.skill_manager.CanUseSkill) { return false ; }
        if (IsCombo && !combo_CanInput) { return false; }
        //===================== 为了 兼容========
        //if (check)
        //{
        //    if (owner.character != null)
        //    {
        //        owner.character.IsMoving = false;
        //        if (owner.character.MyState == State.noAction)
        //        {
        //            Log.Error("在NoAction状态释放技能!============");
        //        }
        //    }
        //}
        Skill_Runnging_State = SkillRunningState.Start;
        if (cd_auto) { LastUse_Time = game.time; }
        else { LastUse_Time = 0; }
        //===运行模块====
        for (int i = 0; i < datas_start.Count; i++)
        {
           game.skill_manager.GetObj(datas_start[i], this).Run(null,this);
        }
        if (Skill_Type == SkillType.Normal)
        {
            SkillPool.CurrentSkill = this;
        }
        return true;
    }
    private Skill_Base sb;

    public void Update()
    {
        #region 添加新的
        //Profiler.BeginSample("添加新的");
        if (Runing_Add.Count > 0)
        {
            for (int i = 0; i < Runing_Add.Count; i++)
            {
                sb = Runing_Add[i];
                //if (sb.Remove && sb.RemoveSkillID == SkillOnlyID)
                //{
                //    sb.Remove = false;
                //}
                //else
                //{
                //    Runing.Add(sb);
                //}
            }
            Runing_Add.Clear();
        }
        #endregion
        if (Runing.Count > 0)
        {
            for (int i = 0; i < Runing.Count; i++)
            {
                sb = Runing[i];
                //Profiler.BeginSample("模块：" + sb.ID);
                if (sb.State != SkillState.Over)
                {
                    sb.SkillUpdate(null, this);
                    //if (sb.Remove && sb.RemoveSkillID == SkillOnlyID)
                    //{
                    //    sb.Remove = false;
                    //    Runing.RemoveAt(i);
                    //    i--;
                    //}
                }
                else
                {
                    sb.RealOver();
                    Runing.RemoveAt(i);
                    i--;
                }
                //Profiler.EndSample();
            }
        }
    }

    #region 连击
    public Skill_Combo combo;
    public bool IsCombo = false;
    public bool combo_next = false;
    public int combo_index = 0;
    public float combo_lasttime = 0;
    public bool combo_CanInput = true;

    public bool CombEnd()
    {
        //Log.Info("====CombEnd======");
        combo_CanInput = false;
        combo_index++;//索引++
        if (combo_next && (combo_index < combo.ids.Count))
        {//有输入，继续下一次攻击

            combo_next = false;
            combo.RunNext(combo_index, this);
            combo_lasttime = game.time;
            return false;
        }

        //没有输入
        combo_next = false;
        combo_index = -1;
        combo.SetUseNum(-1);
        combo.State = SkillState.Over;
        combo = null;
        combo_lasttime = game.time;
        return true;
    }

    public void Set(Skill_Combo _combo)
    {
        //Log.Info("set:" + combo_CanInput);
        if (combo_CanInput)
        {
            combo_CanInput = false;
            if (combo == null)
            {//一开始触发
                //Log.Info("set:ok");
                _combo.SetUseNum(1);
                combo = _combo;
                if (combo_lasttime == 0 || game.time - combo_lasttime >= combo.LifeTime)
                {//攻击间隔大于一定时间，重置攻击序列
                    combo_index = 0;
                }
                else
                {
                    combo_index = (combo_index + 1) % combo.ids.Count;
                }
                combo.RunNext(combo_index, this);
            }
            else
            {//连接过程中点击
                if (!combo_next)
                {
                    combo_next = true;
                }
                _combo.State = SkillState.Over;
            }
        }
        else
        {
            _combo.State = SkillState.Over;
        }
    }

    public void SetInput(bool _input)
    {
        combo_CanInput = _input;
    }

    public void ReSet_Combo_Input()
    {
        combo_CanInput = true;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void ReSet_Combo()
    {
        if (combo != null) { combo.SetUseNum(-1); }
        combo = null;
        combo_CanInput = true;
        combo_index = -1;
        combo_lasttime = 0;
    }
    #endregion

    public void ReSet()
    {
        LastUse_Time = 0;
    }
}
public enum SkillRunType
{
    click,
    down
}
public enum SkillRunningState
{
    Init=1,
    Start=2,
    End=4,
    RealOver=5
}
