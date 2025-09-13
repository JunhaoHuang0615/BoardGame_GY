using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using TMPro;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public Animator attackUnitAnimator; //攻击方的动画组件
    private float movespeed = 1f;
    public Vector3 activeUnit_ori_pos; //攻击方原始位置

    public bool isAttacking = false; //攻击方在进行攻击


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

    public void AttackerEndAttck()
    {
        isAttacking = false;
    }

    public void GetAttackerAnimationTime(string animationName)
    {
        AnimationClip clip = this.GetAnimationClipByName(attackUnit.attackPrefab.GetComponentInChildren<Animator>(), animationName);
        animationTime = clip.length; //以秒为单位
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

    public IEnumerator StartAttack(string attackpoint_name = "")
    {
        //确认攻击者移动的方向
        //获得攻击者的位置和目标者的位置
        GameObject activeUnit = attackUnit.attackPrefab;
        GameObject target = beattacked.attackPrefab;
        Transform targetPosition = target.transform;
        activeUnit_ori_pos = activeUnit.transform.position;
        if(attackpoint_name != "")
        {
            foreach (var attackpoint in target.GetComponentsInChildren<AttackPoint>())
            {
                if(attackpoint.attack_point_name == attackpoint_name)
                {
                    targetPosition = attackpoint.GetComponent<Transform>();
                }
            }
        }

        Vector3 moveNormalizedAttackDir = (targetPosition.position - activeUnit.transform.position).normalized;

        float distance = Vector3.Distance(activeUnit.transform.position, targetPosition.position);

        float speed = distance / animationTime * Time.deltaTime;

        while (Vector3.Distance(activeUnit.transform.position, targetPosition.position) > 1f)
        {
            activeUnit.transform.position += speed * moveNormalizedAttackDir;
            yield return null;
        }
        activeUnit.transform.position = targetPosition.position; //为了保证目标位置绝对准确
    }

    public IEnumerator ChildCompMovement(string stringParameters)
    {
        // 由于Unity的动画时间轴上的参数只能传递一个，那么我们需要通过|来q区分传递的参数
        //拆分参数字符串
        string[] paras = stringParameters.Split('|');
        string childCompName = paras[0];
        string attackPointName = paras[1];
        //确认攻击者移动的方向

        //获得攻击者的位置和目标者的位置
        Transform[] childrenTransform = GetComponentsInChildren<Transform>();
        GameObject childCompObj = null;
        foreach (Transform childTransform in childrenTransform)
        {
            if (childTransform.name == childCompName)
            {
                childCompObj = childTransform.gameObject;
            }
        }
        
        Transform target = beattacked.attackPrefab.transform;

        //获取攻击点
        if (attackPointName != "")
        {
            foreach (var attackpoint in target.GetComponentsInChildren<AttackPoint>())
            {
                if (attackpoint.attack_point_name == attackPointName)
                {
                    target = attackpoint.GetComponent<Transform>();
                }
            }
        }

        activeUnit_ori_pos = childCompObj.transform.position;
        Vector3 moveNormalizedAttackDir = (target.position - childCompObj.transform.position).normalized;

        float distance = Vector3.Distance(childCompObj.transform.position, target.position);

        float speed = distance / animationTime * Time.deltaTime;

        while (Vector3.Distance(childCompObj.transform.position, target.position) > 1f)
        {
            childCompObj.transform.position += speed * moveNormalizedAttackDir;
            yield return null;
        }
        childCompObj.transform.localPosition = Vector3.zero;

    }
    public IEnumerator AttackResult()
    {
        attackUnitAnimator.speed = 0;
        yield return ResultHandle();
        attackUnitAnimator.speed = 1;
    }

    public void PlayAttackerAnimation(string animationName)
    {
        attackUnit.attackPrefab.GetComponentInChildren<Animator>().Play(animationName);
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

        if(randomSeed >= 102)
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
            this.beattacked.DamageTaken(attackUnit);
            yield return new WaitForSeconds(2);
        }

    }
}
