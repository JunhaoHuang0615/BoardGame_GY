
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargettingState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public TargettingState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Targetting");

        if((GameManager.Instance.aiTarget = FindFinalTagetUnit()) != null)
        {   
            //找到了目标
            this.fsm.TransitionToState(StateType.MOVING);
        }
        else
        {
            this.fsm.TransitionToState(StateType.STAND);
        }
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {


    }   

    //寻找潜在目标（寻找潜在的敌人）
    public List<Unit> FindPotentialTarget()
    {
        List<Unit> potentialTargets = new List<Unit>();
        if(GameManager.Instance.moveableTiles.Count > 0)
        {
            GameManager.Instance.moveableTiles.Clear();
        }
        if(GameManager.Instance.attackRangeTiles.Count > 0)
        {
            GameManager.Instance.attackRangeTiles.Clear();
        }

        aiUnit.ShowMoveRangeTwo();

        foreach(var tile in GameManager.Instance.moveableTiles)
        {
            if(tile.unitOnTile != null &&tile.unitOnTile.tag == "Player" && tile.unitOnTile.playerID != aiUnit.playerID && (!potentialTargets.Contains(tile.unitOnTile)))
            {
                potentialTargets.Add(tile.unitOnTile);
            }
        }
        foreach (var tile in GameManager.Instance.attackRangeTiles)
        {
            if (tile.unitOnTile != null && tile.unitOnTile.tag == "Player" && tile.unitOnTile.playerID != aiUnit.playerID && (!potentialTargets.Contains(tile.unitOnTile)))
            {
                potentialTargets.Add(tile.unitOnTile);
            }
        }

        return potentialTargets;
    }
    //计算当前AI和潜在目标之间的距离分数

    private int CalculateDistanceScore(Unit unit)
    {
        //使用A星算法，得到目标所在格子的位置
        Tile endTile = PathFinding.Instance.AStarPathFindDistance(aiUnit.standOnTile, unit.standOnTile); //目的是拿到最短路径
        Tile tempTile = endTile;

        int score = 100;

        while(tempTile != null)
        {
            score -= 1;
            tempTile = tempTile.parentTile;
        }
        //最短路径越长，得分越低

        return score;
    }

    private Unit FindFinalTagetUnit()
    {
        Unit result = null;

        List<Unit> potentialUnits = FindPotentialTarget();
        //用一个字典存储目标 和 最终的得分
        Dictionary<Unit, float> targetUnitScore = new Dictionary<Unit, float>();


        if(potentialUnits.Count > 0)
        {
            foreach(var potentialUnit in potentialUnits)
            {
                int totalScore = 0;
                int destinationScore = CalculateDistanceScore(potentialUnit);
                totalScore += destinationScore; //距离得分
                totalScore += potentialUnit.health * -1; //生命值得分  后期，要根据种类的比重来计算

                targetUnitScore.Add(potentialUnit,totalScore);

            }
        }

        //寻找得分高的目标
        if(targetUnitScore.Count > 0) {
            result = potentialUnits[0]; //首先让result等于列表中的第一个元素
            foreach (var unit in potentialUnits)
            {
                if(targetUnitScore[unit] > targetUnitScore[result])
                {
                    result = unit;
                }
            }
        }


        return result;
    }


}

