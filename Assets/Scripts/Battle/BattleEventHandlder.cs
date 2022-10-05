using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public Animator attackUnitAnimator; //攻击方的动画组件
    public Dictionary<AttackType, AttackPathProfile> attackPathDict;
    public GameObject pathgroup;

    private void Awake()
    {
        attackPathDict = new Dictionary<AttackType, AttackPathProfile>();
        AttackPathProfile[] group = pathgroup.GetComponentsInChildren<AttackPathProfile>();
        foreach(var pathData in group)
        {
            if (!attackPathDict.ContainsKey(pathData.attackType))
            {
                attackPathDict.Add(pathData.attackType, pathData);
            }
        }

    }

    public IEnumerator AttackMovement()
    {
        bool flag = true;
        while (flag)
        {
            yield return attackPathDict[attackUnit.attackType].StartAttack();
            flag = false;
        }
    }

    public IEnumerator AttackResult()
    {
        attackUnitAnimator.speed = 0;
        yield return ResultHandle();
        attackUnitAnimator.speed = 1;
    }
    public IEnumerator AttackUnitMoveBack()
    {
        bool flag = true;
        while (flag)
        {
            yield return attackPathDict[attackUnit.attackType].ReturnAttack();
            flag = false;
        }
    }

    public IEnumerator ResultHandle()
    {
        int randomSeed = Random.Range(0,101);// 0可以取得到， 101取不到， 需要值是0~100的整型

        if(randomSeed >= 50)
        {
            //闪避
            beattacked.attackPrefab.GetComponentInChildren<Animator>().Play("miss");
            while (!beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("miss"))
            {
                yield return null;
            }
            //判断当前动画是否已经完成
            while (beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("miss"))
            {
                yield return null; //卡在动画播放
            }

        }
        else
        {
            //扣血
            yield return new WaitForSeconds(2);
        }

    }
}
