using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public Dictionary<AttackType, AttackPathProfile> attackTypeDict;
    public AttackPathProfile pathData;
    public GameObject parent;
    public Animator attackUnitAnimator; //攻击方的动画组件
    private void Awake()
    {
        attackTypeDict = new Dictionary<AttackType, AttackPathProfile>();
        //自动根据子级的PathData来注册进入Dictionary
        AttackPathProfile[] groups = parent.GetComponentsInChildren<AttackPathProfile>();
        foreach(var data in groups)
        {
            if (!attackTypeDict.ContainsKey(data.attackType))
            {
                attackTypeDict.Add(data.attackType, data);
            }
        }

    }

    public IEnumerator AttackStart()
    {
        bool flag  = true;
        while (flag)
        {
            yield return attackTypeDict[attackUnit.attackType].AttackAni();
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
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(pathData.backPath.path.length - distanceTravelled) > 0.5f)
        {
            transform.position = pathData.backPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (pathData.backPath.path.length / (pathData.backEndTIme - pathData.backStartTime)) * Time.deltaTime;
            yield return null;
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
