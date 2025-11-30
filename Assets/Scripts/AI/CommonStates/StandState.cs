
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
        //行动值系统：AI完成行动后，如果不再是当前可行动单位，切换到IDLE
        //当currentActiveUnit不是当前单位时，说明该AI已经完成了行动，可以回到IDLE状态
        if (GameManager.Instance.currentActiveUnit != this.aiUnit)
        {
            this.fsm.TransitionToState(StateType.IDLE);
        }

        //旧回合制系统（已注释）
        //if (GameManager.Instance.nowPlayerID == 1)
        //{
        //    this.fsm.TransitionToState(StateType.IDLE);
        //}
    }


}
