

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.UI.CanvasScaler;

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
    public int health; //currenthealth
    public int maxHealth;
    public int moveRange;//移动范围
    public int moveSpeed; //动画的移动的速度
    public int attackRange; //根据装备来确定！
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
    public EquipmentList equipList; //武器包
    public List<string> attackEquipList = new List<string>(); //武器包里的武器
    public Weapon attackEquipUsedRightNow; //当前单位正在使用的武器,只有单位在真正使用了之后，才会进行切换
    public Weapon selectEquip; //选择的武器
    public AttackType attackType; //要根据武器来定

    private GameManager gm;
    private DataManager dataManager;
    private ObjectPool obp;
    private SceneLoader sl;
    public PlayerAnimator playerAnimator;
    public bool canExcute; //判定角色是否可以执行操作
    public bool canAttack;
    public bool isAttacking;


    public BattlePrefabType battlePreType;
    public GameObject attackPrefab;
    public Animator attackPrefabAnimator;
    private List<Tile> counterAttackTiles = new List<Tile>();
    // Start is called before the first frame update
    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        obp = FindObjectOfType<ObjectPool>();
        buttonList = gameObject.AddComponent<ButtonList>();
        buttonList.unit = this;
        equipList = gameObject.AddComponent<EquipmentList>();
        equipList.unit = this;
        playerAnimator = this.GetComponent<PlayerAnimator>();
        sl = FindObjectOfType<SceneLoader>();
        
    }
    void Start()
    {
        Invoke("InitData", 0.2f);

    }

    void InitData()
    {
        dataManager = FindObjectOfType<DataManager>();
        if(attackEquipList.Count > 0)
        {
            InitWeapon();
        }
    }
    public void InitWeapon()
    {
        selectEquip = dataManager.GetWeapon(attackEquipList[0]);
        this.attackRange = selectEquip.Range;
        this.attackType = (AttackType)Enum.Parse(typeof(AttackType), selectEquip.AttackType);
        this.attackAbility = selectEquip.Attack;
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
        CloseAttackEquipList();
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
            gm.selectedUnit.CloseAttackEquipList();
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
                ShowAttackRange(gm.selectedUnit.standOnTile,true);
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
                ShowAttackRange(gm.selectedUnit.standOnTile,true);
            }
        }
        playerAnimator.SetAnimationParam(this,0,0);
    }

    public IEnumerator Attack(Unit attackedUnit)
    {
        attackPrefabAnimator = attackPrefab.GetComponentInChildren<Animator>();
        attackPrefabAnimator.SetTrigger(attackType.ToString()); //开启攻击动画
        attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 16;
        attackedUnit.attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 15;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().attackUnit = this;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().beattacked = attackedUnit;


        while (!attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackType.ToString()))
        {
            yield return null;
        }
        //判断当前动画是否已经完成
        while (attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackType.ToString()))
        {
            yield return null; //卡在动画播放
        }

    }
    //attackTile 发起攻击的格子
    //needSaveAttackTiles: 确认发动了攻击，才会进行存储
    public void ShowAttackRange(Tile attackTIle,bool needSaveAttackTiles = false)
    {   
        gm.attackRangeTiles.Clear();
        //洪水攻击类型
        switch (this.selectEquip.Range_Pattern)
        {
            case "Mele": //近战
                FloodAttackRange(attackTIle);
                break;
            case "Ten":
                CrossAttackRange(attackTIle, selectEquip.Range, selectEquip.Range);
                break;
            case "Hoseki":
                FloodAttackRangeWithUnattackableRange(attackTIle, 1, selectEquip.Range);
                break;
            default:
                FloodAttackRangeWithUnattackableRange(attackTIle, 1, selectEquip.Range);
                break;

        }
        // FloodAttackRange(attackTIle);
        //弓箭手 内圈格子（不允许攻击的范围（0~1范围无法攻击），外圈格子允许攻击的范围（1~2可以攻击））
        // FloodAttackRangeWithUnattackableRange(attackTIle,2,4);
        //十字型攻击方式
        
        if (needSaveAttackTiles) {
            this.counterAttackTiles.Clear();
            //不可以直接等于gm.attackRangeTiles，否则gm.attackRangeTiles一旦发生变化，counterAttackTiles也会跟着发生变化
            foreach (var tiles in gm.attackRangeTiles)
            {
                this.counterAttackTiles.Add(tiles);
            }

        }


    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attackTIle">攻击的发起格子</param>
    /// <param name="across">横向range</param>
    /// <param name="vertical">纵向Range</param>
    /// <param name="blockable">是否会被地形（墙）阻挡，即如果Tile的属性tiletype为wall的话，则无法继续延伸，但是可以将wall视为攻击目标，因此wall的那一个tile也是可以被攻击到的</param>
    public void CrossAttackRange(Tile attackTIle, int across, int vertical,bool blockable = true)
    {
        List<Tile> close = new List<Tile>(); // 用于记录已处理过的格子

        // 使用Vector的预定义方向：上、下、左、右
        Vector2[] directions = new Vector2[]
        {
        Vector2.up,    // 上
        Vector2.down,  // 下
        Vector2.right, // 右
        Vector2.left   // 左
        };

        // 分别处理横向和纵向
        for (int dirIndex = 0; dirIndex < directions.Length; dirIndex++)
        {
            Vector2 direction = directions[dirIndex];
            int range = (direction.x == 0) ? vertical : across;  // 判断是横向还是纵向

            Tile currentTile = attackTIle;
            for (int i = 1; i <= range; i++)
            {
                // 找到当前方向上i步的Tile
                Tile nextTile = currentTile.GetNeighbourInDirection(direction);
                // 如果没有下一个格子，或者超出了地图边界，停止检测
                if (nextTile == null)
                {
                    break;
                }

                // 如果是可移动的格子，跳过（也就是说只在可以移动的格子上显示攻击范围）
                if (nextTile.isMoveableTile)
                {
                    continue;
                }

                // 如果被墙阻挡（根据tileType判断），且blockable为true，则停止检测
                if (blockable && nextTile.tileType == TileType.Wall)
                {
                    break;
                }

                // 标记为可攻击格子
                nextTile.HightAttackableTile();
                if (nextTile.unitOnTile != null)
                    nextTile.unitOnTile.HightAttackUnitSprite();

                // 将格子加入全局攻击范围
                if (!gm.attackRangeTiles.Contains(nextTile))
                    gm.attackRangeTiles.Add(nextTile);

                // 更新当前处理的格子
                currentTile = nextTile;

                // 记录已经处理过的Tile
                close.Add(nextTile);
            }
        }
    }
    public void FloodAttackRangeWithUnattackableRange(Tile attackTIle,int inside, int outside)
    {
        List<Tile> now = new List<Tile>();
        List<Tile> open = new List<Tile>();
        List<Tile> close = new List<Tile>();

        now.Add(attackTIle);
        for (int i = 0; i < outside; i++)
        {
            foreach (var current in now)
            {
                close.Add(current);
                List<Tile> currentTileNeighbors = current.neighbors;
                foreach (var neighbor in currentTileNeighbors)
                {
                    if (close.Contains(neighbor) || open.Contains(neighbor))
                    {
                        continue;
                    }
                    open.Add(neighbor);
                    // 如果格子不可移动，加入open
                    if (!neighbor.isMoveableTile)
                    {
                        // 判断当前格子是否在inside到outside之间
                        if (i >= inside)
                        {   
                            // 只标记inside到outside的格子为可攻击
                            neighbor.HightAttackableTile();
                            if (neighbor.unitOnTile != null)
                                neighbor.unitOnTile.HightAttackUnitSprite();
                            // 将格子加入全局攻击范围
                            if (!gm.attackRangeTiles.Contains(neighbor))
                                gm.attackRangeTiles.Add(neighbor);
                        }

                    }
                }
            }

            now.Clear();
            foreach (var tile in open)
            {
                now.Add(tile);
            }
            open.Clear();
        }

    }
    public void FloodAttackRange(Tile attackTIle)
    {
        //now : 存放的是当前正在进行检测的Tile
        //close: 已经被检测的Tile
        //Open: 一次检测中，成功流水的Tile，目的是为了下一次的检测
        List<Tile> now = new List<Tile>();
        List<Tile> open = new List<Tile>();
        List<Tile> close = new List<Tile>();

        //第一次检测的时候，玩家起点即是第一次的检测点
        now.Add(attackTIle);
        for (int i = 0; i < attackRange; i++)
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
                        if (!gm.attackRangeTiles.Contains(neighbor))
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
                {
                    gm.moveableTiles.Add(tile);
                }
                    
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
            ShowAttackRange(moveTile);
            //List<Tile> neighbors = moveTile.neighbors;
/*            foreach (var neighbor in neighbors)
            {
                if (neighbor.isMoveableTile == false)
                {

                    //说明此MoveTile的邻边存在不可移动的格子，是一个边缘格子
                    ShowAttackRange(moveTile); //传入的是移动范围最边缘的格子
                }
            }*/
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
        ShowAttackRange(this.standOnTile,true);
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
    public void CloseAttackEquipList()
    {
        equipList.CloseButtons();
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

    public void ChangeWeapon(Weapon weapon)
    {
        this.attackRange = weapon.Range;
        this.attackAbility = weapon.Attack;
        this.attackType = (AttackType)Enum.Parse(typeof(AttackType), weapon.AttackType);
    }

    public bool CanCounterAttack(Unit attckUnit)
    {
        if (counterAttackTiles.Count > 0) {
            foreach (var tile in counterAttackTiles)
            {
                if(tile.unitOnTile != null && tile.unitOnTile == attckUnit)
                {   
                    return true;
                }
            }
        }
        else
        {
            //要重新执行一次ShowAttackRange,适用于第一回合，AI没有发动攻击的时候
            ShowAttackRange(this.standOnTile, true);
            foreach (var tile in counterAttackTiles)
            {
                if (tile.unitOnTile != null && tile.unitOnTile == attckUnit)
                {
                    gm.ResetMoveableRange();
                    gm.ResetMovePath();
                    return true;
                }
            }
            gm.ResetMoveableRange();
            gm.ResetMovePath();

        }

        return false;
    }

    public int CalculateDamage(Unit targetUnit)
    {
        //需要考虑的情况：防御力高于攻击力
        int damage = Mathf.Max(this.attackAbility - targetUnit.defenseAbility, 0);

        targetUnit.health -= damage;
        
        return damage; //用于动画显示
    }

}
