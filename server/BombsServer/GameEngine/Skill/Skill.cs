//using System;
//using System.Collections.Generic;
//using System.Linq;
//using GameEngine.Script;
//namespace GameEngine
//{
//    public class Skill
//    {
//        private Skill_Manager skill_manager;

//        #region 基础属性--Ok
//        /// <summary>
//        /// 技能当前等级
//        /// </summary>
//        public int Level = 1;
//        /// <summary>
//        /// 技能名称
//        /// </summary>
//        public string Name;
//        /// <summary>
//        /// 射程
//        /// </summary>
//        public float Range;
//        /// <summary>
//        /// 技能ID
//        /// </summary>
//        public int SkillID = -100;
//        public int SkillOnlyID = -10;
//        /// <summary>
//        /// 时间缩放
//        /// </summary>
//        public float TimeAdd = 0f;
//        public SkillObj _owner;
//        /// <summary>
//        /// 技能拥有者
//        /// </summary>
//        public SkillObj owner
//        {
//            get
//            {
//                if (_owner == null)
//                {
//                    Log.Error("=============skill owner is null=============");
//                }
//                return _owner;

//            }
//            set { _owner = value; }
//        }
//        public SkillRunningState _Skill_Runnging_State = SkillRunningState.RealOver;
//        public SkillRunningState Skill_Runnging_State
//        {
//            get
//            {
//                return _Skill_Runnging_State;
//            }
//            set
//            {
//                //Debug.Log("111111111111");
//                _Skill_Runnging_State = value;
//            }
//        }
//        /// <summary>
//        /// 等级
//        /// </summary>
//        public int rank;
//        /// <summary>
//        /// 出发类型
//        /// </summary>
//        public SkillRunType TriggerType;
//        /// <summary>
//        /// 鼠标抬起触发的id
//        /// </summary>
//        public List<int> up_ids;
//        public SkillType Skill_Type = SkillType.Normal;
//        #endregion

//        #region CD--Ok
//        public bool cd_auto;
//        /// <summary>
//        /// 技能CD
//        /// </summary>
//        public float CD;
//        /// <summary>
//        /// 技能上次使用的时间
//        /// </summary>
//        [System.NonSerialized]
//        public long LastUse_Time = 0;

//        public float cd_now = 0;
//        /// <summary>
//        /// 技能现在所剩的CD时间
//        /// </summary 
//        public float Cd_Time_Now
//        {
//            get
//            {
//                if (LastUse_Time == 0) { return 0; }
//                cd_now = CD - (DateTime.Now.Ticks - LastUse_Time) / 10000000.00f;
//                if (cd_now < 0) { cd_now = 0; }
//                return cd_now;
//            }
//        }

//        public bool iscd = false;
//        /// <summary>
//        /// 技能现在是否CD,true表示技能无法使用
//        /// </summary>
//        public bool IsInCD
//        {
//            get
//            {
//                if (LastUse_Time == 0) { iscd = false; }
//                else
//                {
//                    iscd = (DateTime.Now.Ticks - LastUse_Time) / 10000000.00f < CD;
//                }
//                return iscd;
//            }
//        }
//        #endregion

//        #region 技能容器--OK
//        private Skill_Pool _skillpool;
//        public Skill_Pool SkillPool
//        {
//            get
//            {
//                if (_skillpool == null)
//                {
//                    _skillpool = owner.sp;
//                }
//                return _skillpool;
//            }
//            set
//            {
//                _skillpool = value;
//            }
//        }
//        #endregion

//        #region 模块--OK
//        /// <summary>
//        /// 运行中的模块
//        /// </summary>
//        public List<Skill_Base> Runing_Add;
//        //public List<Skill_Base> Runing_Remove;
//        /// <summary>
//        /// 运行中的模块
//        /// </summary>
//        public List<Skill_Base> Runing;
//        #endregion

//        public bool down;
//        /// <summary>
//        /// 是否鼠标抬起后，执行相关模块
//        /// </summary>
//        public bool CanUp = true;
//        public List<int> datas_start;

//        #region 构造函数
//        public Skill() { }
//        public Skill(Skill_Manager _skill_manager)
//        {
//            skill_manager = _skill_manager;
//            Runing = new List<Skill_Base>();
//            Runing_Add = new List<Skill_Base>();
//            SkillOnlyID = skill_manager.GetSkillIndex();
//        }
//        public Skill(int id, SkillObj obj, int level, Skill_Manager _skill_manager, SkillType type)
//        {
//            skill_manager = _skill_manager;
//            SkillOnlyID = skill_manager.GetSkillIndex();
//            SkillID = id;
//            skill_manager.SkillCopyData(this);
//            owner = obj;
//            Level = level;

//            Runing = new List<Skill_Base>();
//            Runing_Add = new List<Skill_Base>();
//            TimeAdd = 0;
//            Skill_Type = type;
//            SkillPool.Add(this);
//            datas_start = new List<int>();
//            List<Skill_Base> datas = skill_manager.GetSkillDataList(SkillID);
//            if (datas == null) { return; }
//            for (int i = 0; i < datas.Count; i++)
//            {
//                if (datas[i].Play)
//                {
//                    datas_start.Add(datas[i].ID);
//                }
//            }
//            datas = null;
//        }
//        #endregion

//        #region 释放技能
//        public bool Use()
//        {
//            if (TriggerType != SkillRunType.click) { return false; }
//            return RealUse();
//        }

//        public bool DownUse()
//        {
//            if (TriggerType != SkillRunType.down) { return false; }
//            down = true;
//            CanUp = true;
//            if (!RealUse())
//            {
//                CanUp = false;
//                down = false;
//                return false;
//            }
//            return true;
//        }

//        public bool UpUse()
//        {
//            if (TriggerType != SkillRunType.down) { return false; }
//            if (down)
//            {
//                down = false;
//                if (!CanUp) { return false; }
//                //===运行模块====
//                for (int i = 0; i < up_ids.Count; i++)
//                {
//                    //Skill_Manager.Instance.RunModule(up_ids[i], owner, this, this, true);
//                }
//                return true;
//            }
//            return false;
//        }

//        public bool RealUse()
//        {
//            Log.Info("使用技能：" + SkillID);
//            if (!skill_manager.CanUseSkill) { return false; }
//            Skill_Runnging_State = SkillRunningState.Start;
//            //==开始CD
//            if (cd_auto) { LastUse_Time = DateTime.Now.Ticks; }
//            else { LastUse_Time = 0; }
//            //===运行模块====
//            for (int i = 0; i < datas_start.Count; i++)
//            {
//                skill_manager.GetObj(datas_start[i], this).Run(null, this);
//            }
//            return true;
//        }
//        #endregion

//        private Skill_Base sb;
//        public void Update()
//        {
//            #region 添加新的
//            //Profiler.BeginSample("添加新的");
//            if (Runing_Add.Count > 0)
//            {
//                for (int i = 0; i < Runing_Add.Count; i++)
//                {
//                    sb = Runing_Add[i];
//                    if (sb.Remove && sb.RemoveSkillID == SkillOnlyID)
//                    {
//                        sb.Remove = false;
//                    }
//                    else
//                    {
//                        Runing.Add(sb);
//                    }
//                }
//                Runing_Add.Clear();
//            }
//            #endregion
//            if (Runing.Count > 0)
//            {
//                for (int i = 0; i < Runing.Count; i++)
//                {
//                    sb = Runing[i];
//                    if (sb.State != SkillState.Over)
//                    {
//                        sb.SkillUpdate(this);
//                        if (sb.Remove && sb.RemoveSkillID == SkillOnlyID)
//                        {
//                            sb.Remove = false;
//                            Runing.RemoveAt(i);
//                            i--;
//                        }
//                    }
//                    else
//                    {
//                        sb.RealOver();
//                        Runing.RemoveAt(i);
//                        i--;
//                    }
//                }
//            }
//        }

//        //=====拷贝=====
//        public void CopyData(Skill data)
//        {
//            data.Level = this.Level;
//            data.Name = this.Name;
//            data.CD = this.CD;
//            data.Range = this.Range;
//            data.rank = this.rank;
//            data.TriggerType = this.TriggerType;
//            data.up_ids = this.up_ids;
//            data.cd_auto = this.cd_auto;
//        }

//        //====debug=====
//        public void Show()
//        {
//            Log.Info("技能：" + SkillID);
//            Log.Info("运行中的模块：");
//            for (int i = 0; i < Runing.Count; i++)
//            {
//                Runing[i].Show();
//            }
//        }
//    }
//    public enum SkillRunType
//    {
//        click,
//        down
//    }
//    public enum SkillRunningState
//    {
//        Init = 1,
//        Start = 2,
//        End = 4,
//        RealOver = 5
//    }
//    public enum SkillType
//    {
//        Normal,
//        Talent,
//        Dead
//    }
//}
