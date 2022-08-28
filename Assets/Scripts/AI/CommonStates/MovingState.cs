
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;

    public MovingState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Moving");
        Tile targetTile = DesideMovingTile();
        if(targetTile != null && targetTile != aiUnit.standOnTile)
        {
            aiUnit.canExcute = true;
            Tile endTile = PathFinding.Instance.AStarPathFind(aiUnit.standOnTile,targetTile);
            PathFinding.Instance.GetPath(endTile);

            targetTile.SendUnitHere(aiUnit,PathFinding.Instance.path);
        }
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

    public Tile DesideMovingTile()
    {
        Tile targetTile = null;
        //拿到目标对象
        var targetUnit = GameManager.Instance.aiTarget;

        //以目标对象为中心，以ai的攻击范围为半径，执行ShowAttackRange()
        if(GameManager.Instance.attackRangeTiles.Count > 0)
        {
            GameManager.Instance.attackRangeTiles.Clear();
        }
        GameManager.Instance.ResetMovePath();
        GameManager.Instance.ResetMoveableRange();
        aiUnit.ShowAttackRange(targetUnit.standOnTile); //会将攻击范围的格子存储于gm里的attackRangeTiles;
        aiUnit.DFSShowMoveRange(aiUnit.moveRange, aiUnit.standOnTile);
        //确定潜在可移动的格子
        List<Tile> potentialMoveTile = new List<Tile>();
        foreach(var tile in GameManager.Instance.attackRangeTiles)
        {
            if (GameManager.Instance.moveableTiles.Contains(tile) && tile.unitOnTile == null)
            {
                potentialMoveTile.Add(tile);
            }
        }

        Dictionary<Tile, float> tileScoreDict = new Dictionary<Tile, float>();
        if(potentialMoveTile.Count > 0)
        {   
            foreach(var potentialTile in potentialMoveTile)
            {
                //要选择一个距离目标对象越远，并且离自己越近的格子
                float target_potential_DisScore = CalculateDistanceScore(targetUnit.standOnTile, potentialTile);
                float potential_ai_DisScore = CalculateDistanceScore(potentialTile, aiUnit.standOnTile);
                float resultScore = target_potential_DisScore * 10 - potential_ai_DisScore;
                tileScoreDict.Add(potentialTile,resultScore);
            }
        }

        if(tileScoreDict.Count > 0)
        {
            targetTile = potentialMoveTile[0]; //首先让result等于列表中的第一个元素
            foreach (var tile in potentialMoveTile)
            {
                if (tileScoreDict[tile] > tileScoreDict[targetTile])
                {
                    targetTile = tile;
                }
            }
        }

        return targetTile;
    }

    private float CalculateDistanceScore(Tile startTile, Tile targetTile)
    {
        Tile endTile = PathFinding.Instance.AStarPathFind(startTile, targetTile);

        Tile tempTile = endTile;

        float score = 0;

        while(tempTile != null)
        {
            score += 1;
            tempTile = tempTile.parentTile;
        }
        return score;
    }


}
