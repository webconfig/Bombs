using System;
using System.Collections.Generic;
namespace GameEngine.Script
{
    public  class Skill_Pool : ScriptBase
    {
        public List<Skill> skills = new List<Skill>();
        /// <summary>
        /// 外加技能
        /// </summary>
        public Skill skill_out;

        public void Add(Skill skill)
        {
            skills.Add(skill);
        }

        public override void Update()
        {
            //外加技能
            if (skill_out != null)
            {
                skill_out.Update();
            }
            for (int i = 0; i < skills.Count; i++)
            {
                skills[i].Update();
            }
        }


        public override void Show()
        {
            Log.Info("Skill_Pool:");
            for (int i = 0; i < skills.Count; i++)
            {
                skills[i].Show();
            }
        }
    }
}
