using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public AttackPathProfile pathData; //进攻路线
    public Animator attackUnitAnimator; //攻击方的动画组件
    
    public IEnumerator AttackMovement()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(pathData.attackPath.path.length - distanceTravelled)> 0.5f)
        {
            transform.position = pathData.attackPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (pathData.attackPath.path.length / (pathData.attackEndTime - pathData.attackStartTime) ) * Time.deltaTime;
            yield return null;
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
