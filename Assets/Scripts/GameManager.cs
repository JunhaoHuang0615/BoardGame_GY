
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Unit selectedUnit;
    public List<Unit> allUnits;
    public List<Tile> tiles;
    public List<Tile> moveableTiles; //存储可移动的Tile
    public List<Tile> attackRangeTiles; //存储处于可攻击范围的格子
    public List<Unit> playerUnits;
    public List<Unit> deadUnitList = new List<Unit>();
    public bool isAnimating; //动画进行中

    public Stack<Action> actions;
    public CameraFollow cameraFollow;
    // Start is called before the first frame update
    public Tile upEdgeTile;
    public Tile downEdgeTile;
    public Tile leftEdgeTile;
    public Tile rightEdgeTile;
    public LayerMask tileLayer;

    //控制回合制系统（旧系统，已注释，保留以防需要）
    //public int nowPlayerID; //当前回合可操控的棋子
    //public int nextTurnPlayerID; //下一回合可操控的棋子ID

    //行动值系统（星穹铁道机制）
    public const float baseActionValue = 10000f; //基础行动值（星穹铁道用10000）
    public const float roundDistance = 100f; //保留用于向后兼容
    public Unit currentActiveUnit; //当前可以行动的单位
    public bool isProcessingTurn = false; //是否正在处理回合

    //行动队列：按行动值排序，行动值大的在前（先行动）
    private List<Unit> actionQueue = new List<Unit>();

    public bool isDeadAnimationPlaying = false; //用于管理棋子的死亡动画是否结束

    //战斗系统相关
    public Unit activeUnit;
    public Unit passiveUnit;

    //AI相关
    public Unit aiTarget;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        isAnimating = false;
        actions = new Stack<Action>();
        tiles = new List<Tile>();
        attackRangeTiles = new List<Tile>();
        Tile[] tempTiles = FindObjectsOfType<Tile>();
        foreach(var tile in tempTiles)
        {
            tiles.Add(tile);
        }
        allUnits = new List<Unit>();
        Unit[] tempUnits = FindObjectsOfType<Unit>();
        foreach (var unit in tempUnits)
        {
            allUnits.Add(unit);
        }
        GetEdgeTile();
        //旧回合制系统初始化（已注释）
        //nowPlayerID = 1;
        //nextTurnPlayerID = 2;

        //初始化行动值系统
        InitializeActionValueSystem();

        EventManager.AddEventListener<Unit>("UnitReturn",OnUnitReturn);
        EventManager.AddEventListener("DeadAnimationPlaying",OnDeadAnimationPlaying);

        //创建行动顺序UI
        StartCoroutine(CreateActionOrderUICoroutine());
    }

    public void OnDeadAnimationPlaying()
    {
        isDeadAnimationPlaying = true;
    }

    public IEnumerator WaitAnimation(Animator animator, String animName, Action onFinished = null, int animationStateInfo = 0)
    {
        animator.Play(animName);
        while (!animator.GetCurrentAnimatorStateInfo(animationStateInfo).IsName(animName))
        {
            yield return null;
        }
        //判断当前动画是否已经完成
        while (animator.GetCurrentAnimatorStateInfo(animationStateInfo).IsName(animName))
        {
            yield return null; //卡在动画播放
        }
        onFinished?.Invoke();
    }
    //取消选择的时候执行
    public void ResetMoveableRange()
    {
        foreach(var tile in tiles)
        {
            tile.RestHightMovableTile();
        }
        moveableTiles.Clear();
        foreach(var unit in allUnits)
        {
            unit.RestHightUnitSprite();
        }
    }

    public void ResetMovePath()
    {
        foreach (var tile in tiles)
        {
            tile.ResetCost();
        }
    }
    public void EnablePlayerCollider(bool enable)
    {
        foreach(var player in playerUnits)
        {
            player.GetComponent<Collider2D>().enabled = enable;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimating == true)
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(actions.Count > 0)
            {
                Action action = actions.Pop();
                action();

            }
        }

        //旧回合制系统（已注释）
        //if(nowPlayerID == 2)
        //{
        //    StartCoroutine(AITurn());
        //}

        //行动值系统：检查是否有单位可以行动
        //如果正在播放死亡动画，等待动画完成后再处理（类似原来AITurn的逻辑）
        if (!isProcessingTurn && !isAnimating && !isDeadAnimationPlaying)
        {
            ProcessActionValueTurn();
        }
    }

    //初始化行动值系统（星穹铁道机制）
    void InitializeActionValueSystem()
    {
        //清空行动队列
        actionQueue.Clear();

        foreach (Unit unit in allUnits)
        {
            if (unit.roundspeed <= 0)
            {
                unit.roundspeed = 100; //默认速度
            }
            //星穹铁道机制：初始行动值 = 10000 ÷ 速度
            //速度越高，行动值越小，出手越快
            int effectiveSpeed = unit.GetEffectiveRoundSpeed();
            unit.currentActionValue = baseActionValue / effectiveSpeed;
            //调用Idle()方法，设置单位进入idle状态（stand = false）
            unit.Idle();
        }
    }

    //处理行动值回合（星穹铁道机制：所有单位行动值同步减少）
    public void ProcessActionValueTurn()
    {
        //如果正在处理回合，或者已经有单位在行动，直接返回
        if (isProcessingTurn || currentActiveUnit != null)
        {
            return;
        }

        //星穹铁道机制：所有单位行动值同步减少
        //找到行动值最小的单位（最快到达0的单位）
        float minActionValue = float.MaxValue;
        Unit fastestUnit = null;

        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.health > 0 && unit != currentActiveUnit)
            {
                if (unit.currentActionValue < minActionValue)
                {
                    minActionValue = unit.currentActionValue;
                    fastestUnit = unit;
                }
            }
        }

        if (fastestUnit == null)
        {
            return; // 没有可用单位
        }

        //计算减少量：基于最快单位的速度
        //行动值减少速度 = 速度（因为行动值 = 10000/速度，所以减少速度与速度成正比）
        float deltaTime = Time.deltaTime;
        float speedMultiplier = 2f; //速度倍数，用于调整时间推进速度
        float reductionAmount = fastestUnit.GetEffectiveRoundSpeed() * deltaTime * speedMultiplier;

        //所有单位行动值同步减少
        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.health > 0 && unit != currentActiveUnit)
            {
                unit.currentActionValue -= reductionAmount;
            }
        }

        //更新行动队列：行动值 <= 0 的单位可以行动
        UpdateActionQueue();

        //如果队列不为空，处理队列头部的单位（每次只处理一个）
        if (actionQueue.Count > 0)
        {
            //再次检查，确保没有单位在行动（双重保险）
            if (currentActiveUnit != null || isProcessingTurn)
            {
                return;
            }

            //队列按行动值从小到大排序（行动值小的先行动，因为先到0）
            Unit nextUnit = actionQueue[0];
            actionQueue.RemoveAt(0); //从队列移除

            //设置标志，开始处理单位行动
            isProcessingTurn = true;
            currentActiveUnit = nextUnit;

            //直接处理单位行动（不使用协程）
            HandleUnitTurn(nextUnit);
        }
    }

    //更新行动队列：行动值 <= 0 的单位加入队列（星穹铁道机制）
    private void UpdateActionQueue()
    {
        foreach (Unit unit in allUnits)
        {
            //检查条件：
            //1. 单位活着
            //2. 不是当前正在行动的单位（currentActiveUnit）
            //3. 行动值 <= 0（星穹铁道机制：行动值先归零者先行动）
            //4. 还没在队列中
            if (unit != null && unit.health > 0 &&
                unit != currentActiveUnit &&
                unit.currentActionValue <= 0f &&
                !actionQueue.Contains(unit))
            {
                actionQueue.Add(unit);
            }
        }

        //按行动值从小到大排序（行动值小的先行动，因为先到0）
        //同行动值判定：可以添加我方优先等逻辑
        actionQueue.Sort((a, b) =>
        {
            int valueCompare = a.currentActionValue.CompareTo(b.currentActionValue);
            if (valueCompare != 0)
            {
                return valueCompare;
            }
            // 同行动值时，可以按其他规则排序（如我方优先、队伍顺序等）
            // 这里暂时按速度排序（速度快的优先）
            return b.GetEffectiveRoundSpeed().CompareTo(a.GetEffectiveRoundSpeed());
        });
    }

    //获取行动队列（用于调试和显示）
    public List<Unit> GetActionQueue()
    {
        return new List<Unit>(actionQueue);
    }

    //获取所有单位的行动信息（用于UI显示）
    //返回按"下一个要行动的顺序"排序的单位列表
    //排序规则：从左到右排列
    // - 最左边：第三个要行动的（或更后面的）
    // - 中间：下一个要行动的
    // - 最右边：当前正在行动的单位
    public List<Unit> GetAllUnitsSortedByActionValue()
    {
        List<Unit> sortedUnits = new List<Unit>();
        Unit currentUnit = null;

        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.health > 0)
            {
                if (unit == currentActiveUnit)
                {
                    //当前行动的单位单独保存
                    currentUnit = unit;
                }
                else
                {
                    sortedUnits.Add(unit);
                }
            }
        }

        //其他单位按"下一个要行动的顺序"排序（星穹铁道机制）
        //排序规则：按行动值从小到大排序（行动值小的先行动，因为先到0）
        sortedUnits.Sort((a, b) =>
        {
            //星穹铁道机制：行动值小的先行动
            int valueCompare = a.currentActionValue.CompareTo(b.currentActionValue);
            if (valueCompare != 0)
            {
                return valueCompare;
            }

            //同行动值时，可以按其他规则排序（如我方优先、队伍顺序等）
            //这里暂时按速度排序（速度快的优先）
            return b.GetEffectiveRoundSpeed().CompareTo(a.GetEffectiveRoundSpeed());
        });

        //现在sortedUnits的顺序是：下一个要行动的 -> 下下个要行动的 -> 下下下个要行动的 -> ...
        //对于纵向布局（从上到下）：
        // - 最上面应该是当前行动的
        // - 然后是下一个要行动的
        // - 最下面是最后行动的
        //所以不需要反转，直接按这个顺序，然后把当前单位放在最前面

        //当前行动的单位放在最上面（列表开头，因为VerticalLayoutGroup从上到下排列）
        if (currentUnit != null)
        {
            sortedUnits.Insert(0, currentUnit);
        }

        //最终顺序：当前要行动的 -> 下一个要行动的 -> 下下个要行动的 -> ... -> 最后要行动的
        return sortedUnits;
    }

    //获取单位的行动值百分比（星穹铁道机制：用于UI显示）
    public float GetUnitActionValuePercent(Unit unit)
    {
        if (unit == null) return 0f;
        //星穹铁道机制：行动值 = 10000 ÷ 速度
        //UI显示：当前行动值 / 单次行动所需行动值
        int effectiveSpeed = unit.GetEffectiveRoundSpeed();
        float singleActionValue = baseActionValue / effectiveSpeed;
        //返回百分比（1.0表示刚好可以行动，小于1.0表示还需要等待）
        return unit.currentActionValue / singleActionValue;
    }

    // ========== 预测系统（星穹铁道机制） ==========

    /// <summary>
    /// 预测未来行动的数据结构
    /// </summary>
    [Serializable]
    public class FutureAction
    {
        public Unit unit;                    // 单位
        public int roundIndex;               // 回合索引（0=当前回合，1=下一个回合，2=下下个回合）
        public float predictedTime;          // 预计行动时间（从当前时刻开始）
        public float predictedActionValue;   // 预计行动值
        public int predictedSpeed;           // 预计速度（考虑buff/debuff）
    }

    /// <summary>
    /// 获取未来行动顺序预测（星穹铁道机制：模拟时间推进）
    /// </summary>
    /// <param name="maxActions">最大预测行动数（用于覆盖多个大回合）</param>
    /// <returns>未来行动列表，按时间排序</returns>
    public List<FutureAction> GetFutureActions(int maxActions = 20)
    {
        List<FutureAction> futureActions = new List<FutureAction>();

        // 速度倍数（与ProcessActionValueTurn中的一致）
        float speedMultiplier = 2f;

        // 复制当前状态用于模拟
        Dictionary<Unit, float> simulatedActionValues = new Dictionary<Unit, float>();
        Dictionary<Unit, int> unitRoundCount = new Dictionary<Unit, int>(); // 记录每个单位已行动次数

        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.health > 0)
            {
                simulatedActionValues[unit] = unit.currentActionValue;
                unitRoundCount[unit] = 0;
            }
        }

        float currentTime = 0f;
        int actionCount = 0;

        // 模拟时间推进，直到预测足够多的行动
        while (actionCount < maxActions && simulatedActionValues.Count > 0)
        {
            // 找到行动值最小的单位（最先到0）
            Unit nextUnit = null;
            float minActionValue = float.MaxValue;

            foreach (var kvp in simulatedActionValues)
            {
                if (kvp.Value < minActionValue)
                {
                    minActionValue = kvp.Value;
                    nextUnit = kvp.Key;
                }
            }

            if (nextUnit == null) break;

            // 星穹铁道机制：
            // 1. 找到行动值最小的单位（minActionValue），它即将行动
            // 2. 所有单位行动值同步减少 minActionValue（让最小值的单位到0）
            // 3. 该单位行动，所有单位再减去它的单次行动所需行动值

            int effectiveSpeed = nextUnit.GetEffectiveRoundSpeedForRound(unitRoundCount[nextUnit]);
            float singleActionValue = baseActionValue / effectiveSpeed; // 此次行动单位的单次行动所需行动值

            // 计算时间推进：时间 = minActionValue / (最快单位的速度 * 速度倍数)
            // 找到所有单位中速度最快的，用于计算时间推进
            int fastestSpeed = 0;
            foreach (var unit in simulatedActionValues.Keys)
            {
                int speed = unit.GetEffectiveRoundSpeedForRound(unitRoundCount[unit]);
                if (speed > fastestSpeed)
                {
                    fastestSpeed = speed;
                }
            }

            if (fastestSpeed == 0) break;

            // 时间推进 = minActionValue / (最快速度 * 速度倍数)
            // 因为所有单位行动值同步减少minActionValue，所以时间基于这个减少量
            float timeStep = minActionValue / (fastestSpeed * speedMultiplier);
            currentTime += timeStep;

            // 第一步：所有单位行动值减去 minActionValue（让最小值的单位到0）
            foreach (var unit in simulatedActionValues.Keys.ToList())
            {
                simulatedActionValues[unit] -= minActionValue;
            }

            // 第二步：该单位行动，所有单位再减去它的单次行动所需行动值
            float actionValueUsed = singleActionValue;
            foreach (var unit in simulatedActionValues.Keys.ToList())
            {
                simulatedActionValues[unit] -= actionValueUsed;
            }

            // 行动单位重新设置为下一次行动所需行动值
            int nextRoundSpeed = nextUnit.GetEffectiveRoundSpeedForRound(unitRoundCount[nextUnit]);
            simulatedActionValues[nextUnit] = baseActionValue / nextRoundSpeed;
            unitRoundCount[nextUnit]++;

            // 创建未来行动记录
            FutureAction futureAction = new FutureAction
            {
                unit = nextUnit,
                roundIndex = unitRoundCount[nextUnit] - 1, // 当前回合索引
                predictedTime = currentTime,
                predictedActionValue = 0f, // 行动后为0
                predictedSpeed = effectiveSpeed
            };

            futureActions.Add(futureAction);
            actionCount++;
        }

        // 按预计时间排序
        futureActions.Sort((a, b) => a.predictedTime.CompareTo(b.predictedTime));

        return futureActions;
    }

    /// <summary>
    /// 获取未来行动的UI显示列表（用于ActionOrderUI）
    /// </summary>
    /// <param name="maxDisplayCount">最大显示数量</param>
    /// <returns>未来行动列表</returns>
    public List<FutureAction> GetFutureActionsForUI(int maxDisplayCount = 20)
    {
        List<FutureAction> futureActions = GetFutureActions(maxDisplayCount);

        // 限制显示数量（避免UI过多）
        if (futureActions.Count > maxDisplayCount)
        {
            futureActions = futureActions.GetRange(0, maxDisplayCount);
        }

        return futureActions;
    }

    //处理单个单位的回合（星穹铁道机制）
    private void HandleUnitTurn(Unit unit)
    {
        //星穹铁道机制：所有单位的行动值减去此次行动单位的行动值
        //此次行动单位的单次行动所需行动值 = 10000 ÷ 速度
        int effectiveSpeed = unit.GetEffectiveRoundSpeed();
        float actionValueUsed = baseActionValue / effectiveSpeed;

        //所有单位的行动值减去此次行动单位的行动值
        foreach (Unit u in allUnits)
        {
            if (u != null && u.health > 0)
            {
                u.currentActionValue -= actionValueUsed;
            }
        }

        //行动单位重新设置为下一次行动所需行动值
        unit.currentActionValue = baseActionValue / effectiveSpeed;

        //单位行动结束时，减少速度修改器的剩余回合数（如果有）
        if (unit.GetSpeedModifiers().Count > 0)
        {
            unit.OnTurnEnd();
        }

        //重置单位状态，允许行动
        unit.hasMoved = false;
        unit.hasAttacked = false;
        //调用Idle()方法，设置单位进入idle状态（stand = false）
        unit.Idle();

        //如果是AI单位，触发AI开始行动
        if (unit.playerID == 2)
        {
            //确保AI的FSM在IDLE状态
            FSM aiFSM = unit.GetComponent<FSM>();
            if (aiFSM == null)
            {
                Debug.LogError($"AI单位 {unit.name} 没有FSM组件！");
                isProcessingTurn = false;
                currentActiveUnit = null;
                //继续处理下一个单位
                ProcessActionValueTurn();
                return;
            }

            if (aiFSM.GetCurrentState() != StateType.IDLE)
            {
                aiFSM.TransitionToState(StateType.IDLE);
            }

            //GM选择AI单位，设置selected = true，触发AI从IDLE转换到TARGETTING
            selectedUnit = unit;
            unit.selected = true;

            //AI会自动完成行动，完成后会调用Stand()方法，Stand()中会继续处理下一个单位
        }
        //如果是玩家单位，自动选中并打开UI
        else
        {
            //自动选中玩家单位并打开UI
            ResetMoveableRange();
            ResetMovePath();
            EnablePlayerCollider(true);
            unit.EnableGoodCollider(true);

            //取消之前选中的单位
            if (selectedUnit != null && selectedUnit != unit)
            {
                selectedUnit.CloseButtonList();
                selectedUnit.CloseEquipmentList();
                selectedUnit.selected = false;
                selectedUnit.playerAnimator.SetAnimationParam(selectedUnit, 0, 0);
            }

            //选中当前单位
            selectedUnit = unit;
            unit.selected = true;
            unit.OpenButtonList();

            //显示移动范围或攻击范围
            if (!unit.hasMoved)
            {
                unit.ShowMoveRangeTwo();
            }
            else if (!unit.hasAttacked)
            {
                unit.ShowAttackRange(unit.standOnTile);
            }

            unit.playerAnimator.SetAnimationParam(unit, 0, 0);

            //玩家单位由玩家手动操作
            //当玩家调用Stand()时，会继续处理下一个单位
        }
    }

    //旧回合制系统（已注释，保留以防需要）
    /*
    public IEnumerator AITurn()
    {
        List<Unit> aiList = new List<Unit>();
        foreach(var unit in allUnits)
        {
            if(unit.playerID == 2)
            {
                aiList.Add(unit);
            }
        }

        foreach(var ai in aiList)
        {
            while (isDeadAnimationPlaying)
            {
                yield return null;
            }
            this.selectedUnit = ai;
            ai.selected = true;
            //
            while(ai.GetComponent<FSM>().GetCurrentState() != StateType.STAND && ai.GetComponent<FSM>().GetCurrentState() != StateType.FINISH)
            {
                yield return null;
            }
        }
        TurnEnd();
    }
    */

    public void GetEdgeTile()
    {
        EnableAllUnitCollider(false);
        var raycastHits = Physics2D.RaycastAll(new Vector2(0,0),Vector2.up,tileLayer);
        if(raycastHits.Length > 0)
        {
            upEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>() ;
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.down, tileLayer);
        if (raycastHits.Length > 0)
        {
            downEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>();
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.left, tileLayer);
        if (raycastHits.Length > 0)
        {
            leftEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>();
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.right, tileLayer);
        if (raycastHits.Length > 0)
        {
            rightEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>();
        }
        EnableAllUnitCollider(true);


    }
    public void EnableAllUnitCollider(bool enable)
    {
        foreach(var unit in allUnits)
        {
            unit.GetComponent<Collider2D>().enabled = enable;
        }
    }

    //旧回合制系统方法（已注释，保留以防需要）
    /*
    public void TurnEnd()
    {
        //PlayerID交换
        int temp = nowPlayerID;
        nowPlayerID = nextTurnPlayerID;
        nextTurnPlayerID = temp;
        selectedUnit = null;
        moveableTiles.Clear();
        actions.Clear();
        foreach (var unit in playerUnits)
        {
            unit.RestUnitState();
        }
        foreach(var tile in tiles)
        {
            tile.RestHightMovableTile();
        }
        foreach(var unit in deadUnitList)
        {
            if (allUnits.Contains(unit))
            {
                allUnits.Remove(unit);
            }
            if (playerUnits.Contains(unit))
            {
                playerUnits.Remove(unit);
            }
            Destroy(unit.gameObject);
        }
        deadUnitList.Clear();
    }
    */

    public void OnUnitReturn(Unit deadUnit)
    {
        deadUnitList.Add(deadUnit);
        deadUnit.gameObject.SetActive(false);

        isDeadAnimationPlaying = false;

        //如果死亡的单位是当前正在行动的单位，继续处理下一个单位
        if (deadUnit == currentActiveUnit)
        {
            isProcessingTurn = false;
            currentActiveUnit = null;
            //继续处理下一个单位（会等待死亡动画完成，因为Update中检查了isDeadAnimationPlaying）
            ProcessActionValueTurn();
        }
    }

    //行动顺序UI相关
    private ActionOrderUI actionOrderUI;

    //创建行动顺序UI（使用协程确保在下一帧创建，此时所有单位已初始化）
    private IEnumerator CreateActionOrderUICoroutine()
    {
        yield return null; //等待一帧，确保所有单位都已初始化

        //先检查是否已经存在 ActionOrderUI
        ActionOrderUI existingUI = FindObjectOfType<ActionOrderUI>();
        if (existingUI != null)
        {
            //如果已存在，更新其位置到屏幕左侧
            RectTransform existingRect = existingUI.GetComponent<RectTransform>();
            if (existingRect != null)
            {
                existingRect.anchorMin = new Vector2(0, 0);
                existingRect.anchorMax = new Vector2(0, 1);
                existingRect.pivot = new Vector2(0, 1);
                existingRect.anchoredPosition = new Vector2(10, -10);
                existingRect.sizeDelta = new Vector2(220, 0);
                Debug.Log("ActionOrderUI 已存在，已更新位置到屏幕左侧");
            }
            actionOrderUI = existingUI;
            yield break;
        }

        //查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            //如果没有Canvas，创建一个
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("创建了新的Canvas");
        }
        else
        {
            //确保Canvas的RenderMode是ScreenSpaceOverlay，这样UI位置计算才正确
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log("已将Canvas的RenderMode设置为ScreenSpaceOverlay");
            }
        }

        //创建ActionOrderUI对象
        GameObject uiObj = new GameObject("ActionOrderUI");
        uiObj.transform.SetParent(canvas.transform, false);

        RectTransform uiRect = uiObj.AddComponent<RectTransform>();
        //设置锚点到左侧，从顶部到底部
        uiRect.anchorMin = new Vector2(0, 0);
        uiRect.anchorMax = new Vector2(0, 1);
        uiRect.pivot = new Vector2(0, 1);
        //设置位置：距离左边10像素，距离顶部-10像素（向下偏移10像素）
        uiRect.anchoredPosition = new Vector2(10, -10);
        //设置尺寸：宽度220，高度填满（0表示使用锚点）
        uiRect.sizeDelta = new Vector2(220, 0);

        //添加ActionOrderUI组件
        actionOrderUI = uiObj.AddComponent<ActionOrderUI>();

        //创建内容容器
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(uiObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1);
        contentRect.anchorMax = new Vector2(0f, 1);
        contentRect.pivot = new Vector2(0f, 1);
        contentRect.anchoredPosition = new Vector2(10, -10);
        contentRect.sizeDelta = new Vector2(200, 2000);

        //添加垂直布局组件
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;

        //设置ActionOrderUI的contentParent
        actionOrderUI.SetContentParent(contentObj.transform);

        Debug.Log($"行动顺序UI已创建！单位数量: {allUnits.Count}");
    }
}
