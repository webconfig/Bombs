using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillObj : MonoBehaviour
{
    //public Character character;
    //public Skill_Pool sp;
    //public Character_BUFF buff;
    //public Character_Force_New force;
    //public CameraFollowRole camera_follow;
    //public EffectControl effect_control;
    //public Character_Spawn_Control spawn_control;
    //public BoxCollider colloider_box;
    //public CapsuleCollider colloider_capsule;
    //public SphereCollider colloider_Sphere;
    //public Collider _Collider;
    //public HumanControl human;
    //public Skill_GameObject_Control gameobject_control;
    //public AI_Use_Skill ai_use_skill;
    //private int id;
    //public int type;
    //public bool colloider_open=true;
    //public void Init()
    //{  
    //    character = GetComponent<Character>();
    //    sp = GetComponent<Skill_Pool>();
    //    buff = GetComponent<Character_BUFF>();
    //    force = GetComponent<Character_Force_New>();
    //    camera_follow = GetComponent<CameraFollowRole>();
    //    effect_control = GetComponent<EffectControl>();
    //    spawn_control = GetComponent<Character_Spawn_Control>();
    //    colloider_capsule = GetComponent<CapsuleCollider>();
    //    colloider_Sphere = GetComponent<SphereCollider>();
    //    colloider_box = GetComponent<BoxCollider>();
    //    gameobject_control = GetComponent<Skill_GameObject_Control>();
    //    human = GetComponent<HumanControl>();
    //    _Collider = GetComponent<Collider>();
    //    ai_use_skill = GetComponent<AI_Use_Skill>();
    //    if (_Collider!=null)
    //    {
    //        colloider_open = _Collider.enabled;
    //    }
    //    if (character != null && (_Collider != null))
    //    {
    //        id = GetInstanceID();
    //        Skill_Manager.Instance.AddObj(id, this);
    //    }
    //}
    //void OnEnable()
    //{
    //    if (character != null && (_Collider != null))
    //    {
    //        id = GetInstanceID();
    //        Skill_Manager.Instance.AddObj(id, this);
    //    }
    //}

    //void OnDisable()
    //{
    //    if (character != null && (_Collider != null))
    //    {
    //        Skill_Manager.Instance.RemoveObj(id);
    //        if (_Collider != null)
    //        {
    //            _Collider.enabled = colloider_open;
    //        }
    //    }
    //}

    //public int only_use_skill_id = 0;
    //private bool _objinit = false;
    //public void ObjInit(Dictionary<string, string> datas)
    //{
    //    if (_objinit) { return; }
    //    _objinit = true;
    //    Init();
    //}
}
