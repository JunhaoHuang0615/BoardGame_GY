using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishState : AIStates
{
    // Start is called before the first frame update
    private Unit aiUnit;
    private FSM fsm;

    public FinishState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Finished");
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }
}
