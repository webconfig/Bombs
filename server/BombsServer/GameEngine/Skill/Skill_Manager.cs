using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System;
using GameEngine;

public class Skill_Manager : MonoBehaviour
{
    #region 固定的数据
    /// <summary>
    /// 摩擦系数
    /// </summary>
    public float FrictionNum_New = 3f;
    /// <summary>
    /// 重力系数
    /// </summary>
    public float GravityNum_New = 9.8f;
    public int DamagePadNum = 15;
    /// <summary>
    /// 跑步动画名称
    /// </summary>
    public string RunAnimName = "run";
    #endregion

    #region 运行时数据
    /// <summary>
    /// 是否可以使用技能
    /// </summary>
    public bool CanUseSkill = true;
    #endregion

    public void SkillManagerInit()
    {
        try
        {
            //XmlInit();
            //InitSkill();
        }
        catch (Exception ex)
        {
            Log.Error("重大bug---〉" + ex.ToString());
        }

    }

    public void SkillStart()
    {
        Log.Info("===========SkillStart============");
        CanUseSkill = true;
        CreateModulePool();
    }

    public void SkillEnd()
    {
        //string k = "1";
        CanUseSkill = false;
        //====打断所有技能===
        //if (BattleInit.instance != null) { BattleInit.instance.Over(); }
        //FrontManager.Instance.Clear();
        //if (UIDialogueManager.Instance != null)
        //{
        //    UIDialogueManager.Instance.EndDialogue();
        //}
        //Skill_Pool.ResetAllSkill();
        //Skill_Pool.AllSkill.Clear();

        //save_objs.Clear();
        //ClearObjs();

        //buff_pools.Clear();
        //CDDS.Clear();
        //ClearSkillModulePool();

        //k += "2";
        ////============清理资源============
        //ClearBtn();
        //ClearAudio();
        //ClearNull();
        //Skill_Audio_Manager.Instance.Clear();
        //BloodManager.Instance.DestoryAll();
        //CountManager.Instance.DestoryAll();
        //DamagePadManager.Instance.DestoryAll();
        //ObjPoolManager.Instance.Clear(ObjItemType.Battle.ToString());
        //ObjPoolManager.Instance.Clear(ObjItemType.Skill.ToString());
        //ObjPoolManager.Instance.Clear(ObjItemType.UI.ToString());
        //if (Skill_Pool_Manager.Instance != null)
        //{
        //    Skill_Pool_Manager.Instance.Clear();
        //}
        //RescourceManager.Instance.DestoryAll();
        //k += "3";
        //Log.Info(string.Format("=============战斗结束{0}==========", k));
    }



    #region 资源
    private Dictionary<XmlType, XmlData> xmls = new Dictionary<XmlType, XmlData>();
    private void XmlInit()
    {
        xmls.Add(XmlType.Model, new XmlData(@"Xml/resource/model"));
        xmls.Add(XmlType.Audio, new XmlData(@"Xml/resource/audio"));
        xmls.Add(XmlType.Skill, new XmlData(@"Xml/skills"));
        xmls.Add(XmlType.Damage, new XmlData(@"Xml/damage"));
        xmls.Add(XmlType.SkillResource, new XmlData(@"Xml/skillresource"));
        xmls.Add(XmlType.InitClass, new XmlData(@"Xml/skillsInit"));
        LoadXml();
    }
    private void LoadXml()
    {
        foreach (var item in xmls)
        {
            if (item.Value.Path.ToLower().IndexOf("http") >= 0)
            {//网络加载

            }
            else
            {//本地加载
                string str = System.IO.File.ReadAllText(item.Value.Path);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(str);
                item.Value.Data = xml;
            }
        }
    }

    public void LoadSkills(List<int> ids)
    {
        //List<ObjItem> Objs = new List<ObjItem>();
        //Dictionary<string, ObjItem> effect_ids = new Dictionary<string, ObjItem>();
        //Dictionary<string, ObjItem> character_ids = new Dictionary<string, ObjItem>();
        //for (int i = 0; i < ids.Count; i++)
        //{
        //    SkillResource(GetXmlData(XmlType.SkillResource), GetXmlData(XmlType.Audio), ids[i], effect_ids, character_ids);
        //}
        //Objs.AddRange(effect_ids.Values.ToList());
        //Objs.AddRange(character_ids.Values.ToList());
        //if (Skill_Pool_Manager.Instance == null)
        //{
        //    Skill_Pool_Manager.Init();
        //}
        //Skill_Pool_Manager.Instance.Add(Objs);
    }
    //private void SkillResource(XmlDocument xml_skill_resource, XmlDocument xml_audio, int skillid, Dictionary<string, ObjItem> effect_ids, Dictionary<string, ObjItem> character_ids)
    //{
    ////技能本身的特效
    //XmlNode node = xml_skill_resource.SelectSingleNode(@"/items/skill[@id='" + skillid + "']");
    //if (node == null) { return; }

    //#region 特效资源
    //string str = node.Attributes["effect"].InnerText;
    //if (!string.IsNullOrEmpty(str))
    //{
    //    string[] datas = str.Split(',');
    //    for (int i = 0; i < datas.Length; i++)
    //    {
    //        string[] valus = datas[i].Split('*');
    //        ObjItem item_skill = new ObjItem();
    //        item_skill.name = valus[0];
    //        item_skill.name_key = item_skill.name.GetHashCode();
    //        item_skill.Num = int.Parse(valus[1]);
    //        item_skill.Max = 10;//这一类的特性最多可以有多少个
    //        if (!effect_ids.ContainsKey(item_skill.name))
    //        {
    //            item_skill.obj_id = valus[0];
    //            item_skill.NeedInit = true;
    //            item_skill.MonoScript = new List<Type>() { typeof(EffectControl) };
    //            item_skill.MonoScript.Add(typeof(SkillObj));
    //            if (HasEffect(skillid, item_skill.obj_id))
    //            {
    //                item_skill.MonoScript.Add(typeof(Skill_Pool));
    //            }
    //            effect_ids.Add(item_skill.name, item_skill);
    //        }
    //        else if (item_skill.Num > effect_ids[item_skill.name].Num)
    //        {
    //            item_skill.obj_id = valus[0];
    //            item_skill.NeedInit = true;
    //            item_skill.MonoScript = new List<Type>() { typeof(EffectControl) };
    //            item_skill.MonoScript.Add(typeof(SkillObj));
    //            if (HasEffect(skillid, item_skill.obj_id))
    //            {
    //                item_skill.MonoScript.Add(typeof(Skill_Pool));
    //            }
    //            effect_ids[item_skill.name] = item_skill;
    //        }
    //    }
    //}
    //#endregion

    //#region 怪物资源
    //XmlNode item = null;
    //XmlNodeList nodes = node.SelectNodes("character");
    //for (int k = 0; k < nodes.Count; k++)
    //{
    //    item = nodes[k];
    //    ObjItem item_skill = new ObjItem();
    //    item_skill.name = item.Attributes["name"].InnerText;
    //    item_skill.name_key = item_skill.name.GetHashCode();
    //    item_skill.Num = Skill_Manager.GetXmlAttrInt(item, "num");
    //    item_skill.MonoScript = new List<Type>() { typeof(SkillObj) };
    //    if (!character_ids.ContainsKey(item_skill.name))
    //    {
    //        item_skill.obj_id = item.Attributes["id"].InnerText;
    //        item_skill.datas = new Dictionary<string, string>();
    //        string[] strs = item.Attributes["datas"].InnerText.Split(',');
    //        string[] vs;
    //        if (strs != null && strs.Length > 0)
    //        {
    //            for (int i = 0; i < strs.Length; i++)
    //            {
    //                vs = strs[i].Split(':');
    //                item_skill.datas.Add(vs[0], vs[1]);
    //            }
    //        }
    //        item_skill.NeedInit = true;
    //        character_ids.Add(item_skill.name, item_skill);
    //    }
    //    else if (character_ids[item_skill.name].Num < item_skill.Num)
    //    {
    //        item_skill.obj_id = item.Attributes["id"].InnerText;
    //        item_skill.datas = new Dictionary<string, string>();
    //        string[] strs = item.Attributes["datas"].InnerText.Split(',');
    //        string[] vs;
    //        if (strs != null && strs.Length > 0)
    //        {
    //            for (int i = 0; i < strs.Length; i++)
    //            {
    //                vs = strs[i].Split(':');
    //                item_skill.datas.Add(vs[0], vs[1]);
    //            }
    //        }
    //        item_skill.NeedInit = true;
    //        character_ids[item_skill.name] = item_skill;
    //    }
    //}
    //#endregion

    //#region 音效资源
    //string audio = node.Attributes["audio"].InnerText;
    //if (!string.IsNullOrEmpty(audio))
    //{
    //    string[] audios = audio.Split(',');
    //    for (int i = 0; i < audios.Length; i++)
    //    {
    //        string[] valus = audios[i].Split('*');
    //        Skill_Audio_Manager.Instance.Add(valus[0]);
    //    }
    //}
    //#endregion
    //}
    /// <summary>
    /// 获取xml字符串
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public XmlDocument GetXmlData(XmlType type)
    {
        return xmls[type].Data;
    }

    #endregion

    /// <summary>
    /// 获取一个附加技能
    /// </summary>
    /// <param name="target"></param>
    /// <param name="CurrentSkill"></param>
    /// <returns></returns>
    public Skill GetOutSkill(SkillObj target)
    {
        //Skill_Pool sp = target.sp;
        //if (sp == null)
        //{
        //    //Log.Info(target.gameObject.name + "--> sp is null!");
        //    sp = Skill_Pool.AddSkillPool(target);
        //}
        //if (sp.skill_out._owner == null)
        //{
        //    Log.Info(target.gameObject.name + "--> out skill owner is null!");
        //    sp.skill_out._owner = target;
        //}
        //return sp.skill_out;
        return null;
    }


    #region 技能全局存储
    public Dictionary<int, List<SkillObj>> save_objs = new Dictionary<int, List<SkillObj>>();
    public void SaveObj(int key, SkillObj obj)
    {
        if (save_objs.ContainsKey(key))
        {
            save_objs[key].Add(obj);
        }
        else
        {
            save_objs.Add(key, new List<SkillObj>() { obj });
        }
    }
    public void SaveOnlyOne(int key, SkillObj obj)
    {
        if (save_objs.ContainsKey(key))
        {
            List<SkillObj> objs = save_objs[key];
            objs.Clear();
            objs.Add(obj);
        }
        else
        {
            save_objs.Add(key, new List<SkillObj>() { obj });
        }
    }
    public void RemoveObject(int key)
    {
        if (save_objs.ContainsKey(key))
        {
            save_objs.Remove(key);
        }
    }
    public void RemoveObject(int key, SkillObj obj)
    {
        if (save_objs.ContainsKey(key))
        {
            save_objs[key].Remove(obj);
        }
    }
    public List<SkillObj> GetObj(int key)
    {
        if (save_objs.ContainsKey(key))
        {
            return save_objs[key];
        }
        else
        {
            return null;
        }
    }
    public SkillObj GetObjectByName(int key)
    {
        if (save_objs.ContainsKey(key))
        {
            return save_objs[key][0];
        }
        return null;
    }
    #endregion

    #region 技能模块
    /// <summary>
    /// 对象池
    /// </summary>
    public Dictionary<int, List<Skill_Base>> RunDatas = new Dictionary<int, List<Skill_Base>>();
    /// <summary>
    /// 每个模块的模板
    /// </summary>
    private Dictionary<string, Skill_Base> ItemTypeDatas = new Dictionary<string, Skill_Base>();
    /// <summary>
    /// 技能表里面所有数据
    /// </summary>
    public Dictionary<int, SkillData> skills = new Dictionary<int, SkillData>();
    private void InitSkill()
    {
        //Log.Info("InitSkill");
        //========读取xml==========
        #region 技能表
        XmlDocument xml_skill = GetXmlData(XmlType.Skill);
        XmlNodeList nodes = xml_skill.SelectNodes(@"/skills/skill");
        for (int i = 0; i < nodes.Count; i++)
        {
            SkillData skill_data = new SkillData();
            skill_data.Datas = new Dictionary<int, Skill_Base>();
            skill_data.DatasList = new List<Skill_Base>();
            #region 技能数据
            skill_data.skill_property = new Skill();
            skill_data.skill_property.SkillID = Skill_Manager.GetXmlAttrInt(nodes[i], "id");
            skill_data.ID = skill_data.skill_property.SkillID;

            skill_data.skill_property.rank = Skill_Manager.GetXmlAttrInt(nodes[i], "rank");
            skill_data.skill_property.Name = nodes[i].Attributes["name"].InnerText;
            skill_data.skill_property.CD1 = Skill_Manager.GetXmlAttrFloat(nodes[i], "cd");

            skill_data.skill_property.Elements = Skill_Manager.GetXmlAttrInts(nodes[i], "element");


            skill_data.skill_property.cd_auto = true;
            if (nodes[i].Attributes["cd_auto"] != null && nodes[i].Attributes["cd_auto"].Value == "false")
            {
                skill_data.skill_property.cd_auto = false;
            }
            skill_data.skill_property.AiUseScript = false;
            if (nodes[i].Attributes["ai_use_script"] != null && nodes[i].Attributes["ai_use_script"].Value == "true")
            {
                skill_data.skill_property.AiUseScript = true;
            }

            //skill_data.skill_property.cd_auto=nodes[i].Attributes["name"].InnerText==
            skill_data.skill_property.up_ids = Skill_Manager.GetXmlAttrInts(nodes[i], "up_ids");
            if (nodes[i].Attributes["trigger"] != null)
            {
                switch (nodes[i].Attributes["trigger"].Value)
                {
                    case "click": skill_data.skill_property.TriggerType = SkillRunType.click; break;
                    case "down": skill_data.skill_property.TriggerType = SkillRunType.down; break;
                    default: skill_data.skill_property.TriggerType = SkillRunType.click; break;
                }
            }
            else
            {
                skill_data.skill_property.TriggerType = SkillRunType.click;
            }

            if (nodes[i].Attributes["range"] != null)
            {
                skill_data.skill_property.Range = Skill_Manager.GetXmlAttrFloat(nodes[i], "range");
            }
            if (nodes[i].Attributes["cd2"] != null)
            {
                skill_data.skill_property.CD2 = Skill_Manager.GetXmlAttrFloat(nodes[i], "cd2");
            }
            else
            {
                skill_data.skill_property.CD2 = -100;
            }
            #endregion
            #region 模块
            foreach (XmlNode item in nodes[i].ChildNodes)
            {
                Skill_Base sb = null;
                switch (item.Name)
                {
                    //====连击=====
                    case "combo"://连击
                        sb = new Skill_Combo();// ScriptableObject.CreateInstance<Skill_Combo>();
                        break;
                    case "combo_input"://连击输入
                        sb = new Skill_Combo_Input();//ScriptableObject.CreateInstance<Skill_Combo_Input>();
                        break;
                    case "combo_item_end"://连击结束
                        sb = new Skill_Combo_Item_End();//ScriptableObject.CreateInstance<Skill_Combo_Item_End>();
                        break;

                }
                if (sb != null)
                {
                    if (!ItemTypeDatas.ContainsKey(item.Name))
                    {
                        ItemTypeDatas.Add(item.Name, sb);
                    }
                    sb.class_type_str = item.Name;
                    sb.class_type = sb.class_type_str.GetHashCode();
                    try
                    {
                        sb.Init(skill_data.skill_property, item);
                    }
                    catch
                    {
                        Log.Error("技能：" + skill_data.ID + ",模块：" + sb.ID + " 填写数据错误");
                    }
                    if (skill_data.Datas.ContainsKey(sb.ID))
                    {
                        Log.Error("技能id：" + skill_data.ID + "============================已经存在id：" + sb.ID);
                        return;
                    }
                    skill_data.Datas.Add(sb.ID, sb);
                    skill_data.DatasList.Add(sb);
                }
            }
            #endregion
            if (skills.ContainsKey(skill_data.ID)) { Log.Error("技能表出现重复技能ID:" + skill_data.ID); return; }
            skills.Add(skill_data.ID, skill_data);
        }
        #endregion
    }

    /// <summary>
    /// 初始化技能模块对象池
    /// </summary>
    private void CreateModulePool()
    {
        RunDatas.Clear();
        XmlDocument xml_skill_init = GetXmlData(XmlType.InitClass);
        XmlNodeList nodes = xml_skill_init.SelectNodes(@"/skills/item");
        string type = "";
        int num = 0;
        Skill_Base sb = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            type = nodes[i].Attributes["type"].InnerText;
            num = Skill_Manager.GetXmlAttrInt(nodes[i], "num");
            if (ItemTypeDatas.ContainsKey(type))
            {
                List<Skill_Base> new_data = new List<Skill_Base>();
                for (int j = 0; j < num; j++)
                {
                    sb = (ItemTypeDatas[type] as IDeepCopy).DeepCopy(false);
                    sb.State = SkillState.RealOver;
                    new_data.Add(sb);

                }
                int type_key = type.GetHashCode();
                RunDatas.Add(type_key, new_data);
            }
        }
    }

    /// <summary>
    /// 拷贝数据
    /// </summary>
    /// <param name="skillid"></param>
    /// <param name="data"></param>
    public void SkillCopyData(Skill data)
    {
        if (skills.ContainsKey(data.SkillID))
        {
            skills[data.SkillID].skill_property.CopyData(data);
        }
    }

    public List<Skill_Base> GetSkillDataList(int id)
    {
        if (!skills.ContainsKey(id))
        {
            Log.Error("不包含技能：" + id);
            return null;
        }
        //Log.Info("获取技能：" + id);
        return skills[id].DatasList;
    }

    #region 获取一个模块
    public Skill_Base GetObj(int obj_id, Skill_Base sb_now)
    {
        //#if UNITY_EDITOR
        int skill_id = sb_now.OrigSkill.SkillID;
        if (!skills.ContainsKey(skill_id))
        {
            Log.Info("不包含技能iD：" + skill_id);
            return null;
        }

        if (!skills[skill_id].Datas.ContainsKey(obj_id))
        {
            Log.Info("技能iD：" + skill_id + "---不包含模块：" + obj_id);
            return null;
        }
        //#endif
        Skill_Base sb = null, target = null;
        //目标类
        target = skills[skill_id].Datas[obj_id];
        int classtype = target.class_type;

        if (RunDatas.ContainsKey(classtype))
        {
            List<Skill_Base> skill_base_datas = RunDatas[classtype];
            if (skill_base_datas.Count > 0)
            {
                sb = skill_base_datas[0];
                skill_base_datas.RemoveAt(0);
                target.Copy(sb);
                sb.OrigSkill = sb_now.OrigSkill;
                sb.OrigObj = sb_now.OrigObj;
                return sb;
            }
        }
        else
        {
            List<Skill_Base> new_data = new List<Skill_Base>();
            RunDatas.Add(classtype, new_data);
        }
        Log.Info("===== new obj!--:" + target.class_type_str);
        sb = (target as IDeepCopy).DeepCopy(true);
        sb.OrigSkill = sb_now.OrigSkill;
        sb.OrigObj = sb_now.OrigObj;
        return sb;
    }
    public Skill_Base GetObj(int obj_id, Skill OrigSkill)
    {
        //#if UNITY_EDITOR
        int skill_id = OrigSkill.SkillID;
        if (!skills.ContainsKey(skill_id))
        {
            Log.Info("不包含技能iD：" + skill_id);
            return null;
        }

        if (!skills[skill_id].Datas.ContainsKey(obj_id))
        {
            Log.Info("技能iD：" + skill_id + "---不包含模块：" + obj_id);
            return null;
        }
        //#endif
        Skill_Base sb = null, target = null;
        //目标类
        target = skills[skill_id].Datas[obj_id];
        int classtype = target.class_type;

        if (RunDatas.ContainsKey(classtype))
        {
            List<Skill_Base> skill_base_datas = RunDatas[classtype];
            if (skill_base_datas.Count > 0)
            {
                sb = skill_base_datas[0];
                skill_base_datas.RemoveAt(0);
                target.Copy(sb);
                sb.OrigSkill = OrigSkill;
                sb.OrigObj = OrigSkill.owner;
                return sb;
            }
        }
        else
        {
            List<Skill_Base> new_data = new List<Skill_Base>();
            RunDatas.Add(classtype, new_data);
        }
        Log.Info("===== new obj!--:" + target.class_type_str);
        sb = (target as IDeepCopy).DeepCopy(true);
        sb.OrigSkill = OrigSkill;
        sb.OrigObj = OrigSkill.owner;
        return sb;
    }
    #endregion

    public void BackObj(Skill_Base sb)
    {
        try
        {
            RunDatas[sb.class_type].Add(sb);
        }
        catch
        {
            Log.Info("1221111111111111111");
        }
    }
    /// <summary>
    /// 运行一个模块
    /// </summary>
    /// <param name="id"></param>
    public Skill_Base RunModule(int id, SkillObj Data, Skill skill_target, Skill OrigSkill, bool AddToList, bool IsHit = false)
    {
        //if (id > 0)
        //{
        //    if (OrigSkill == null)
        //    {
        //        Log.Info("===========ppppp1====>:" + id);
        //        return null;
        //    }
        //    //ProfilerSample.BeginSample("RunModule:" + id);
        //    Skill_Base sb = GetObj(id, OrigSkill);
        //    if (sb == null)
        //    {
        //        //Log.Error("获取到模块为空：" + skill_target.OrigSkill.SkillID + "-" + id);
        //        return null;
        //    }
        //    if (!sb.SkillTags.Contains(Skill_Manager.Use_Dead_Not_Kill_key))
        //    {
        //        if (CheckDead(skill_target.owner.gameObject))
        //        {//使用技能的人死了，且该技能不能在死后继续运行，就不运行
        //            sb.State = SkillState.RealOver;
        //            return null;
        //        }
        //    }
        //    if (!sb.SkillTags.Contains(Skill_Manager.Self_Dead_Not_Kill_key))
        //    {
        //        if (Data != null && CheckDead(Data.gameObject))
        //        {//被技能作用的人死了，且该技能不能在死后继续运行，就不运行
        //            sb.State = SkillState.RealOver;
        //            return null;
        //        }
        //    }

        //    sb.IsHit = IsHit;
        //    sb.AddToList = AddToList;
        //    if (sb.OrigSkill == null)
        //    {
        //        Log.Info("ppppppppp");
        //        Skill_Base sb2 = GetObj(id, OrigSkill);
        //    }
        //    sb.Run(Data, skill_target);
        //    return sb;
        //}
        return null;
    }
    /// <summary>
    /// 运行一个模块
    /// </summary>
    /// <param name="id"></param>
    public Skill_Base RunModule(int id, SkillObj Data, Skill skill_target, Skill_Base sb_now, bool AddToList, bool IsHit = false)
    {
        //if (id > 0)
        //{
        //    //ProfilerSample.BeginSample("RunModule:" + id);
        //    if (sb_now.OrigSkill == null)
        //    {
        //        Log.Info("===========ppppp2====>:" + id);
        //        return null;
        //    }
        //    Skill_Base sb = GetObj(id, sb_now);
        //    if (sb == null)
        //    {
        //        //Log.Error("获取到模块为空：" + skill_target.OrigSkill.SkillID + "-" + id);
        //        return null;
        //    }
        //    if (!sb.SkillTags.Contains(Skill_Manager.Use_Dead_Not_Kill_key))
        //    {
        //        if (CheckDead(sb_now.OrigObj.gameObject))
        //        {//使用技能的人死了，且该技能不能在死后继续运行，就不运行
        //            sb.State = SkillState.RealOver;
        //            return null;
        //        }
        //    }
        //    if (!sb.SkillTags.Contains(Skill_Manager.Self_Dead_Not_Kill_key))
        //    {
        //        if (Data != null && CheckDead(Data.gameObject))
        //        {//被技能作用的人死了，且该技能不能在死后继续运行，就不运行
        //            sb.State = SkillState.RealOver;
        //            return null;
        //        }
        //    }

        //    sb.IsHit = IsHit;
        //    sb.AddToList = AddToList;
        //    if (sb.OrigSkill == null)
        //    {
        //        Log.Info("ppppppppp");
        //        Skill_Base sb2 = GetObj(id, sb_now);
        //    }
        //    sb.Run(Data, skill_target);
        //    return sb;
        //}
        return null;
    }

    public Skill_Base RunModule(int id, Vector3 pos, Skill skill_target, Skill_Base sb_now, bool AddToList)
    {
        //if (id > 0)
        //{
        //    if (sb_now.OrigSkill == null)
        //    {
        //        Log.Info("===========ppppp3====>:" + id);
        //        return null;
        //    }
        //    Skill_Base sb = GetObj(id, sb_now);
        //    if (sb == null)
        //    {
        //        Log.Error("获取到模块为空：" + sb_now.OrigSkill.SkillID + "-" + id);
        //        return null;
        //    }
        //    if (!sb.SkillTags.Contains(Skill_Manager.Use_Dead_Not_Kill_key))
        //    {
        //        if (CheckDead(sb_now.OrigObj.gameObject))
        //        {//使用技能的人死了，且该技能不能在死后继续运行，就不运行
        //            sb.State = SkillState.RealOver;
        //            return null;
        //        }
        //    }

        //    if (sb is ISetPosition)
        //    {
        //        (sb as ISetPosition).SetPosition(pos);
        //    }
        //    sb.IsHit = false;
        //    sb.AddToList = AddToList;
        //    sb.Run(null, skill_target);
        //    return sb;
        //}
        return null;
    }

    #endregion

    #region xml 读取
    private static int XmlNullValu = 0;
    public static int GetXmlAttrInt(XmlNode node, string key)
    {
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                return int.Parse(str);
            }
        }
        return XmlNullValu;
    }
    public static float GetXmlAttrFloat(XmlNode node, string key)
    {
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                return float.Parse(str);
            }
        }
        return XmlNullValu;
    }
    public static Vector3 GetXmlAttrVector(XmlNode node, string key)
    {
        //Log.Info(key);
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                return GetVector(str);
            }
        }
        return Vector3.zero;
    }
    public static List<Vector3> GetXmlAttrVectors(XmlNode node, string key)
    {
        List<Vector3> result = new List<Vector3>();
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] datas = str.Split('|');

                foreach (string pp in datas)
                {
                    string[] strs = pp.Split(',');

                    result.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
                }
            }
        }
        return result;
    }
    public static List<int> GetXmlAttrInts(XmlNode node, string key)
    {

        if (node.Attributes[key] == null) { return new List<int>(); }

        //Log.Info(key);
        int num = 0;
        List<int> result = new List<int>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                if (int.TryParse(item, out num))
                {
                    result.Add(num);
                }
            }
        }
        return result;
    }
    public static List<float> GetXmlAttrFloats(XmlNode node, string key)
    {

        if (node.Attributes[key] == null) { return new List<float>(); }

        //Log.Info(key);
        float num = 0;
        List<float> result = new List<float>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                if (float.TryParse(item, out num))
                {
                    result.Add(num);
                }
            }
        }
        return result;
    }
    public static List<string> GetXmlAttrStrings(XmlNode node, string key)
    {
        if (node.Attributes[key] != null)
        {
            List<string> result = new List<string>();
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string item in strs)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        else
        {
            return null;
        }
    }
    public static List<List<int>> GetXmlAttrIntss(XmlNode node, string key)
    {
        //Log.Info(key);
        int num = 0;
        List<List<int>> result = new List<List<int>>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                List<int> r = new List<int>();
                string[] items = item.Split('|');
                foreach (string s in items)
                {
                    if (int.TryParse(s, out num))
                    {
                        r.Add(num);
                    }
                }
                result.Add(r);
            }
        }
        return result;
    }
    public static int GetXmlInt(XmlNode node)
    {
        if (node != null)
        {
            if (!string.IsNullOrEmpty(node.InnerText))
            {
                return int.Parse(node.InnerText);
            }
        }
        return XmlNullValu;
    }
    public static Vector3 GetVector(string str)
    {
        string[] strs = str.Split(',');
        if (strs.Length == 1)
        {
            return new Vector3(float.Parse(strs[0]), float.Parse(strs[0]), float.Parse(strs[0]));
        }
        else
        {
            return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        }
    }

    public static string ToString(Vector3 v)
    {
        if (v != Vector3.zero)
        {
            return v.x + "," + v.y + "," + v.z;
        }
        return "";
    }
    public static string ToString(bool b)
    {
        return b ? "yes" : "";
    }
    public static string ToString(List<int> datas)
    {
        if (datas == null || datas.Count <= 0) { return ""; }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        for (int i = 0; i < datas.Count; i++)
        {
            str.Append(datas[i] + ",");
        }
        return str.ToString().TrimEnd(',');
    }
    public static string ToString(List<string> datas)
    {
        if (datas == null || datas.Count <= 0) { return ""; }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        for (int i = 0; i < datas.Count; i++)
        {
            str.Append(datas[i] + ",");
        }
        return str.ToString().TrimEnd(',');
    }
    #endregion
}
public class SkillData
{
    public int ID;
    public Skill skill_property;
    public Dictionary<int, Skill_Base> Datas;
    public List<Skill_Base> DatasList = new List<Skill_Base>();
}
public class XmlData
{
    /// <summary>
    /// 路径
    /// </summary>
    public string Path;
    /// <summary>
    /// 数据
    /// </summary>
    public XmlDocument Data;

    public XmlData(string path)
    {
        Path = path;
    }
}

public enum XmlType
{
    /// <summary>
    /// 资源模型
    /// </summary>
    Model,
    /// <summary>
    /// 声音特效
    /// </summary>
    Audio,
    /// <summary>
    /// 技能
    /// </summary>
    Skill,
    /// <summary>
    /// 技能拥有的资源
    /// </summary>
    SkillResource,
    /// <summary>
    /// 伤害
    /// </summary>
    Damage,
    /// <summary>
    /// 对象初始化数量
    /// </summary>
    InitClass
}

public class ClassPool<T> where T : new()
{
    public List<T> pools = new List<T>();
    public T Get()
    {
        if (pools.Count > 0)
        {
            T item = pools[0];
            pools.RemoveAt(0);
            return item;
        }
        else
        {
            //#if UNITY_EDITOR
            //            Log.Info("===pool new t====");
            //#endif
            return new T();
        }
    }
    public void Back(T item)
    {
        pools.Add(item);
    }
    public void InitPool(int num)
    {
        pools.Clear();
        for (int i = 0; i < num; i++)
        {
            pools.Add(new T());
        }
    }
    public void Clear()
    {
        pools.Clear();
    }
}
[System.Serializable]
public class TextSkillObjItem
{
    public string key;

    public List<GameObject> datas;

    public TextSkillObjItem(string _key, List<GameObject> _datas)
    {
        key = _key;
        datas = _datas;
    }
}
