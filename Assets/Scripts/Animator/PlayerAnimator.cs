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

    public void SetDead(bool isDead)
    {
        animator.SetBool("IsDead", isDead);
        if (isDead)
        {
            animator.SetTrigger("Dead");
        }
    }
}
