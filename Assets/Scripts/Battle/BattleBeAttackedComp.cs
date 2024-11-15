using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBeAttackedComp : MonoBehaviour
{
    public AttackPointType attackPointType;
    //public Vector3 attackPointPosition;
}

public enum AttackPointType
{
    HEAD,
    BODY,
    FOOT,
}
