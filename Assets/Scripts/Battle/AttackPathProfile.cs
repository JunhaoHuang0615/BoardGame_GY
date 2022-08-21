using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class AttackPathProfile : MonoBehaviour
{
    public PathCreator attackPath; //进攻路线
    public PathCreator backPath; //进攻后的退回路线
    public float attackStartTime; //进攻发生的时间
    public float attackEndTime; //进攻完成的时间
    public float backStartTime;
    public float backEndTIme;
}

