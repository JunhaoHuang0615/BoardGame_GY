using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAttackActiveComp : MonoBehaviour
{
    public bool needSaveOriginalPosition; //挂的时候就确认好了
    [HideInInspector]
    public Vector3 oriPosition;
    public float distanceToTarget = 1f;
    [HideInInspector]
    public Vector3 targetPosition;
    public AnimationCurve attack_attack_move_correctionCurve;

    public float moveSpeed; //反应的是当前物体的移动速度，受曲线控制，不是人为位置
    [SerializeField]
    public MyAttackTypeAnimationDictionary myAttacktypeAnimation; //Editor内部编辑使用
    public Dictionary<AttackType, MyAnimationData> attack_animationData; //运行时使用

    private void Start()
    {
        attack_animationData = myAttacktypeAnimation.ToDictionary();
    }
}

[Serializable]
public class MyAttackTypeAnimationDictionary
{
    [SerializeField]
    public AttackTypeAnimation[] attackTypeAnimations;

    public Dictionary<AttackType, MyAnimationData> ToDictionary()
    {
        Dictionary<AttackType, MyAnimationData> attackType_AniationDict = new Dictionary<AttackType, MyAnimationData>();
        foreach (var item in attackTypeAnimations)
        {
            attackType_AniationDict.Add(item.attackType, item.AnimationData);
        }
        return attackType_AniationDict;
    }
}
[Serializable]
public class AttackTypeAnimation
{
    [SerializeField]
    public AttackType attackType;

    [SerializeField]
    public MyAnimationData AnimationData;
}
[Serializable]
public class MyAnimationData
{
    [SerializeField]
    public AnimationCurve animationCurve;
    [SerializeField]
    public float curve_max_height; //曲线的放大倍数，曲线的最高值
    [SerializeField]
    public AnimationCurve speedCurve;
    public float speed_curve_MaxSpeed; //对应的速度是速度曲线中，纵轴为1的值
    public AttackPointType targetAttackPointType; //这个动画对应的目标位置
    public Vector3 attackPointOffset; //注意区分左右，向右移动的时候，offset应该是向左调。向左移动的时候，offset向右边调整

}