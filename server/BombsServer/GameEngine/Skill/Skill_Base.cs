using System;
using System.Collections.Generic;
using System.Xml;
using GameEngine.Script;
namespace GameEngine
{
    /// <summary>
    /// 技能模块基类
    /// </summary>
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
        public List<string> SkillTags;
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
                if (_state == SkillState.RealOver)
                {
                    //#if UNITY_EDITOR
                    //                //Debug.Log("State Set:" + value + ",--" + ",--" + ID+","+test_key);
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
        public string class_type;
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
        public long time_begin = 0;

        #region 时间控制
        /// <summary>
        /// 延迟
        /// </summary>
        public float Dealy = 0;
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
                SkillTags = Skill_Manager.GetXmlAttrStrings(data, "skill_tags");
            }
            else
            {
                SkillTags = new List<string>();
            }
            //====时间===
            Dealy = Skill_Manager.GetXmlAttrFloat(data, "delay");
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
            time_begin = DateTime.Now.Ticks;
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
        public virtual void SkillDealy(Skill skill)
        {

        }

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void SkillUpdate(Skill skill)
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
        public virtual void SkillLifeTime(Skill skill)
        {

        }

        /// <summary>
        /// 强行结束
        /// </summary>
        public virtual void End(Skill Skill, List<int> NotKillTags)
        {

        }

        public void RealOver()
        {
            //if (State == SkillState.RealOver) { Debug.Log("RealOver Error!"); }
            SetUseNum(-1);
            if (UseNum != 0)
            {
                Log.Error(class_type + "->" + UseNum + "--" + ID);
                //Debug.Log("========================================================");
                //for (int i = 0; i < UseNumStrs.Count; i++)
                //{
                //    Debug.Log(UseNumStrs[i]);
                //}
                //Debug.Log("========================================================");
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


        #region 时间控制
        private SkillTimeEvent t_e;
        private void CheckTimeDealy(Skill skill)
        {
            //if (skill._stop) { time_begin += Time.deltaTime; return; }
            //time_begin += OrigSkill.TimeAdd * TimeAddMul;
            time_run = (DateTime.Now.Ticks - time_begin) / 10000000.00f;
            if (time_run >= Dealy)
            {//运行
                t_e = null;
                SkillDealy(skill);
                if (State != SkillState.Over)
                {
                    time_begin = DateTime.Now.Ticks;
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
            //if (skill._stop) { time_begin += Time.deltaTime; return; }
            //time_begin += OrigSkill.TimeAdd * TimeAddMul;
            time_run = (DateTime.Now.Ticks - time_begin) / 10000000.00f;
            if (time_run >= LifeTime)
            {//超过生命周期
                t_e = null;
                SkillLifeTime(skill);
            }
        }
        #endregion

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void Copy(Skill_Base data)
        {
            data.class_type = this.class_type;
            data.ID = this.ID;
            data.Play = this.Play;
            data._state = SkillState.Init;
            data.AddToList = this.AddToList;
            data.SkillTags = this.SkillTags;
            //=====重置数值====
            data.UseNum = 0;
            data.Prev_Data = null;

            //==时间===
            data.Dealy = this.Dealy;
            data.LifeTime = this.LifeTime;
            data.CD = this.CD;
        }

        public void SetUseNum(int num)
        {
            //UseNumStrs.Add(UseNum.ToString() + " , " + num +","+ new System.Diagnostics.StackTrace().ToString());
            UseNum += num;
            //UseNumStrs.Add("result:"+UseNum.ToString());
        }

        public SkillObj GetObj(string p, Skill skill)
        {
            if (string.Equals(p, "user"))
            {
                return OrigObj;
            }
            else if (string.Equals(p, "self"))
            {
                return Prev_Data;
            }
            else if (string.Equals(p, "get"))
            {
                return Prev_Data;
            }
            return null;
        }

        //=====debug=====
        public virtual void Show()
        {
            Log.Info("模块ID：{0}", ID);
        }
    }
    public enum SkillState
    {
        Init = 1,
        Start = 2,
        OverDelay = 3,
        Running = 4,
        Waiting = 5,
        Over = 7,
        Back = 8,
        RealOver = 9
    }
    public delegate void SkillTimeEvent(Skill _skill);
}
