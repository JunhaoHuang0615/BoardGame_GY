
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public StandState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        this.aiUnit.Stand();
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        //AI回合结束的时候，进入IDLE
        if (GameManager.Instance.nowPlayerID == 1)
        {
            this.fsm.TransitionToState(StateType.IDLE);
        }

    }


}
