
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargettingState : AIStates
{
    private Unit aiUnit;
    private FSM fsm;
    public Weapon weapon_before;//AI在遍历查找武器的潜在攻击对象之前所装备的武器

    private Dictionary<Weapon,List<Unit>> weaponPotentialTargetDict = new Dictionary<Weapon, List<Unit>>();

    public void FindPotentialTargetWithWeapon()
    {
        weaponPotentialTargetDict.Clear();
        weapon_before = this.aiUnit.CurrentWeapon;
        foreach (var weapon in this.aiUnit.weaponList)
        {
            Weapon tempWeapData = DataManager.Instance.GetWeapon(weapon);
            this.aiUnit.HoveredWeapon(tempWeapData);
            List<Unit> potentialTargetList = this.FindPotentialTarget();
            weaponPotentialTargetDict.Add(tempWeapData, potentialTargetList);

        }
    }

    public TargettingState(FSM fsm)
    {
        this.fsm = fsm;
        this.aiUnit = fsm.aiUnit;
    }

    public void OnEnter()
    {
        Debug.Log("Targetting");
        Weapon best_weapon = null;
        if((GameManager.Instance.aiTarget = FindFinalTagetUnit(out best_weapon)) != null)
        {   
            //找到了目标
            this.aiUnit.SwitchWeapon(best_weapon);
            this.weapon_before = best_weapon;
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

    // 查找某个武器最适合的攻击目标
    private Unit FindTagetUnitPerWeapon(Weapon weapon, out float score)
    {
        Unit result = null;
        float weapon_score = 0; //此武器对应最适合目标的得分
        if (!weaponPotentialTargetDict.ContainsKey(weapon)) {
            score = weapon_score;
            return null;
        }
        List<Unit> potentialUnits = weaponPotentialTargetDict[weapon];

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
                totalScore += (int)weapon.range;
                if(potentialUnit.health - weapon.attackAbility <= 0) //如果此武器可以正好击杀对象，加上武器得分
                {
                    totalScore += 10; //固定加10分，要公平
                }
                targetUnitScore.Add(potentialUnit,totalScore);

            }
        }

        //寻找得分高的目标
        if(targetUnitScore.Count > 0) {
            result = potentialUnits[0]; //首先让result等于列表中的第一个元素
            weapon_score = targetUnitScore[result];
            foreach (var unit in potentialUnits)
            {
                if(targetUnitScore[unit] > targetUnitScore[result])
                {
                    result = unit;
                    weapon_score = targetUnitScore[unit];
                }
            }
        }

        score = weapon_score;
        return result;
    }

    private Unit FindFinalTagetUnit(out Weapon weapon)
    {
        weapon = weapon_before;
        this.FindPotentialTargetWithWeapon();
        float best_score = -100;
        Unit result_unit = null;
        foreach (var each_weapon in weaponPotentialTargetDict.Keys)
        {
            float tempscore = 0;
            Unit tempUnit = this.FindTagetUnitPerWeapon(each_weapon, out tempscore); //某一个武器的最佳攻击目标，以及分数
            if(tempscore > best_score)
            {
                best_score = tempscore;
                result_unit = tempUnit; //取score最大值
                weapon = each_weapon;
            }
        }


        return result_unit;
    }


}

