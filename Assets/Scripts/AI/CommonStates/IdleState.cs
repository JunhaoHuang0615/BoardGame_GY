
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("IDLE");
        this.aiUnit.Idle();
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
       //进入选择对象的State Targetting
       if(aiUnit.selected == true)
        {
            this.fsm.TransitionToState(StateType.TARGETTING);
        }
       //Fininsh状态
       else if(aiUnit.health <= 0)
        {
            this.fsm.TransitionToState(StateType.FINISH);
        }

    }


}
