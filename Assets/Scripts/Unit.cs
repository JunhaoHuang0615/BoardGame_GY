

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CharacterType
{
    Soldier,
    Archer, //弓兵可爬山
    Good, //地貌单位
}

public class Unit : MonoBehaviour
{
    //系统
    public bool selected; 
    //角色属性
    public int playerID;  //玩家ID
    public int health;
    public int moveRange;//移动范围
    public int moveSpeed; //移动的速度
    public int attackRange;
    public int attackAbility;
    public bool isAttackable;
    public int defenseAbility;
    public CharacterType type;
    public int flyHeight; //用于判定是否可以行走到某个格子
    //角色状态
    public bool hasMoved;
    public bool hasAttacked;
    public bool stand;

    //单位站在哪一个格子上
    public Tile standOnTile;
    public Tile previousStandTile; //记录之前所占的格子
    public Color moveableHightColor;
    public Color attackableHightColor;
    public ButtonList buttonList;

    private GameManager gm;
    private ObjectPool obp;
    private SceneLoader sl;
    public PlayerAnimator playerAnimator;
    public bool canExcute; //判定角色是否可以执行操作
    public bool canAttack;
    public bool isAttacking;

    public BattlePrefabType battlePreType;
    public GameObject attackPrefab;
    public Animator attackPrefabAnimator;
    public AttackType attackType;
    // Start is called before the first frame update
    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        obp = FindObjectOfType<ObjectPool>();
        buttonList = gameObject.AddComponent<ButtonList>();
        buttonList.unit = this;
        playerAnimator = this.GetComponent<PlayerAnimator>();
        sl = FindObjectOfType<SceneLoader>();
    }

    //这个事件要求gameobject挂载collider
    //object在Z轴方向上的位置
    private void OnMouseDown()
    {   
        if(this.isAttackable == true && gm.selectedUnit !=null && gm.selectedUnit.canAttack == true)
        {
            gm.passiveUnit = this;
            gm.activeUnit = gm.selectedUnit;
            gm.activeUnit.Stand();
            StartCoroutine( sl.LoadBattleScene());
            CameraFollow.instance.RecordCameraPosition();
        }
        if(this.playerID != gm.nowPlayerID)
        {
            return;
        }
        if(this.stand == true)
        {
            return;
        }
        if(gm.isAnimating == true)
        {
            return;
        }
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        EnableGoodCollider(true);
        gm.EnablePlayerCollider(true);
        CloseButtonList();

        //取消选择
        if (selected == true && gm.selectedUnit == this)
        {   
            selected = false;
            gm.selectedUnit = null;

        }
        //当前的Unit没有被选择，但是有其他被选择的Unit
        else if (selected == false && gm.selectedUnit != null)
        {
           
            gm.selectedUnit.CloseButtonList();
            gm.selectedUnit.selected = false;
            gm.selectedUnit.playerAnimator.SetAnimationParam(gm.selectedUnit, 0, 0);
            selected = true;
            gm.selectedUnit = this;
            //ShowMoveRange();
            OpenButtonList();
            if(!this.hasMoved)
            ShowMoveRangeTwo();
            else if (!hasAttacked)
            {
                ShowAttackRange(gm.selectedUnit.standOnTile);
            }

        }
        else if (!selected && gm.selectedUnit == null)
        {   
            selected = true;
            gm.selectedUnit = this;
            //ShowMoveRange();
            OpenButtonList();
            if (!this.hasMoved)
                ShowMoveRangeTwo();
            else if (!hasAttacked)
            {
                ShowAttackRange(gm.selectedUnit.standOnTile);
            }
        }
        playerAnimator.SetAnimationParam(this,0,0);
    }
    //要求： 动画名称， Attack类型， 动画设置内的Trigger名称，必须完全一致
    public IEnumerator Attack(Unit attackedUnit)
    {
        attackPrefabAnimator = attackPrefab.GetComponentInChildren<Animator>();
        attackPrefabAnimator.SetTrigger(GetAttackTrigger(this.attackType)); //开启攻击动画
        attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 16;
        attackedUnit.attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 15;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().attackUnit = this;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().beattacked = attackedUnit;


        while (!attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(GetAttackTrigger(this.attackType)))
        {
            yield return null;
        }
        //判断当前动画是否已经完成
        while (attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(GetAttackTrigger(this.attackType)))
        {
            yield return null; //卡在动画播放
        }

    }
    //attackTile 发起攻击的格子
    public void ShowAttackRange(Tile attackTIle)
    {   
        //now : 存放的是当前正在进行检测的Tile
        //close: 已经被检测的Tile
        //Open: 一次检测中，成功流水的Tile，目的是为了下一次的检测
        List<Tile> now = new List<Tile>();
        List<Tile> open = new List<Tile>();
        List<Tile> close = new List<Tile>();

        //第一次检测的时候，玩家起点即是第一次的检测点
        now.Add(attackTIle);
        for(int i = 0; i< attackRange; i++)
        {
            //判断监测点四个方向是否可以有水通过
            foreach (var current in now)
            {
                //将检测过的点放入close
                close.Add(current);
                //得到相邻的点
                List<Tile> currentTileNeighbors = current.neighbors;
                //判断相邻的点是否有水可以经过，加入到open列表中
                foreach (var neighbor in currentTileNeighbors)
                {
                    //已经检测过的点，不需要放入open
                    if (close.Contains(neighbor))
                    {
                        continue;
                    }
                    //open列表已经存在了的neighbor，不需要重复进入open
                    if (open.Contains(neighbor))
                    {
                        continue;
                    }
                    else
                    {
                        if (!neighbor.isMoveableTile)
                        {
                            open.Add(neighbor);
                            //tile变成高亮显示
                            neighbor.HightAttackableTile();
                            if (neighbor.unitOnTile != null)
                                neighbor.unitOnTile.HightAttackUnitSprite();
                        }
                        if(!gm.attackRangeTiles.Contains(neighbor))
                            gm.attackRangeTiles.Add(neighbor);
                    }
                }
            }
            //找到此次水可以通过的tile
            now.Clear();//检测完后清空Now列表
                        //open列表复制到Now中
            foreach (var tile in open)
            {
                now.Add(tile);  //把此次水通过的tile，变成下一次的检测点
            }
            open.Clear();
        }
    }

    //进阶版洪水算法
    public void ShowMoveRangeTwo()
    {
        if (!hasMoved)
        {
            EnableGoodCollider(false);
            //洪水算法起始点
            DFSShowMoveRange(gm.selectedUnit.moveRange, gm.selectedUnit.standOnTile);
            ShowAttackRangeInMoveRange(gm.moveableTiles);
        }

    }
    // now: 代表现在正在检测的点
    //close: 已经检测过的点
    //open: 是递归的中间产物，即是下一次递归的now
    public void Flood(int RemainMoveRange,Tile now)
    {
        if(RemainMoveRange <= 0)
        {
            return;
        }
        else
        {
            //得到检测点邻边的tile
            List<Tile> currentTileNeighbors = now.neighbors;
            //对每一个Neighbor进行衡量，决定是否可以有水流过去
            foreach(var neighbor in currentTileNeighbors)
            {
 
                if(RemainMoveRange >= neighbor.GetNeedMoveAbility(gm.selectedUnit))
                {
                    //可以流入水
                    //tile变成高亮显示
                    neighbor.HightMoveableTile();
                    if (neighbor.unitOnTile != null)
                        neighbor.unitOnTile.HightUnitSprite();
                    //加入到可移动的List中
                    if(!gm.moveableTiles.Contains(neighbor))
                    gm.moveableTiles.Add(neighbor);
                    //递归，进行下一次的检测
                    //传入剩下的点数，第二个参数：open
                    Flood(RemainMoveRange - neighbor.GetNeedMoveAbility(gm.selectedUnit), neighbor);

                }
                
            }
        }
    }

    //depth first search
    public void DFSShowMoveRange(int unitMoveAbilty, Tile startTile)
    {
        //把所有的Tile的所需的移动点数设置成99
        foreach(var tile in gm.tiles)
        {
            tile.needAcrtionPoint = 99;
        }
        //监测点队列
        Queue<Tile> opens = new Queue<Tile>();
        //存储可移动的Tile的列表
        List<Tile> moveableTileList = new List<Tile>();

        //startTile的所需点数初始化为0
        startTile.needAcrtionPoint = 0;
        opens.Enqueue(startTile);

        while(opens.Count > 0)
        {
            //出列监测点
            Tile now = opens.Dequeue();
            if(now.needAcrtionPoint > unitMoveAbilty)
            {
                continue;
            }
            else
            {
                moveableTileList.Add(now);
            }

            //判断邻边的格子
            foreach(var neighbor in now.neighbors)
            {
                //计算邻边的格子到起点所需的行动点数
                int neighborActionPointFromStartTile = now.needAcrtionPoint + neighbor.GetNeedMoveAbility(gm.selectedUnit);
                //对比原来所需的点数，小的就入列，大的舍去
                if(neighborActionPointFromStartTile < neighbor.needAcrtionPoint) {
                    neighbor.needAcrtionPoint = neighborActionPointFromStartTile;
                    opens.Enqueue(neighbor);
                }
            }
        }
        if(moveableTileList.Count > 0)
        {
            foreach(var tile in moveableTileList)
            {
                tile.HightMoveableTile();
                if (tile.unitOnTile != null)
                    tile.unitOnTile.HightUnitSprite();
                //加入到可移动的List中
                if (!gm.moveableTiles.Contains(tile))
                    gm.moveableTiles.Add(tile);
            }
        }
    }
    public void HightUnitSprite()
    {
        if (type == CharacterType.Good)
        {
            this.GetComponent<SpriteRenderer>().color = moveableHightColor;
        }
    }
    public void HightAttackUnitSprite()
    {
        if (type == CharacterType.Good)
        {
            this.GetComponent<SpriteRenderer>().color = attackableHightColor;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = attackableHightColor;
            this.isAttackable = true;
        }
    }
    public void RestHightUnitSprite()
    {
        this.GetComponent<SpriteRenderer>().color = Color.white;
        this.isAttackable = false;
        this.canExcute = false;
    }
    public void EnableGoodCollider(bool enable)
    {
        foreach (var unit in gm.allUnits)
        {
            if (unit.type == CharacterType.Good)
            {
                unit.GetComponent<Collider2D>().enabled = enable;
            }
        }
    }

    public void ShowAttackRangeInMoveRange(List<Tile> moveRangeList)
    {   
        if(gm.attackRangeTiles.Count > 0)
        {
            gm.attackRangeTiles.Clear();
        }
        foreach(var moveTile in moveRangeList)
        {
            List<Tile> neighbors = moveTile.neighbors;
            foreach (var neighbor in neighbors)
            {
                if(neighbor.isMoveableTile == false)
                {
                    //说明此MoveTile的邻边存在不可移动的格子，是一个边缘格子
                    ShowAttackRange(moveTile); //传入的是边缘的格子
                }
            }
        }
    }
    public void Move(List<Tile> path)
    {
        //协程
        StartCoroutine(MoveTo(path));
    }

    //协程方法
    IEnumerator MoveTo(List<Tile> path)
    {
        gm.isAnimating = true;
        this.CloseButtonList();
        previousStandTile = standOnTile;
        Vector2Int previousVector = Vector2Int.zero; //0 ,0
        for (int i = 1; i < path.Count; i++)
        {
            var tile = path[i];
            var previousTile = path[i - 1];
            Vector2Int currentVector = new Vector2Int((int)(tile.transform.position.x - previousTile.transform.position.x),
                                                      (int)(tile.transform.position.y - previousTile.transform.position.y));
            if(currentVector != previousVector)
            {
                playerAnimator.SetAnimationParam(this,currentVector.x,currentVector.y);
                previousVector = currentVector;
            }
            while (Vector3.Distance(transform.position,tile.transform.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, tile.transform.position, moveSpeed * Time.deltaTime);
                gm.cameraFollow.SetCameraPosition(()=> transform.position);
                yield return null;
            }
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        EnableGoodCollider(true);
        this.hasMoved = true;
        OpenButtonList();
        previousStandTile.UnitOnTile();
        ShowAttackRange(this.standOnTile);
        Action moveAction = new Action(ResetUnitPosition);
        gm.actions.Push(moveAction);
        gm.isAnimating = false;
        gm.cameraFollow.ResetCameraPosition();
    }
    public void OpenButtonList()
    {
        if (buttonList.buttons.Count > 0)
        {
            return;
        }
        DesideButton();
        buttonList.GenerateButtons();
    }
    public void CloseButtonList()
    {
        buttonList.CloseButtons();
    }

    public void DesideButton()
    {
        if (!this.hasMoved)
        {
            buttonList.AddButton(GameObjectType.MOVEBUTTON);
        }
        if (!this.hasAttacked)
        {
            buttonList.AddButton(GameObjectType.ATTACKBUTTON);
        }
        if(!hasAttacked || !hasMoved)
        {   
            buttonList.AddButton(GameObjectType.STANDBUTTON);
        }
    }

    public void ResetUnitPosition()
    {   
        if(this.stand == true)
        {   
            if(gm.actions.Count > 0)
            {
                Action action = gm.actions.Pop();
                action();
            }

            return;
        }

        //归还角色位置
        this.standOnTile.unitOnTile = null;
        Tile tempTile = this.standOnTile;
        this.standOnTile = previousStandTile;
        previousStandTile = tempTile;
        this.standOnTile.unitOnTile = this;
        this.transform.position = standOnTile.transform.position;
        this.transform.position = new Vector3(transform.position.x,transform.position.y,-1);
        this.GetComponent<Collider2D>().enabled = false;
        previousStandTile.UnitOnTile();
        CloseButtonList();
        //角色的参数还原
        hasMoved = false;
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        canExcute = true;
        if (gm.selectedUnit != null && gm.selectedUnit != this)
        {
            gm.selectedUnit.CloseButtonList();
            gm.selectedUnit.selected = false;
            gm.selectedUnit.hasMoved = false;
            gm.selectedUnit.playerAnimator.SetAnimationParam(gm.selectedUnit, 0, 0);
        }
        //OpenButtonList();
        gm.selectedUnit = this;
        gm.selectedUnit.selected = true;
        this.GetComponent<Collider2D>().enabled = true;
        ShowMoveRangeTwo();
        playerAnimator.SetAnimationParam(this,0,-1);

    }
    public void RestUnitState()
    {
        hasAttacked = false;
        hasMoved = false;
        stand = false;
        canExcute = false;
        isAttackable = false;
        this.GetComponent<SpriteRenderer>().color = Color.white;
        selected = false;
        playerAnimator.SetAnimationParam(this,0,0);
        buttonList.CloseButtons();
    }

    public string GetAttackTrigger(AttackType attackType)
    {
        return attackType.ToString();
    }

    public void Stand()
    {
        CloseButtonList();
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        stand = true;
        if (gm.selectedUnit != null && gm.selectedUnit == this)
        {
            gm.selectedUnit = null;
        }
        this.selected = false;
        this.playerAnimator.SetAnimationParam(this, 0, 0);
    }

    public void Idle()
    {
        playerAnimator.SetAnimationParam(this,0,0);
        this.hasMoved = false;
        this.hasAttacked = false;
        this.selected = false;
        this.stand = false;
    }

}
