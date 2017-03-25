using System.Collections.Generic;
using System.Xml;
namespace GameEngine
{
    public class Skill_Anim_Clip : Skill_Base, IDeepCopy
    {
        /// <summary>
        /// 动画
        /// </summary>
        public string Anim;
        /// <summary>
        /// 打断后播放的动画
        /// </summary>
        public string Anim_End;
        /// <summary>
        /// 自己结束后播放的动画
        /// </summary>
        public string Anim_Over;
        /// <summary>
        /// 播放动画的人
        /// </summary>
        public string obj;
        /// <summary>
        /// 当已经在播放这个动画的时候，在播放该动画是否重置
        /// </summary>
        public bool play_reset = true;
        public bool loop;
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init(Skill Skill, XmlNode data)
        {
            base.Init(Skill, data);
            Anim = data.Attributes["anim"].InnerText;
            Anim_End = data.Attributes["anim_end"].InnerText;
            Anim_Over = data.Attributes["anim_over"].InnerText;
            obj = data.Attributes["obj"].InnerText;
            if (data.Attributes["loop"] != null)
            {
                loop = data.Attributes["loop"].InnerText == "yes";
            }
            else
            {
                loop = false;
            }
        }

        public override void SkillDealy(Skill skill)
        {
            State = SkillState.Running;
            Prev_Data = GetObj(obj, skill);
            Log.Info("设置动画：" + Anim);
            Prev_Data.control.anim = Anim;
        }

        public override void End(Skill Skill, List<int> NotKillTags)
        {
            if(State== SkillState.Running)
            {
                Prev_Data.control.anim = Anim_End;
            }
            State = SkillState.Over;
        }

        public override void SkillLifeTime(Skill skill)
        {
            if (State == SkillState.Running)
            {
                Log.Info("SkillLifeTime设置动画：" + Anim_Over);
                Prev_Data.control.anim = Anim_Over;
            }
            State = SkillState.Over;
        }

        #region 拷贝对象
        public Skill_Base DeepCopy(bool init)
        {
            Skill_Anim_Clip data = new Skill_Anim_Clip();
            if (init) { this.Copy(data); }
            return data;
        }
        public override void Copy(Skill_Base _data)
        {
            Skill_Anim_Clip data = _data as Skill_Anim_Clip;
            base.Copy(data);
            data.Anim = this.Anim;
            data.Anim_End = this.Anim_End;
            data.Anim_Over = this.Anim_Over;
            data.obj = this.obj;
            data.play_reset = this.play_reset;
            data.obj = this.obj;
            data.Anim_Over = this.Anim_Over;
            data.loop = this.loop;
        }
        #endregion
    }
}
