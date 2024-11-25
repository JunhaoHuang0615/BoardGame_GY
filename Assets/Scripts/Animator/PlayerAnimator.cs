using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    public Animator animator;

    //给Animator添加参数
    public void SetAnimationParam(Unit unit, int x, int y)
    {
        animator.SetBool("Selected",unit.selected);
        animator.SetBool("Stand", unit.stand);
        animator.SetInteger("X",x);
        animator.SetInteger("Y", y);
    }

    public void SetAnmationTrigger(string trigger)
    {   
        animator.SetTrigger(trigger);
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void SetAnimationBool(string paraName,bool value)
    {
        animator.SetBool(paraName,value);
    }
}
