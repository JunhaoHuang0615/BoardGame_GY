

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
    public float attackRange;
    public float attackAbility;
    public bool isAttackable;
    public int defenseAbility;
    public CharacterType type;
    public int flyHeight; //用于判定是否可以行走到某个格子
    //行动值系统相关属性
    public int roundspeed; //回合速度，决定行动顺序
    public float currentActionValue; //当前行动值（累积值）

    //速度修改器系统（星穹铁道机制：支持buff/debuff）
    [System.Serializable]
    public class SpeedModifier
    {
        public int speedChange;        // 速度变化值（正数为加速，负数为减速）
        public int remainingRounds;    // 剩余回合数（0=永久，-1=直到手动移除）
        public Unit source;            // 来源单位（可选）
        public string description;     // 描述（可选）

        public SpeedModifier(int speedChange, int remainingRounds, Unit source = null, string description = "")
        {
            this.speedChange = speedChange;
            this.remainingRounds = remainingRounds;
            this.source = source;
            this.description = description;
        }
    }

    private List<SpeedModifier> speedModifiers = new List<SpeedModifier>(); // 速度修改器列表

    //角色状态
    public bool hasMoved;
    public bool hasAttacked;
    public bool stand;

    //单位站在哪一个格子上
    public Tile standOnTile;
    public Tile previousStandTile; //记录之前所占的格子
    public Color moveableHightColor;
    public Color attackableHightColor;
    public Color enterHightColor;
    private bool isEnterable = true;
    public ButtonList buttonList;
    public EquipButtonList equipButtonList; //武器背包
    public List<string> weaponList = new List<string>();
    public Weapon CurrentWeapon; //当前装备的武器
    public Weapon hoverdWeapon; //提供玩家预览时候的武器

    private GameManager gm;
    private ObjectPool obp;
    private SceneLoader sl;
    public PlayerAnimator playerAnimator;
    public bool canExcute; //判定角色是否可以执行操作
    public bool canAttack;
    public bool isAttacking;
    public AttackType attackType;

    public BattlePrefabType battlePreType;
    public GameObject attackPrefab;
    public Animator attackPrefabAnimator;
    // Start is called before the first frame update

    public List<Tile> attackRangetiles = new List<Tile>(); //用于记录当前装备的武器的攻击范围格子
    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        obp = FindObjectOfType<ObjectPool>();
        buttonList = gameObject.AddComponent<ButtonList>();
        buttonList.unit = this;
        equipButtonList = gameObject.AddComponent<EquipButtonList>();
        equipButtonList.unit = this;
        playerAnimator = this.GetComponent<PlayerAnimator>();
        sl = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        Invoke("InitWeapon", 0.2f);
        EventManager.AddEventListener<Unit>("UnitDead", OnUnitDead);
    }

    private void OnUnitDead(Unit deadUnit)
    {
        if (deadUnit != this) {
            //判断死亡的Unit是否是自己
            return;
        }
        //如果是自己。播放死亡动画
        this.playerAnimator.SetDead(true);
        this.standOnTile.unitOnTile = null;
        StartCoroutine(gm.WaitAnimation(this.playerAnimator.animator, "Dead", () =>{
            EventManager.TriggerEvent<Unit>("UnitReturn", deadUnit);
        }));
    }

    //武器的初始化
    void InitWeapon()
    {
        if (weaponList.Count > 0)
        {
            this.SwitchWeapon(DataManager.Instance.GetWeapon(weaponList[0]));
            this.HoveredWeapon(DataManager.Instance.GetWeapon(weaponList[0]));
        }

    }
    private void OnMouseEnter()
    {

        if (isEnterable)
        {
            HightLightEnterUnitSprite();
        }



    }
    public void OnMouseExit()
    {
        if (isEnterable)
        {
            ResettEnterUnitSprite();
        }
    }
    //这个事件要求gameobject挂载collider
    //object在Z轴方向上的位置
    private void OnMouseDown()
    {
        if(this.isAttackable == true && gm.selectedUnit !=null && gm.selectedUnit.canAttack == true)
        {
            gm.passiveUnit = this;
            gm.activeUnit = gm.selectedUnit;
            gm.selectedUnit.SwitchWeapon(gm.selectedUnit.hoverdWeapon);
            gm.selectedUnit.RecordAttackRangeTiles(gm.selectedUnit.standOnTile);//为了做攻击范围的记录

            //行动值系统：不要在这里调用Stand()，因为战斗还没有开始
            //Stand()应该在战斗完成后调用
            //对于玩家单位，战斗完成后需要手动调用Stand()来结束回合
            StartCoroutine( sl.LoadBattleScene());
            CameraFollow.instance.RecordCameraPosition();

            //启动协程等待战斗完成后再调用Stand()
            StartCoroutine(WaitForBattleCompleteAndStand(gm.activeUnit));
        }

        //行动值系统：检查是否是当前可以行动的单位
        //只有玩家单位（playerID == 1）且是当前可行动单位才能被选择
        if (playerID != 1)
        {
            return;
        }
        //如果不是当前可行动单位，不能选择
        if (gm.currentActiveUnit != this)
        {
            return;
        }

        //旧回合制系统检查（已注释）
        //if(this.playerID != gm.nowPlayerID)
        //{
        //    return;
        //}

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
        CloseEquipmentList();

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
            gm.selectedUnit.CloseEquipmentList();
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

    public IEnumerator Attack(Unit attackedUnit)
    {
        attackPrefabAnimator = attackPrefab.GetComponentInChildren<Animator>();
        attackPrefabAnimator.SetTrigger(attackType.ToString()); //开启攻击动画
        attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 16;
        attackedUnit.attackPrefab.GetComponentInChildren<SpriteRenderer>().sortingOrder = 15;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().attackUnit = this;
        attackPrefab.GetComponentInChildren<BattleEventHandlder>().beattacked = attackedUnit;

        attackPrefab.GetComponentInChildren<BattleEventHandlder>().isAttacking = true;

        while (attackPrefab.GetComponentInChildren<BattleEventHandlder>().isAttacking)
        {
            yield return null;
        }
/*        while (!attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackType.ToString()))
        {
            yield return null;
        }
        //判断当前动画是否已经完成
        while (attackPrefabAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackType.ToString()))
        {
            yield return null; //卡在动画播放
        }*/

    }

    //attackTile 发起攻击的格子
    public void ShowAttackRange(Tile attackTile, bool needRecordAttackRangeTiles = false)
    {
        //洪水攻击范围的显示
        //this.FloodAttackRange(attackTile);

        //弓箭手的攻击范围显示
        //this.ArrowAttack(attackTile,2,4);

        //十字的攻击范围
        //this.CrossAttack(attackTile,3,3);

        switch (this.hoverdWeapon.range_pattern)
        {
            case "Mele": //近战
                this.FloodAttackRange(attackTile);
                break;

            case "Ten": //十字攻击
                this.CrossAttack(attackTile,(int)this.attackRange, (int)this.attackRange);
                break;
            case "Archer":
                this.ArrowAttack(attackTile, 1, (int)this.attackRange);
                break;
            case "Hoseki":
                this.FloodAttackRange(attackTile);
                break;
            default:
                this.FloodAttackRange(attackTile);
                break;
        }
        //记录当前装备的武器的攻击范围格子
        if (needRecordAttackRangeTiles) {
            // this.attackRangetiles = gm.attackRangeTiles;  由于是引用对象，是不可以直接赋值的
            // this.attackRangetiles =new List<Tile>(gm.attackRangeTiles); //浅拷贝  这个方法是可以的
            //等同于foreach的写法
            this.attackRangetiles.Clear();
            foreach (var tile in gm.attackRangeTiles)
            {
                this.attackRangetiles.Add(tile);
            }

        }
    }

    public void CrossAttack(Tile attackTile, int hor, int ver, bool blockable = true)
    {
        List < Tile> close = new List < Tile >(); //存储处理过的格子
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
        }; //十字范围的四个方向
        for (int directIndex = 0; directIndex < directions.Length; directIndex++)
        {
            Vector2 direct = directions[directIndex];
            int range = (direct.x == 0) ? ver : hor;
            Tile currentTile = attackTile; //格子的起始点，一定是attackTile
            for (int i = 0; i < range; i++) {
                Tile nextTile = currentTile.GetNeighborTilesWithDirection(direct); //直接拿到方向上的临边格子

                if(nextTile == null) break; // 说明已经到了地图边缘

                if (nextTile.isMoveableTile)
                {
                    continue; //为了不在移动格子内部来显示红色格子
                }

               if(blockable && nextTile.tileType == TileType.Wall)
                {
                    break;
                }

                //标记为可以被攻击的格子

                nextTile.HightAttackableTile();
                if (nextTile.unitOnTile != null)
                    nextTile.unitOnTile.HightAttackUnitSprite();
                if (!gm.attackRangeTiles.Contains(nextTile))
                    gm.attackRangeTiles.Add(nextTile);

                currentTile = nextTile;

                close.Add(nextTile);
            }
        }
    }
    //从attackTile开始，到insideRange的位置，是不可以攻击。
    //从insideRange到outsideRange的位置，可以进行攻击
    public void ArrowAttack(Tile attackTile,int insideRange, int outsideRange)
    {
        //now : 存放的是当前正在进行检测的Tile
        //close: 已经被检测的Tile
        //Open: 一次检测中，成功流水的Tile，目的是为了下一次的检测
        List<Tile> now = new List<Tile>();
        List<Tile> open = new List<Tile>();
        List<Tile> close = new List<Tile>();

        //第一次检测的时候，玩家起点即是第一次的检测点
        now.Add(attackTile);
        for (int i = 0; i < outsideRange; i++)
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
                        open.Add(neighbor);
                        if (!neighbor.isMoveableTile)
                        {

                            //tile变成高亮显示
                            if (i >= insideRange)
                            {
                                neighbor.HightAttackableTile();
                                if (neighbor.unitOnTile != null)
                                    neighbor.unitOnTile.HightAttackUnitSprite();

                            }

                        }
                        if(i >= insideRange)
                        {
                            if (!gm.attackRangeTiles.Contains(neighbor))
                                gm.attackRangeTiles.Add(neighbor);
                        }


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

    public void FloodAttackRange(Tile attackTile)
    {
        //now : 存放的是当前正在进行检测的Tile
        //close: 已经被检测的Tile
        //Open: 一次检测中，成功流水的Tile，目的是为了下一次的检测
        List<Tile> now = new List<Tile>();
        List<Tile> open = new List<Tile>();
        List<Tile> close = new List<Tile>();

        //第一次检测的时候，玩家起点即是第一次的检测点
        now.Add(attackTile);
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
                    gm.moveableTiles.Add(tile);
            }
        }
    }
    public void HightUnitSprite()
    {
        if (type == CharacterType.Good)
        {
            this.isEnterable = false;
            this.GetComponent<SpriteRenderer>().color = moveableHightColor;
        }
    }
    public void HightAttackUnitSprite()
    {
        if (type == CharacterType.Good)
        {
            this.isEnterable = false;
            this.GetComponent<SpriteRenderer>().color = attackableHightColor;
        }
        else
        {
            this.isEnterable = false;
            this.GetComponent<SpriteRenderer>().color = attackableHightColor;
            this.isAttackable = true;
        }
    }
    public void RestHightUnitSprite()
    {
        this.GetComponent<SpriteRenderer>().color = Color.white;
        this.isAttackable = false;
        this.canExcute = false;
        this.isEnterable = true;
    }

    public void HightLightEnterUnitSprite()
    {
        this.GetComponent<SpriteRenderer>().color = enterHightColor;
    }

    public void ResettEnterUnitSprite()
    {
        this.GetComponent<SpriteRenderer>().color = Color.white;
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
/*            foreach (var neighbor in neighbors)
            {
                if(neighbor.isMoveableTile == false)
                {
                    //说明此MoveTile的邻边存在不可移动的格子，是一个边缘格子
                    ShowAttackRange(moveTile); //传入的是边缘的格子
                }
            }*/
            ShowAttackRange(moveTile);
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
        //只有玩家单位才打开UI，AI单位不应该打开UI
        if (playerID == 1)
        {
            OpenButtonList();
        }
        previousStandTile.UnitOnTile();
        ShowAttackRange(this.standOnTile);
        Action moveAction = new Action(ResetUnitPosition);
        gm.actions.Push(moveAction);
        gm.isAnimating = false;
        gm.cameraFollow.ResetCameraPosition();
    }
    public void OpenButtonList()
    {
        //只有玩家单位才能打开UI，AI单位不应该打开UI
        if (playerID != 1)
        {
            return;
        }
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
    public void CloseEquipmentList()
    {
        equipButtonList.CloseButtons();
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

        //行动值系统：结束回合后，继续处理下一个回合
        if (gm.currentActiveUnit == this)
        {
            gm.isProcessingTurn = false;
            gm.currentActiveUnit = null;
            //继续处理下一个回合
            gm.ProcessActionValueTurn();
        }
    }

    public void Idle()
    {
        //如果playerAnimator还没有初始化，先尝试获取
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }
        //如果playerAnimator存在，设置动画参数
        if (playerAnimator != null)
        {
            playerAnimator.SetAnimationParam(this, 0, 0);
        }
        this.hasMoved = false;
        this.hasAttacked = false;
        this.selected = false;
        this.stand = false;
    }

    //等待战斗完成后再调用Stand()
    private IEnumerator WaitForBattleCompleteAndStand(Unit unit)
    {
        //等待战斗场景加载完成（BattleHandler.Start()会被调用，设置isAttacking = true）
        //场景加载需要时间，所以先等待一段时间
        yield return new WaitForSeconds(3.5f); //等待场景加载和淡入动画完成（LoadBattleScene中有3秒等待）

        //等待战斗开始（isAttacking变成true）
        //设置超时时间，避免无限等待
        float waitTime = 0f;
        float maxWaitTime = 5f;
        while (!unit.isAttacking && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        //如果超时还没开始战斗，可能是战斗场景加载失败，直接返回
        if (!unit.isAttacking)
        {
            Debug.LogWarning($"单位 {unit.name} 等待战斗开始超时，可能战斗场景加载失败");
            yield break;
        }

        //等待战斗完成（isAttacking变成false）
        while (unit.isAttacking)
        {
            yield return null;
        }

        //战斗完成后，等待一小段时间确保战斗场景完全卸载
        yield return new WaitForSeconds(0.2f);

        //战斗完成后，调用Stand()来结束回合
        //但只有当前单位是currentActiveUnit时才调用，避免重复调用
        if (gm.currentActiveUnit == unit && !unit.stand)
        {
            unit.Stand();
        }
    }

    public void SwitchWeapon(Weapon weapon)
    {
        this.attackAbility = weapon.attackAbility;
        this.attackRange = weapon.range;
        this.attackType = (AttackType)Enum.Parse(typeof(AttackType), weapon.WeaponType);
        this.CurrentWeapon = weapon;
    }
    public void HoveredWeapon(Weapon weapon)
    {
        this.attackAbility = weapon.attackAbility;
        this.attackRange = weapon.range;
        this.attackType = (AttackType)Enum.Parse(typeof(AttackType), weapon.WeaponType);
        this.hoverdWeapon = weapon;
    }

    public void RecordAttackRangeTiles(Tile attackTile)
    {
        gm.attackRangeTiles.Clear();
        this.ShowAttackRange(this.standOnTile,true);
    }

    public bool CanCounterAttacl(Unit activeUnit)
    {
        if (this.attackRangetiles.Count > 0)
        {
            foreach (var tile in attackRangetiles)
            {
                if (tile.unitOnTile != null && tile.unitOnTile == activeUnit)
                {
                    return true;
                }
            }
            return false;

        }
        else {
            //没有发动过攻击，但是被攻击
            this.RecordAttackRangeTiles(standOnTile);
            foreach (var tile in attackRangetiles)
            {
                if (tile.unitOnTile != null && tile.unitOnTile == activeUnit)
                {
                    gm.ResetMoveableRange();
                    gm.ResetMovePath();
                    return true;
                }
            }
            gm.ResetMoveableRange();
            gm.ResetMovePath();
            return false;


        }
    }

    // damageUnit: 给予伤害的Unit
    public void DamageTaken(Unit damageUnit) {
        float damage = 0;
        if(this.defenseAbility > damageUnit.CurrentWeapon.attackAbility)
        {
            damage = 1;
        }
        else
        {
            damage = damageUnit.CurrentWeapon.attackAbility - this.defenseAbility;
        }

        this.health -= (int)damage;

    }

    // ========== 速度修改器系统（星穹铁道机制） ==========

    /// <summary>
    /// 获取速度修改器列表
    /// </summary>
    public List<SpeedModifier> GetSpeedModifiers()
    {
        return speedModifiers;
    }

    /// <summary>
    /// 添加速度修改器
    /// </summary>
    /// <param name="speedChange">速度变化值（正数为加速，负数为减速）</param>
    /// <param name="remainingRounds">剩余回合数（0=永久，-1=直到手动移除）</param>
    /// <param name="source">来源单位（可选）</param>
    /// <param name="description">描述（可选）</param>
    public void AddSpeedModifier(int speedChange, int remainingRounds, Unit source = null, string description = "")
    {
        speedModifiers.Add(new SpeedModifier(speedChange, remainingRounds, source, description));
    }

    /// <summary>
    /// 移除速度修改器
    /// </summary>
    /// <param name="modifier">要移除的修改器</param>
    public void RemoveSpeedModifier(SpeedModifier modifier)
    {
        if (speedModifiers.Contains(modifier))
        {
            speedModifiers.Remove(modifier);
        }
    }

    /// <summary>
    /// 获取当前有效速度（考虑所有速度修改器）
    /// </summary>
    public int GetEffectiveRoundSpeed()
    {
        int effectiveSpeed = roundspeed;

        foreach (var modifier in speedModifiers)
        {
            effectiveSpeed += modifier.speedChange;
        }

        // 确保速度至少为1（避免除零错误）
        return Mathf.Max(1, effectiveSpeed);
    }

    /// <summary>
    /// 获取指定回合的有效速度（用于预测系统）
    /// </summary>
    /// <param name="roundIndex">回合索引（0=当前回合，1=下一个回合，2=下下个回合...）</param>
    public int GetEffectiveRoundSpeedForRound(int roundIndex)
    {
        int effectiveSpeed = roundspeed;

        foreach (var modifier in speedModifiers)
        {
            // 永久修改器（remainingRounds = 0）始终生效
            if (modifier.remainingRounds == 0)
            {
                effectiveSpeed += modifier.speedChange;
            }
            // 临时修改器：检查是否在指定回合仍然有效
            else if (modifier.remainingRounds > 0)
            {
                // 如果剩余回合数 > roundIndex，说明在这个回合仍然有效
                if (modifier.remainingRounds > roundIndex)
                {
                    effectiveSpeed += modifier.speedChange;
                }
            }
            // remainingRounds = -1 表示直到手动移除，始终生效
            else if (modifier.remainingRounds == -1)
            {
                effectiveSpeed += modifier.speedChange;
            }
        }

        // 确保速度至少为1（避免除零错误）
        return Mathf.Max(1, effectiveSpeed);
    }

    /// <summary>
    /// 回合结束时调用，减少速度修改器的剩余回合数
    /// </summary>
    public void OnTurnEnd()
    {
        // 从后往前遍历，避免移除时索引问题
        for (int i = speedModifiers.Count - 1; i >= 0; i--)
        {
            var modifier = speedModifiers[i];

            // 永久修改器（remainingRounds = 0）或手动移除的（remainingRounds = -1）不减少
            if (modifier.remainingRounds > 0)
            {
                modifier.remainingRounds--;

                // 如果剩余回合数为0，移除该修改器
                if (modifier.remainingRounds == 0)
                {
                    speedModifiers.RemoveAt(i);
                }
            }
        }
    }
}
