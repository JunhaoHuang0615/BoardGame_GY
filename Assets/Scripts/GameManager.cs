
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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

    //行动值系统
    public const float roundDistance = 10000f; //行动条总长度，类似星穹铁道的10000
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

    //初始化行动值系统
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
            //初始行动值设为0，让它们从同一起跑线开始累积
            unit.currentActionValue = 0f;
            //调用Idle()方法，设置单位进入idle状态（stand = false）
            unit.Idle();
        }
    }

    //处理行动值回合（基于队列系统，单线程管理）
    public void ProcessActionValueTurn()
    {
        //如果正在处理回合，或者已经有单位在行动，直接返回，不累积也不选择
        if (isProcessingTurn || currentActiveUnit != null)
        {
            return;
        }

        //累积所有单位的行动值（每帧累积一次，基于速度）
        float deltaTime = Time.deltaTime;
        float speedMultiplier = 200f; //速度倍数，用于调整累积速度

        foreach (Unit unit in allUnits)
        {
            //累积行动值的条件：
            //1. 单位活着
            //2. 不是当前正在行动的单位（currentActiveUnit）
            //3. 单位已完成上一回合（stand = true）可以累积
            //4. 或者单位还没有达到行动值（currentActionValue < roundDistance）可以累积
            if (unit != null && unit.health > 0 && unit != currentActiveUnit)
            {
                if (unit.stand || unit.currentActionValue < roundDistance)
                {
                    unit.currentActionValue += unit.roundspeed * deltaTime * speedMultiplier;
                }
            }
        }

        //更新行动队列：将达到阈值的单位加入队列
        UpdateActionQueue();

        //如果队列不为空，处理队列头部的单位（每次只处理一个）
        //关键：在检查队列之前，确保没有单位在行动
        if (actionQueue.Count > 0)
        {
            //再次检查，确保没有单位在行动（双重保险）
            if (currentActiveUnit != null || isProcessingTurn)
            {
                return;
            }

            //队列按行动值从大到小排序，取第一个
            Unit nextUnit = actionQueue[0];
            actionQueue.RemoveAt(0); //从队列移除

            //设置标志，开始处理单位行动
            isProcessingTurn = true;
            currentActiveUnit = nextUnit;

            //直接处理单位行动（不使用协程）
            HandleUnitTurn(nextUnit);
        }
    }

    //更新行动队列：将达到阈值的单位加入队列（如果还没在队列中）
    private void UpdateActionQueue()
    {
        foreach (Unit unit in allUnits)
        {
            //检查条件：
            //1. 单位活着
            //2. 不是当前正在行动的单位（currentActiveUnit）
            //3. 行动值达到要求（currentActionValue >= roundDistance）
            //4. 还没在队列中
            if (unit != null && unit.health > 0 &&
                unit != currentActiveUnit &&
                unit.currentActionValue >= roundDistance &&
                !actionQueue.Contains(unit))
            {
                actionQueue.Add(unit);
            }
        }

        //按行动值从大到小排序（行动值大的先行动）
        actionQueue.Sort((a, b) => b.currentActionValue.CompareTo(a.currentActionValue));
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

        //其他单位按"下一个要行动的顺序"排序（从小到大，即下一个要行动的排在前面）
        //排序规则：
        //1. 已经达到行动值的单位，按行动值从大到小排序（行动值大的先行动）
        //2. 还没达到行动值的单位，按"谁先达到行动值"排序（先达到的排在前面）
        sortedUnits.Sort((a, b) =>
        {
            bool aReady = a.currentActionValue >= roundDistance;
            bool bReady = b.currentActionValue >= roundDistance;

            if (aReady && bReady)
            {
                //都达到了，按行动值从大到小排序（行动值大的先行动）
                //这与UpdateActionQueue的排序逻辑一致
                return b.currentActionValue.CompareTo(a.currentActionValue);
            }
            else if (aReady && !bReady)
            {
                //a达到了，b没达到，a排在前面（已经达到的优先）
                return -1;
            }
            else if (!aReady && bReady)
            {
                //a没达到，b达到了，b排在前面
                return 1;
            }
            else
            {
                //都没达到，按"谁先达到行动值"排序
                //计算还需要的行动值
                float aRemaining = roundDistance - a.currentActionValue;
                float bRemaining = roundDistance - b.currentActionValue;

                //按"还需要多少时间"排序：剩余行动值 / 速度
                //时间越短，越先达到，应该排在前面
                float aTimeNeeded = aRemaining / a.roundspeed;
                float bTimeNeeded = bRemaining / b.roundspeed;

                int timeCompare = aTimeNeeded.CompareTo(bTimeNeeded);
                if (timeCompare != 0)
                {
                    return timeCompare; //时间短的排在前面
                }

                //如果时间相同，按速度从大到小排序（速度快的优先）
                return b.roundspeed.CompareTo(a.roundspeed);
            }
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

    //获取单位的行动值百分比（0-1）
    public float GetUnitActionValuePercent(Unit unit)
    {
        if (unit == null) return 0f;
        return Mathf.Clamp01(unit.currentActionValue / roundDistance);
    }

    //处理单个单位的回合（不使用协程，单线程处理）
    private void HandleUnitTurn(Unit unit)
    {
        //减少行动值（行动后扣除roundDistance）
        unit.currentActionValue -= roundDistance;

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
            actionOrderUI = existingUI;
            Debug.Log("ActionOrderUI 已存在，使用现有实例");
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

        //创建ActionOrderUI对象
        GameObject uiObj = new GameObject("ActionOrderUI");
        uiObj.transform.SetParent(canvas.transform, false);

        RectTransform uiRect = uiObj.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0, 0);
        uiRect.anchorMax = new Vector2(0, 1);
        uiRect.pivot = new Vector2(0, 1);
        uiRect.anchoredPosition = Vector2.zero;
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
