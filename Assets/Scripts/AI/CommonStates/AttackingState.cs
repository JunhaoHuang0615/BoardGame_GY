
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public AttackingState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Attacking");
        this.aiUnit.RecordAttackRangeTiles(this.aiUnit.standOnTile); //记录ai的攻击范围
        this.aiUnit.isAttacking = true;
        if(GameManager.Instance.selectedUnit == null)
        {
            return;
        }
        GameManager.Instance.aiTarget.isAttackable = true;
        GameManager.Instance.selectedUnit.canAttack = true;
        GameManager.Instance.passiveUnit = GameManager.Instance.aiTarget;
        GameManager.Instance.activeUnit = this.aiUnit;
        //注意：不要在这里调用Stand()，因为战斗还没有开始
        //Stand()应该在战斗完成后，由StandState.OnEnter()调用
        SceneLoader.Instance.StartBattle();
        CameraFollow.instance.RecordCameraPosition();
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        if(aiUnit.health <= 0)
        {
            //进入死亡状态
            this.fsm.TransitionToState(StateType.FINISH);
        }
        else if(aiUnit.isAttacking == false)
        {
            this.fsm.TransitionToState(StateType.STAND);
        }

    }


}
