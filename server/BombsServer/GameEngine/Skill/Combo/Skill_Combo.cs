using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// 连击
/// </summary>
public class Skill_Combo : Skill_Base, IDeepCopy
{
    public List<List<int>> ids;
    public override void Init(Skill skill, XmlNode data)
    {
        base.Init(skill, data);
        ids = Skill_Manager.GetXmlAttrIntss(data, "ids");
    }

    public override void RunInit(SkillObj Data, Skill skill)
    {
        _skill = skill;
        base.RunInit(Data, skill);
        State = SkillState.Running;
        skill.IsCombo = true;
        skill.Set(this);
    }

    public void RunNext(int index,Skill skill)
    {
        for (int i = 0; i < ids[index].Count; i++)
        {
            //Skill_Manager.Instance.RunModule(ids[index][i], null, skill,this,true);
        }
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End(Skill skill, List<int> NotKillTags)
    {
        if (_skill != null)
        {
            _skill.ReSet_Combo();
        }
        State = SkillState.Over;
    }

    #region 拷贝对象
   public Skill_Base DeepCopy(bool init)
    {
        Skill_Combo data = new Skill_Combo(); //ScriptableObject.CreateInstance<Skill_Combo>();
        if (init) { this.Copy(data); }
        return data;
    }

    public override void Copy(Skill_Base _data)
    {
        Skill_Combo data = _data as Skill_Combo;
        base.Copy(data);
        data.ids = this.ids;
        //=====重置数值====
        data._skill = null;
    }
    #endregion

    //====计算过程使用的数据=====
    public Skill _skill;
}

