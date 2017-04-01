﻿namespace GameEngine.Script
{
    public class SkillObj:ScriptBase
    {
        public Skill_Pool sp;
        public EntityControl control;
        public override void Start()
        {
            sp = gameobject.GetComponent<Skill_Pool>();
            control = gameobject.GetComponent<EntityControl>();
        }
    }
}