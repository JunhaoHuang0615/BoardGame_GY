/***
 *
 * Title:"" 项目：AAA
 * 主题：
 * Description:
 * 功能：
 *
 * Date:2021/
 * Version:0.1v
 * Coder:Junhao Huang
 * email:huangjunhao0615@gmail.com
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateType
{
    IDLE,
    TARGETTING,
    MOVING,
    ATTACKING,
    STAND,
    DEAD,
    FINISH,
}

public class FSM : MonoBehaviour
{
    public Unit aiUnit;
    private AIStates currentStates;
    private Dictionary<StateType, AIStates> stateDict;
    // Start is called before the first frame update
    void Awake()
    {
        aiUnit = gameObject.GetComponent<Unit>(); 
        stateDict = new Dictionary<StateType, AIStates>();
        stateDict.Add(StateType.IDLE, new IdleState(this) );
        stateDict.Add(StateType.TARGETTING, new TargettingState(this));
        stateDict.Add(StateType.MOVING, new MovingState(this));

        TransitionToState(StateType.IDLE);
    }

    public void TransitionToState(StateType stateType)
    {
        if(currentStates != null)
        {
            currentStates.OnExit();
        }
        currentStates = stateDict[stateType];
        currentStates.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentStates != null)
        {
            currentStates.OnUpdate();
        }
    }

    public StateType GetCurrentState()
    {
        foreach(StateType key in stateDict.Keys)
        {
            if(currentStates == stateDict[key])
            {
                return key;
            }
        }
        return 0;
    }
}
