
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class TargettingState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;
    private Weapon unit_weapon_before; //如果AI不发动攻击，需要切换回之前的武器。

    public TargettingState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Targetting");
        Weapon selected_weapon = null;
        if((GameManager.Instance.aiTarget = FindFinalTagetUnit(out selected_weapon)) != null)
        {
            //找到了目标
            //切换武器：

            this.aiUnit.ChangeWeapon(selected_weapon);
            this.aiUnit.attackEquipUsedRightNow = selected_weapon;
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
    //加入了武器系统，所以我们需要遍历所有的可能性
    public Dictionary<Weapon, List<Unit>> weapon_potentialtargetlist = new Dictionary<Weapon, List<Unit>>();

    public void FindPotentialTargetWithWeapon()
    {
        unit_weapon_before = this.aiUnit.attackEquipUsedRightNow;
        weapon_potentialtargetlist.Clear(); //要把之前的数据清空

        foreach (var weapon in this.aiUnit.attackEquipList)
        {   
            Weapon tempweapon = DataManager.instance.GetWeapon(weapon);
            this.aiUnit.ChangeWeapon(tempweapon);
            List<Unit> potentialUnits = FindPotentialTarget();
            weapon_potentialtargetlist.Add(tempweapon, potentialUnits);
        }
    }
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
    //计算当前AI和潜在目标之间的距离分数,与武器的距离无关

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

    //找每一个武器的潜在目标的最高得分，应该返回某个武器对应的最适合的Target
    private Unit FindTagetUnitPerWeapon(Weapon weapon,out float score)
    {
        Unit result = null;
        score = 0;
        if (!weapon_potentialtargetlist.ContainsKey(weapon))
        {   
            return null;
        }
        List<Unit> potentialUnits = weapon_potentialtargetlist[weapon];
        //用一个字典存储目标 和 最终的得分
        Dictionary<Unit, float> targetUnitScore = new Dictionary<Unit, float>();

        this.aiUnit.ChangeWeapon(weapon);

        if (potentialUnits.Count > 0)
        {
            foreach(var potentialUnit in potentialUnits)
            {
                float totalScore = 0;
                int destinationScore = CalculateDistanceScore(potentialUnit);
                totalScore += destinationScore; //距离得分
                totalScore += potentialUnit.health * -1; //生命值得分  血越少，攻击这个对象的可能性越高，所以这个对象的得分越高
                totalScore += this.aiUnit.attackRange;
                if(potentialUnit.health  - this.aiUnit.attackAbility <= 0) //如果能直接打死，则加分，否则会Range更高的武器得分会更高
                {
                    //totalScore += this.aiUnit.attackAbility;
                    totalScore += 10;
                }

                targetUnitScore.Add(potentialUnit,totalScore);

            }
        }

        //寻找得分高的目标
        if(targetUnitScore.Count > 0) {
            result = potentialUnits[0]; //首先让result等于列表中的第一个元素
            score = targetUnitScore[result];
            foreach (var unit in potentialUnits)
            {
                if(targetUnitScore[unit] > targetUnitScore[result])
                {
                    result = unit;
                    score = targetUnitScore[unit];
                }
            }
        }


        return result;
    }
    //最终结果，我要获得要Target的单位和要使用的武器
    private Unit FindFinalTagetUnit(out Weapon weapon)
    {
        Unit result = null;
        weapon = unit_weapon_before;
        FindPotentialTargetWithWeapon(); 
        float bestscore = -100;
        foreach (var key in weapon_potentialtargetlist.Keys)
        {   
            float tempscore = 0;
            Unit tempunit = FindTagetUnitPerWeapon(key, out tempscore);
            if (tempscore > bestscore) { 
                bestscore = tempscore;
                result =tempunit;
                weapon = key;
            }
        }



        return result;
    }


}

