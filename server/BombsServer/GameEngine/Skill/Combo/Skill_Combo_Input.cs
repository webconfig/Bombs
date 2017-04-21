using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// 连击
/// </summary>
public class Skill_Combo_Input : Skill_Base, IDeepCopy
{
    public bool value;
    public override void Init(Skill skill, XmlNode data)
    {
        base.Init(skill, data);
        if (data.Attributes["value"] == null) { value = true; }
        else
        {
            value = data.Attributes["value"].Value == "yes"? true:false;
        }
    }

    public override void RunInit(SkillObj Data, Skill skill)
    {
        _skill = skill;
        base.RunInit(Data, skill);
        State = SkillState.Running;
    }

    protected override void SkillDealy(Skill skill)
    {
        skill.SetInput(value);
        State = SkillState.Over;
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End(Skill skill, List<int> NotKillTags)
    {
        if (_skill != null)
        {
            _skill.ReSet_Combo_Input();
        }
        State = SkillState.Over;
    }

    #region 拷贝对象
   public Skill_Base DeepCopy(bool init)
    {
        Skill_Combo_Input data = new Skill_Combo_Input(); //ScriptableObject.CreateInstance<Skill_Combo_Input>();
        if (init) { this.Copy(data); }
        return data;
    }

    public override void Copy(Skill_Base _data)
    {
        Skill_Combo_Input data = _data as Skill_Combo_Input;
        base.Copy(data);
        data.value = this.value;
        //=====重置数值====
        data._skill = null;
    }
    #endregion
    //====计算过程使用的数据=====
    public Skill _skill;
}

