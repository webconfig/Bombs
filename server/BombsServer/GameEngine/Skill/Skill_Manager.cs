//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Xml;

//namespace GameEngine
//{
//    public class Skill_Manager
//    {
//        public bool CanUseSkill = true;

//        public int skill_run_index = 1;
//        public int GetSkillIndex()
//        {
//            skill_run_index++;
//            return skill_run_index;
//        }

//        #region 技能模块
//        /// <summary>
//        /// 对象池
//        /// </summary>
//        public Dictionary<string, List<Skill_Base>> RunDatas = new Dictionary<string, List<Skill_Base>>();
//        /// <summary>
//        /// 每个模块的模板
//        /// </summary>
//        private Dictionary<string, Skill_Base> ItemTypeDatas = new Dictionary<string, Skill_Base>();
//        /// <summary>
//        /// 技能表里面所有数据
//        /// </summary>
//        public Dictionary<int, SkillData> skills = new Dictionary<int, SkillData>();
//        public void InitSkill()
//        {
//            //Debug.Log("InitSkill");
//            Log.Info("开始读取技能表");
//            //========读取xml==========
//            #region 技能表
//            XmlDocument xml_skill = new XmlDocument();
//            xml_skill.Load("config/skill.xml");
//            XmlNodeList nodes = xml_skill.SelectNodes(@"/skills/skill");
//            for (int i = 0; i < nodes.Count; i++)
//            {
//                SkillData skill_data = new SkillData();
//                skill_data.Datas = new Dictionary<int, Skill_Base>();
//                skill_data.DatasList = new List<Skill_Base>();
//                #region 技能数据
//                skill_data.skill_property = new Skill();
//                skill_data.skill_property.SkillID = Skill_Manager.GetXmlAttrInt(nodes[i], "id");
//                skill_data.ID = skill_data.skill_property.SkillID;

//                skill_data.skill_property.rank = Skill_Manager.GetXmlAttrInt(nodes[i], "rank");
//                skill_data.skill_property.Name = nodes[i].Attributes["name"].InnerText;
//                skill_data.skill_property.CD = Skill_Manager.GetXmlAttrFloat(nodes[i], "cd");
//                skill_data.skill_property.cd_auto = true;
//                if (nodes[i].Attributes["cd_auto"] != null && nodes[i].Attributes["cd_auto"].Value == "false")
//                {
//                    skill_data.skill_property.cd_auto = false;
//                }
//                //skill_data.skill_property.cd_auto=nodes[i].Attributes["name"].InnerText==
//                skill_data.skill_property.up_ids = Skill_Manager.GetXmlAttrInts(nodes[i], "up_ids");
//                if (nodes[i].Attributes["trigger"] != null)
//                {
//                    switch (nodes[i].Attributes["trigger"].Value)
//                    {
//                        case "click": skill_data.skill_property.TriggerType = SkillRunType.click; break;
//                        case "down": skill_data.skill_property.TriggerType = SkillRunType.down; break;
//                        default: skill_data.skill_property.TriggerType = SkillRunType.click; break;
//                    }
//                }
//                else
//                {
//                    skill_data.skill_property.TriggerType = SkillRunType.click;
//                }

//                if (nodes[i].Attributes["range"] != null)
//                {
//                    skill_data.skill_property.Range = Skill_Manager.GetXmlAttrFloat(nodes[i], "range");
//                }
//                #endregion
//                #region 模块
//                foreach (XmlNode item in nodes[i].ChildNodes)
//                {
//                    Skill_Base sb = null;
//                    switch (item.Name)
//                    {
//                        case "anim_clip":
//                            sb = new Skill_Anim_Clip();
//                            break;
//                    }
//                    if (sb != null)
//                    {
//                        if (!ItemTypeDatas.ContainsKey(item.Name))
//                        {
//                            ItemTypeDatas.Add(item.Name, sb);
//                        }
//                        sb.class_type = item.Name;
//                        try
//                        {
//                            sb.Init(skill_data.skill_property, item);
//                        }
//                        catch
//                        {
//                           Log.Error("技能：" + skill_data.ID + ",模块：" + sb.ID + " 填写数据错误");
//                        }
//                        if (skill_data.Datas.ContainsKey(sb.ID))
//                        {
//                            Log.Error("技能id：" + skill_data.ID + "============================已经存在id：" + sb.ID);
//                            return;
//                        }
//                        skill_data.Datas.Add(sb.ID, sb);
//                        skill_data.DatasList.Add(sb);
//                    }
//                }
//                #endregion
//                if (skills.ContainsKey(skill_data.ID)) { Log.Error("技能表出现重复技能ID:" + skill_data.ID); return; }
//                skills.Add(skill_data.ID, skill_data);
//            }
//            #endregion
//            Log.Info("完成读取技能表");
//        }
//        /// <summary>
//        /// 拷贝数据
//        /// </summary>
//        /// <param name="skillid"></param>
//        /// <param name="data"></param>
//        public void SkillCopyData(Skill data)
//        {
//            if (skills.ContainsKey(data.SkillID))
//            {
//                skills[data.SkillID].skill_property.CopyData(data);
//            }
//        }
//        public List<Skill_Base> GetSkillDataList(int id)
//        {
//            if (!skills.ContainsKey(id))
//            {
//                Log.Error("不包含技能：" + id);
//                return null;
//            }
//            return skills[id].DatasList;
//        }
//        /// <summary>
//        /// 清空
//        /// </summary>
//        public void ClearSkillModulePool()
//        {
//            RunDatas.Clear();
//        }

//        #region 获取一个模块
//        public Skill_Base GetObj(int obj_id, Skill_Base sb_now)
//        {
//            //#if UNITY_EDITOR
//            int skill_id = sb_now.OrigSkill.SkillID;
//            if (!skills.ContainsKey(skill_id))
//            {
//                Log.Error("不包含技能iD：" + skill_id);
//                return null;
//            }

//            if (!skills[skill_id].Datas.ContainsKey(obj_id))
//            {
//                Log.Error("技能iD：" + skill_id + "---不包含模块：" + obj_id);
//                return null;
//            }
//            //#endif
//            Skill_Base sb = null, target = null;
//            //目标类
//            target = skills[skill_id].Datas[obj_id];
//            string classtype = target.class_type;

//            if (RunDatas.ContainsKey(classtype))
//            {
//                List<Skill_Base> skill_base_datas = RunDatas[classtype];
//                if (skill_base_datas.Count > 0)
//                {
//                    sb = skill_base_datas[0];
//                    skill_base_datas.RemoveAt(0);
//                    target.Copy(sb);
//                    sb.OrigSkill = sb_now.OrigSkill;
//                    sb.OrigObj = sb_now.OrigObj;
//                    return sb;
//                }
//            }
//            else
//            {
//                List<Skill_Base> new_data = new List<Skill_Base>();
//                RunDatas.Add(classtype, new_data);
//            }
//            Log.Error("===== new obj!--:" + target.class_type);
//            sb = (target as IDeepCopy).DeepCopy(true);
//            sb.OrigSkill = sb_now.OrigSkill;
//            sb.OrigObj = sb_now.OrigObj;
//            return sb;
//        }
//        public Skill_Base GetObj(int obj_id, Skill OrigSkill)
//        {
//            //#if UNITY_EDITOR
//            int skill_id = OrigSkill.SkillID;
//            if (!skills.ContainsKey(skill_id))
//            {
//                Log.Error("不包含技能iD：" + skill_id);
//                return null;
//            }

//            if (!skills[skill_id].Datas.ContainsKey(obj_id))
//            {
//                Log.Error("技能iD：" + skill_id + "---不包含模块：" + obj_id);
//                return null;
//            }
//            //#endif
//            Skill_Base sb = null, target = null;
//            //目标类
//            target = skills[skill_id].Datas[obj_id];
//            string classtype = target.class_type;

//            if (RunDatas.ContainsKey(classtype))
//            {
//                List<Skill_Base> skill_base_datas = RunDatas[classtype];
//                if (skill_base_datas.Count > 0)
//                {
//                    sb = skill_base_datas[0];
//                    skill_base_datas.RemoveAt(0);
//                    target.Copy(sb);
//                    sb.OrigSkill = OrigSkill;
//                    sb.OrigObj = OrigSkill.owner;
//                    return sb;
//                }
//            }
//            else
//            {
//                List<Skill_Base> new_data = new List<Skill_Base>();
//                RunDatas.Add(classtype, new_data);
//            }
//            Log.Info("===== new obj!--:" + target.class_type);
//            sb = (target as IDeepCopy).DeepCopy(true);
//            sb.OrigSkill = OrigSkill;
//            sb.OrigObj = OrigSkill.owner;
//            return sb;
//        }
//        #endregion
//        #endregion

//        #region xml 读取
//        private static int XmlNullValu = 0;
//        public static int GetXmlAttrInt(XmlNode node, string key)
//        {
//            if (node.Attributes[key] != null)
//            {
//                string str = node.Attributes[key].InnerText;
//                if (!string.IsNullOrEmpty(str))
//                {
//                    return int.Parse(str);
//                }
//            }
//            return XmlNullValu;
//        }
//        public static float GetXmlAttrFloat(XmlNode node, string key)
//        {
//            if (node.Attributes[key] != null)
//            {
//                string str = node.Attributes[key].InnerText;
//                if (!string.IsNullOrEmpty(str))
//                {
//                    return float.Parse(str);
//                }
//            }
//            return XmlNullValu;
//        }
//        //public static Vector3 GetXmlAttrVector(XmlNode node, string key)
//        //{
//        //    //Debug.Log(key);
//        //    if (node.Attributes[key] != null)
//        //    {
//        //        string str = node.Attributes[key].InnerText;
//        //        if (!string.IsNullOrEmpty(str))
//        //        {
//        //            return GetVector(str);
//        //        }
//        //    }
//        //    return Vector3.zero;
//        //}
//        //public static Color GetXmlAttrColor(XmlNode node, string key)
//        //{
//        //    if (node.Attributes[key] != null)
//        //    {
//        //        string str = node.Attributes[key].InnerText;
//        //        if (!string.IsNullOrEmpty(str))
//        //        {
//        //            string[] strs = str.Split(',');
//        //            if (strs.Length == 4)
//        //            {
//        //                return new Color(float.Parse(strs[0]) / 255.00f, float.Parse(strs[1]) / 255.00f, float.Parse(strs[2]) / 255.00f, float.Parse(strs[3]) / 255.00f);
//        //            }
//        //            else
//        //            {
//        //                return new Color(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
//        //            }
//        //        }
//        //    }
//        //    return Color.white;
//        //}
//        //public static List<Vector3> GetXmlAttrVectors(XmlNode node, string key)
//        //{
//        //    List<Vector3> result = new List<Vector3>();
//        //    if (node.Attributes[key] != null)
//        //    {
//        //        string str = node.Attributes[key].InnerText;
//        //        if (!string.IsNullOrEmpty(str))
//        //        {
//        //            string[] datas = str.Split('|');

//        //            foreach (string pp in datas)
//        //            {
//        //                string[] strs = pp.Split(',');

//        //                result.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
//        //            }
//        //        }
//        //    }
//        //    return result;
//        //}
//        public static List<int> GetXmlAttrInts(XmlNode node, string key)
//        {

//            if (node.Attributes[key] == null) { return new List<int>(); }

//            //Debug.Log(key);
//            int num = 0;
//            List<int> result = new List<int>();
//            string str = node.Attributes[key].InnerText;
//            if (!string.IsNullOrEmpty(str))
//            {
//                string[] strs = str.Split(',');
//                foreach (string item in strs)
//                {
//                    if (int.TryParse(item, out num))
//                    {
//                        result.Add(num);
//                    }
//                }
//            }
//            return result;
//        }
//        public static List<float> GetXmlAttrFloats(XmlNode node, string key)
//        {

//            if (node.Attributes[key] == null) { return new List<float>(); }

//            //Debug.Log(key);
//            float num = 0;
//            List<float> result = new List<float>();
//            string str = node.Attributes[key].InnerText;
//            if (!string.IsNullOrEmpty(str))
//            {
//                string[] strs = str.Split(',');
//                foreach (string item in strs)
//                {
//                    if (float.TryParse(item, out num))
//                    {
//                        result.Add(num);
//                    }
//                }
//            }
//            return result;
//        }
//        public static List<string> GetXmlAttrStrings(XmlNode node, string key)
//        {
//            if (node.Attributes[key] != null)
//            {
//                List<string> result = new List<string>();
//                string str = node.Attributes[key].InnerText;
//                if (!string.IsNullOrEmpty(str))
//                {
//                    string[] strs = str.Split(',');
//                    foreach (string item in strs)
//                    {
//                        result.Add(item);
//                    }
//                }
//                return result;
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public static List<List<int>> GetXmlAttrIntss(XmlNode node, string key)
//        {
//            //Debug.Log(key);
//            int num = 0;
//            List<List<int>> result = new List<List<int>>();
//            string str = node.Attributes[key].InnerText;
//            if (!string.IsNullOrEmpty(str))
//            {
//                string[] strs = str.Split(',');
//                foreach (string item in strs)
//                {
//                    List<int> r = new List<int>();
//                    string[] items = item.Split('|');
//                    foreach (string s in items)
//                    {
//                        if (int.TryParse(s, out num))
//                        {
//                            r.Add(num);
//                        }
//                    }
//                    result.Add(r);
//                }
//            }
//            return result;
//        }
//        public static int GetXmlInt(XmlNode node)
//        {
//            if (node != null)
//            {
//                if (!string.IsNullOrEmpty(node.InnerText))
//                {
//                    return int.Parse(node.InnerText);
//                }
//            }
//            return XmlNullValu;
//        }
//        //public static Vector3 GetVector(string str)
//        //{
//        //    string[] strs = str.Split(',');
//        //    if (strs.Length == 1)
//        //    {
//        //        return new Vector3(float.Parse(strs[0]), float.Parse(strs[0]), float.Parse(strs[0]));
//        //    }
//        //    else
//        //    {
//        //        return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
//        //    }
//        //}

//        //public static string ToString(Vector3 v)
//        //{
//        //    if (v != Vector3.zero)
//        //    {
//        //        return v.x + "," + v.y + "," + v.z;
//        //    }
//        //    return "";
//        //}
//        public static string ToString(bool b)
//        {
//            return b ? "yes" : "";
//        }
//        public static string ToString(List<int> datas)
//        {
//            if (datas == null || datas.Count <= 0) { return ""; }
//            System.Text.StringBuilder str = new System.Text.StringBuilder();
//            for (int i = 0; i < datas.Count; i++)
//            {
//                str.Append(datas[i] + ",");
//            }
//            return str.ToString().TrimEnd(',');
//        }
//        public static string ToString(List<string> datas)
//        {
//            if (datas == null || datas.Count <= 0) { return ""; }
//            System.Text.StringBuilder str = new System.Text.StringBuilder();
//            for (int i = 0; i < datas.Count; i++)
//            {
//                str.Append(datas[i] + ",");
//            }
//            return str.ToString().TrimEnd(',');
//        }
//        #endregion
//    }
//    public class SkillData
//    {
//        public int ID;
//        public Skill skill_property;
//        public Dictionary<int, Skill_Base> Datas;
//        public List<Skill_Base> DatasList = new List<Skill_Base>();
//    }
//}
