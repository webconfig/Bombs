using UnityEngine;

/// <summary>
/// 深拷贝接口
/// </summary>
interface IDeepCopy
{
    Skill_Base DeepCopy(bool init);
}
interface ISetPosition
{
    void SetPosition(Vector3 pos);
}