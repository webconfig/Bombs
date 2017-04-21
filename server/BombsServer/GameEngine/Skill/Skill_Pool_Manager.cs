using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class Skill_Pool_Manager : MonoBehaviour
{
    //public static Skill_Pool_Manager Instance;
    //public static void Init()
    //{
    //    if (Instance == null)
    //    {
    //        GameObject obj = new GameObject();
    //        obj.name = "Skill_Pool_Manager";
    //        obj.transform.parent = GameManager.Instance == null ? null : GameManager.Instance.gameObject.transform;
    //        obj.transform.position = new Vector3(-1000, -1000, -1000);
    //        Instance = obj.AddComponent<Skill_Pool_Manager>();
    //    }
    //}
    //public Dictionary<int, List<SkillObj>> datas_pool = new Dictionary<int, List<SkillObj>>();
    //public Dictionary<int, int> datas_all = new Dictionary<int, int>();
    //public List<ObjItem> AddItems = new List<ObjItem>();
    ////public bool ShowItemCount;
    ///// <summary>
    ///// 注册物体
    ///// </summary>
    ///// <param name="Objs"></param>
    //public void Add(List<ObjItem> Objs)
    //{
    //    if (Objs.Count == 0) { return; }
    //    AddItems.AddRange(Objs);

    //}

    //public SkillObj GetObject(int name, List<ObjItem> new_item, string alert_msg)
    //{
    //    if (datas_pool.ContainsKey(name))
    //    {
    //        List<SkillObj> objs = datas_pool[name];
    //        if (objs.Count > 0)
    //        {
    //            SkillObj _obj = objs[0];
    //            objs.RemoveAt(0);
    //            return _obj;
    //        }
    //        else
    //        {
    //            if (new_item != null)
    //            {
    //                for (int i = 0; i < new_item.Count; i++)
    //                {
    //                    AddCurrent(new_item[i]);
    //                }
    //                if (datas_pool.ContainsKey(name))
    //                {
    //                    objs = datas_pool[name];
    //                    SkillObj _obj = objs[0];
    //                    objs.RemoveAt(0);
    //                    return _obj;
    //                }
    //                else
    //                {
    //                    Debug.Log("skill pool  addd Error!");
    //                }
    //            }
    //            else
    //            {
    //                return null;
    //            }
    //        }
    //    }
    //    return null;
    //}

    ///// <summary>
    ///// 销毁一个物体
    ///// </summary>
    ///// <param name="name"></param>
    ///// <param name="obj"></param>
    //public void DisbaleObj(SkillObj obj)
    //{
    //    if (obj.transform.parent != transform)
    //    {
    //        obj.transform.SetParent(transform, false);
    //    }
    //    obj.gameObject.SetActive(false);
    //    //if (datas_pool.ContainsKey(obj.type))
    //    //{
    //    datas_pool[obj.type].Add(obj);
    //    //}
    //}

    ///// <summary>
    ///// 清理资源
    ///// </summary>
    //public void Clear()
    //{
    //    GameObject obj;
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        obj = transform.GetChild(i).gameObject;
    //        Destroy(obj);
    //    }
    //    datas_pool.Clear();
    //    datas_all.Clear();
    //}

    //void Update()
    //{
    //    //if (ShowItemCount) { ShowItemCount = false;  Debug.Log(transform.childCount); }
    //    if (AddItems.Count <= 0) { return; }
    //    //Debug.Log("skill pool  add");
    //    ObjItem obj_item = null;
    //    for (int i = 0; i < 5; i++)
    //    {
    //        if (AddItems.Count <= 0) { return; }
    //        obj_item = AddItems[0];
    //        AddItems.RemoveAt(0);
    //        AddCurrent(obj_item);
    //    }
    //}

    //private void AddCurrent(ObjItem obj_item)
    //{
    //    //Debug.Log(obj_item.obj_id);
    //    if (!RescourceManager.Instance.FindObj(obj_item.obj_id))
    //    {
    //        RescourceManager.Instance.ReadObj(obj_item.obj_id);//硬盘加载模型
    //    }

    //    //获取序号
    //    int index = 0;
    //    if (datas_all.ContainsKey(obj_item.name_key))
    //    {
    //        index = datas_all[obj_item.name_key];
    //        if (index > obj_item.Max)
    //        {
    //            return;
    //        }
    //        else if (index + obj_item.Num > obj_item.Max)
    //        {
    //            obj_item.Num = obj_item.Max - index;
    //        }
    //    }

    //    List<SkillObj> objs = new List<SkillObj>();
    //    for (int j = 0; j < obj_item.Num; j++)
    //    {
    //        GameObject source = RescourceManager.Instance.GetObj(obj_item.obj_id);
    //        if (source != null)
    //        {
    //            GameObject obj = UnityEngine.GameObject.Instantiate(source);
    //            obj.gameObject.name = obj_item.name + "_" + (j + index);

    //            if (obj_item.NeedInit)
    //            {//需要初始化
    //             //====添加脚本====
    //                if (obj_item.MonoScript != null)
    //                {
    //                    for (int p = 0; p < obj_item.MonoScript.Count; p++)
    //                    {
    //                        Component script = obj.AddComponent(obj_item.MonoScript[p]);
    //                        if (script is IObjInit)
    //                        {
    //                            (script as IObjInit).ObjInit(obj_item.datas);
    //                        }
    //                    }
    //                }
    //                //====初始化脚本
    //                Component[] comps = obj.GetComponents(typeof(Component));
    //                if (comps != null)
    //                {
    //                    for (int k = 0; k < comps.Length; k++)
    //                    {
    //                        if (comps[k] is IObjInit)
    //                        {
    //                            (comps[k] as IObjInit).ObjInit(obj_item.datas);
    //                        }
    //                    }
    //                }
    //            }
    //            //SaveObjectManager.Instance.SaveObject(obj.SkillObj.name, obj);
    //            obj.transform.SetParent(transform, false);
    //            SkillObj so = obj.GetComponent<SkillObj>();
    //            so.type = obj_item.name_key;
    //            obj.SetActive(false);
    //            objs.Add(so);
    //        }
    //    }

    //    if (datas_all.ContainsKey(obj_item.name_key))
    //    {
    //        datas_all[obj_item.name_key] = datas_all[obj_item.name_key] + objs.Count;
    //        datas_pool[obj_item.name_key].AddRange(objs);
    //    }
    //    else
    //    {
    //        datas_all.Add(obj_item.name_key, objs.Count);
    //        datas_pool.Add(obj_item.name_key, objs);
    //    }
    //}
}

