using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public Animator attackUnitAnimator; //攻击方的动画组件
    private float movespeed = 1f;
    public Vector3 activeUnit_ori_pos; //攻击方原始位置
    public bool isAttacking = false;

    private float animationTime = 0;

/*    public Dictionary<AttackType, AttackPathProfile> attackPathDict;
    public GameObject pathgroup;*/

    private void Awake()
    {
        //attackPathDict = new Dictionary<AttackType, AttackPathProfile>();
        //AttackPathProfile[] group = pathgroup.GetComponentsInChildren<AttackPathProfile>();
/*        foreach(var pathData in group)
        {
            if (!attackPathDict.ContainsKey(pathData.attackType))
            {
                attackPathDict.Add(pathData.attackType, pathData);
            }
        }*/

    }

    public void AttackEnd()
    {
        isAttacking = false;
    }

    public IEnumerator AttackMovement()
    {
        bool flag = true;
        while (flag)
        {
            yield return StartAttack();
            flag = false;
        }
    }

    public void PlayAttackerAnim(string animationName)
    {
        attackUnit.attackPrefab.GetComponentInChildren<Animator>().Play(animationName);
    }
    public void GetAttackerAnimationTime(string animationName)
    {
        AnimationClip clip = this.GetAnimationClipByName(attackUnit.attackPrefab.GetComponentInChildren<Animator>(), animationName);
        animationTime = clip.length;
        Debug.Log("动画名称："+clip.name + "动画时间："+clip.length);
    }

    public AnimationClip GetAnimationClipByName(Animator attackerAnimator ,string animationName) { 
        RuntimeAnimatorController controller = attackerAnimator.runtimeAnimatorController;
        foreach (AnimationClip eachClip in controller.animationClips)
        {
            if(eachClip.name == animationName)
            {
                return eachClip; 
            }
        
        }
        return null;
    }

    public IEnumerator StartAttack()
    {
        //确认攻击者移动的方向
        //获得攻击者的位置和目标者的位置
        GameObject activeUnit = attackUnit.attackPrefab;
        GameObject target = beattacked.attackPrefab;
        activeUnit_ori_pos = activeUnit.transform.position;
        Vector3 moveNormalizedAttackDir = (target.transform.position - activeUnit.transform.position).normalized;
        float distance = Vector3.Distance(activeUnit.transform.position, target.transform.position);
        float speed = distance / animationTime * Time.deltaTime;

        while (Vector3.Distance(activeUnit.transform.position,target.transform.position) > 1f)
        {
            activeUnit.transform.position += speed * moveNormalizedAttackDir;
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
        GameObject activeUnit = attackUnit.attackPrefab;

        Vector3 moveNormalizedAttackDir = (activeUnit_ori_pos - activeUnit.transform.position).normalized;
        while (Vector3.Distance(activeUnit.transform.position, activeUnit_ori_pos) > 1f)
        {
            activeUnit.transform.position += movespeed * moveNormalizedAttackDir;
            yield return null;
        }

        activeUnit.transform.position = activeUnit_ori_pos;

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
