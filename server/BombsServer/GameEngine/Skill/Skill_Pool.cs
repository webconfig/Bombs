using System.Collections.Generic;
using UnityEngine;
using GameEngine;
[Script(10)]
public class Skill_Pool : MonoBehaviour
{
    public List<Skill> Datas = new List<Skill>();
    public Skill skill_out;
    public Skill CurrentSkill;

    public void Init(SkillObj _script)
    {
        skill_out = new Skill();
        skill_out._owner = _script;
    }


    public void Add(Skill skill, SkillType type)
    {
        if (type == SkillType.Normal)
        {
            Skill _data = null, item = null;
            for (int i = 0; i < Datas.Count; i++)
            {
                item = Datas[i];
                if (item.SkillID == skill.SkillID)
                {
                    _data = item;
                    break;
                }
            }
            if (_data == null)
            {
                Datas.Add(skill);
            }
        }
    }

    public void Update()
    {
        //外加技能
        if (skill_out != null)
        {
            skill_out.Update();
        }
        for (int i = 0; i < Datas.Count; i++)
        {
            Datas[i].Update();
        }
    }

    #region Kill
    private bool kill_in = false;
    public void KillAllSkill()
    {
        if (kill_in) { return; }
        kill_in = true;
        for (int i = 0; i < Datas.Count; i++)
        {
            KillSkillModule(null, Datas[i].Runing_Add, Datas[i]);
            KillSkillModule(null, Datas[i].Runing, Datas[i]);
        }
        Skill skill = null;
        if (skill_out != null)
        {
            KillSkillModule(null, skill_out.Runing_Add, skill);
            KillSkillModule(null, skill_out.Runing, skill);
        }
        kill_in = false;
    }
    public void KillSkill(List<int> NotKillTags)
    {
        if (kill_in) { return; }
        kill_in = true;
        for (int i = 0; i < Datas.Count; i++)
        {
            KillSkillModule(NotKillTags, Datas[i].Runing_Add, Datas[i]);
            KillSkillModule(NotKillTags, Datas[i].Runing, Datas[i]);
        }
        if (skill_out != null)
        {
            KillSkillModule(NotKillTags, skill_out.Runing_Add, skill_out);
            KillSkillModule(NotKillTags, skill_out.Runing, skill_out);
        }
        kill_in = false;
    }
    public void KillSkillModule(List<int> NotKillTags, List<Skill_Base> items, Skill skill)
    {
        bool kill = true;
        Skill_Base sb = null;
        for (int j = 0; j < items.Count; j++)
        {
            sb = items[j];
            if (NotKillTags != null)
            {
                kill = true;
                for (int k = 0; k < sb.SkillTags.Count; k++)
                {
                    if (NotKillTags.Contains(sb.SkillTags[k]))
                    {
                        kill = false;
                        break;
                    }
                }
            }
            if (kill)
            {
                if (sb.State == SkillState.Over)
                {
                    sb.RealOver();
                    items.RemoveAt(j);
                    j--;
                    continue;
                }
                sb.End(skill, NotKillTags);
                if (sb.State == SkillState.Over)
                {
                    sb.RealOver();
                    items.RemoveAt(j);
                    j--;
                }
            }
        }
    }
    public void KillSkill2(List<int> KillTags)
    {
        if (kill_in) { return; }
        kill_in = true;
        for (int i = 0; i < Datas.Count; i++)
        {
            KillSkillModule2(KillTags, Datas[i].Runing_Add, Datas[i]);
            KillSkillModule2(KillTags, Datas[i].Runing, Datas[i]);
        }
        if (skill_out != null)
        {
            KillSkillModule2(KillTags, skill_out.Runing_Add, skill_out);
            KillSkillModule2(KillTags, skill_out.Runing, skill_out);
        }
        kill_in = false;
    }
    public void KillSkillModule2(List<int> KillTags, List<Skill_Base> items, Skill skill)
    {
        bool kill = false;
        for (int j = 0; j < items.Count; j++)
        {
            kill = false;
            if (items[j].State == SkillState.Over)
            {
                items[j].RealOver();
                items.RemoveAt(j);
                j--;
                continue;
            }
            if (KillTags != null)
            {
                for (int k = 0; k < items[j].SkillTags.Count; k++)
                {
                    if (KillTags.Contains(items[j].SkillTags[k]))
                    {
                        kill = true;
                        break;
                    }
                }
            }
            if (kill)
            {
                items[j].End(skill, null);
                if (items[j].State == SkillState.Over)
                {
                    items[j].RealOver();
                    items.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    /// <summary>
    /// 被击打断对方的技能
    /// </summary>
    /// <param name="rank"></param>
    /// <param name="NotKillTags"></param>
    /// <param name="HitFail"></param>
    /// <returns></returns>
    public bool HitKill(int rank, List<int> NotKillTags, int default_rank)
    {
        bool result = true; bool current_running = false;
        //AddStr("HitKill：0-->"+rank);
        if (CurrentSkill != null)
        {
            if ((CurrentSkill.Skill_Runnging_State != SkillRunningState.End) && (CurrentSkill.Runing.Count != 0 || CurrentSkill.Runing_Add.Count != 0))
            {//当前技能未结束，且等级比自己高，不能打断
                //AddStr("HitKill：1--当前技能未结束:" + CurrentSkill.rank + "," + rank);
                current_running = true;
                if (CurrentSkill.rank >= rank)
                {
                    //AddStr("HitKill：1--rank < CurrentSkill.rank");
                    result = false;
                }
            }
        }

        //AddStr("HitKill：2--:" + result);
        if (result)
        {//打断当前技能成功
            if (CurrentSkill != null)
            {
                CurrentSkill.CanUp = false;
            }
        }

        for (int i = 0; i < Datas.Count; i++)
        {
            if (Datas[i].rank < rank)
            {//干掉等级比自己低的,但是要保留一些不需要杀掉的模块
                //AddStr("HitKill:31：" + Datas[i].SkillID + "---" + Datas[i].rank);
                Datas[i].down = false;
                KillSkillModule(NotKillTags, Datas[i].Runing_Add, Datas[i]);
                KillSkillModule(NotKillTags, Datas[i].Runing, Datas[i]);
            }
            //else
            //{
            //    AddStr("HitKill:32：" + Datas[i].SkillID + "---" + Datas[i].rank);
            //}
        }

        if (!current_running)
        {//当前技能结束，判断默认等级
            //AddStr("HitKill：4--当前技能结束，判断默认等级" + rank + "," + default_rank);
            result = rank > default_rank;
        }

        //AddStr("HitKill：5--:" + result);
        return result;
    }

    /// <summary>
    ///有没有tag
    /// </summary>
    /// <param name="Tags"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool HasTagModule(List<int> Tags, List<Skill_Base> items)
    {
        for (int j = 0; j < items.Count; j++)
        {
            for (int k = 0; k < items[j].SkillTags.Count; k++)
            {
                if (Tags.Contains(items[j].SkillTags[k]))
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    private SkillItemSvaeData save_items;
    /// <summary>
    /// 存储数据
    /// </summary>
    public SkillItemSvaeData SkillSaveItems
    {
        get
        {
            if (save_items == null) { save_items = new SkillItemSvaeData(); }
            return save_items;
        }
    }
}
[System.Serializable]
public class SkillItemSvaeData
{
    //public Skill_Pool sp;
    private Dictionary<int, Skill_Base> datas = new Dictionary<int, Skill_Base>();
    public void AddItem(int key, Skill_Base item)
    {
        if (!datas.ContainsKey(key))
        {
            //Debug.Log("add new item:" + key + "," + item.class_type_str);
            datas.Add(key, item);
        }
        else
        {
            //Debug.Log("add 2222 item:" + key + "," + item.class_type_str);
            datas[key] = item;
        }
    }
    public bool RemoveItem(int key, Skill_Base item)
    {
        if (!datas.ContainsKey(key))
        {
            //Debug.Log("未存在key：" + key+","+item.class_type_str);
            return false;
        }
        //Debug.Log("remove item:" + key + "," + item.class_type_str);
        datas.Remove(key);
        return true;
    }
    public void RemoveAndEndItem(int key)
    {
        if (datas.ContainsKey(key))
        {
            Skill_Base sb = datas[key];
            sb.End(null, null);
            datas.Remove(key);
        }
    }

    public Skill_Base GetItems(int key)
    {
        if (datas.ContainsKey(key))
        {
            return datas[key];
        }
        return null;
    }
    public void Remove(int key)
    {
        datas.Remove(key);
    }

}
public enum SkillType
{
    Normal,
    Talent,
    Dead
}

