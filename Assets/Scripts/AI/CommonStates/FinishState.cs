using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public FinishState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Finish");
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        

    }
}
