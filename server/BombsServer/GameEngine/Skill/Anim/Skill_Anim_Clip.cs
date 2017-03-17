using System.Collections.Generic;
using System.Xml;
namespace GameEngine
{
    public class Skill_Anim_Clip : Skill_Base
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
            base.SkillDealy(skill);
        }

        public override void End(Skill Skill, List<int> NotKillTags)
        {
            State = SkillState.Over;
        }

        public override void SkillLifeTime(Skill skill)
        {
            State = SkillState.Over;
        }

    }
}
