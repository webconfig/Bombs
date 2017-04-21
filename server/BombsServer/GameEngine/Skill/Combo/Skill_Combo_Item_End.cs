using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// 连击模块其中一项结束
/// </summary>
public class Skill_Combo_Item_End : Skill_Base, IDeepCopy
{
    public List<int> ids_end ;
    public List<int> ids_next;


    public override void Init(Skill Skill, XmlNode data)
    {
        base.Init(Skill, data);
        ids_end = Skill_Manager.GetXmlAttrInts(data, "ids_end");
        ids_next = Skill_Manager.GetXmlAttrInts(data, "ids_next");
    }

    protected override void SkillDealy(Skill skill)
    {
        if (skill.CombEnd())
        {
            for(int i=0;i<ids_end.Count;i++)
            {
                //Skill_Manager.Instance.RunModule(ids_end[i], null, skill,this, true);
            }
        }
        else
        {
            for (int i = 0; i < ids_next.Count; i++)
            {
                //Skill_Manager.Instance.RunModule(ids_next[i], null, skill, this, true);
            }
        }
        State = SkillState.Over;
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End(Skill skill, List<int> NotKillTags)
    {
        State = SkillState.Over;
    }
    #region 拷贝对象
   public Skill_Base DeepCopy(bool init)
    {
        Skill_Combo_Item_End data = new Skill_Combo_Item_End();// ScriptableObject.CreateInstance<Skill_Combo_Item_End>();
        if (init) { this.Copy(data); }
        return data;
    }

    public override void Copy(Skill_Base _data)
    {
        Skill_Combo_Item_End data = _data as Skill_Combo_Item_End;
        base.Copy(data);
        data.ids_end = this.ids_end;
        data.ids_next = this.ids_next;
    }
    #endregion
}

