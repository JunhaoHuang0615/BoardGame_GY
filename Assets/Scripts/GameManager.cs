
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public List<Unit> deadList;
    public bool isAnimating; //动画进行中

    public Stack<Action> actions;
    public CameraFollow cameraFollow;
    // Start is called before the first frame update
    public Tile upEdgeTile;
    public Tile downEdgeTile;
    public Tile leftEdgeTile;
    public Tile rightEdgeTile;
    public LayerMask tileLayer;

    //控制回合制系统
    public int nowPlayerID; //当前回合可操控的棋子
    public int nextTurnPlayerID; //下一回合可操控的棋子ID

    //战斗系统相关
    public Unit activeUnit;
    public Unit passiveUnit;
    public bool passiveUnitCanCounterAttack; //决定了是否能够反击

    public bool animationWaitting;
    public bool isPrepareing; //是否处于战斗准备阶段

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
        deadList = new List<Unit>();
        allUnits = new List<Unit>();
        PrePareGame();
        //GameStart();

    }
    //取消选择的时候执行
    public void PrePareGame()
    {
        allUnits.Clear();
        isPrepareing = true;
        Tile[] tempTiles = FindObjectsOfType<Tile>();
        Unit[] tempUnits = FindObjectsOfType<Unit>();
        foreach (var unit in tempUnits)
        {
            allUnits.Add(unit);
        }
        foreach (var tile in tempTiles)
        {
            tiles.Add(tile);
        }
        GetEdgeTile();
    }

    public void GameStart()
    {
        isPrepareing = false;
        isAnimating = false;
        actions.Clear();
        nowPlayerID = 1;
        nextTurnPlayerID = 2;
    }

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

        if(nowPlayerID == 2)
        {
            StartCoroutine(AITurn());
        }
    }

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
            while (animationWaitting)
            {
                yield return null;
            }
            if (ai.isDead)
            {
                continue;
            }
            this.selectedUnit = ai;
            ai.selected = true;
            while(ai.GetComponent<FSM>().GetCurrentState() != StateType.STAND)
            {
                yield return null;
            }
        }
        TurnEnd();
    }

    public void GetEdgeTile()
    {
        EnableAllUnitCollider(false);
        var raycastHits = Physics2D.RaycastAll(new Vector2(0,0),Vector2.up, Mathf.Infinity, tileLayer);
        if(raycastHits.Length > 0)
        {
            upEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>() ;
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.down, Mathf.Infinity, tileLayer);
        if (raycastHits.Length > 0)
        {
            downEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>();
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.left, Mathf.Infinity, tileLayer);
        if (raycastHits.Length > 0)
        {
            leftEdgeTile = raycastHits[raycastHits.Length - 1].collider.GetComponent<Tile>();
        }
        raycastHits = Physics2D.RaycastAll(new Vector2(0, 0), Vector2.right, Mathf.Infinity, tileLayer);
        
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
        foreach (var unit in deadList)
        {
            ReturnUnitOnMap(unit);
        }
        deadList.Clear();
    }

    public bool CheckUnitAdjacent(Unit active, Unit passive)
    {
        //检测主动攻击的Unit下的Tile的neibours是否包含passive的tile
        //包含则相邻
        Tile active_tile = active.standOnTile;
        foreach (var tile in active_tile.neighbors)
        {
            if(tile == passive.standOnTile)
            {
                return true;
            }
        }
        return false;
        //不包含则远程

    }

    public void ReturnUnitOnMap(Unit unit)
    {
        if(unit.playerID == 1)
        {
            if (playerUnits.Contains(unit))
            {
                playerUnits.Remove(unit);
            }
        }

        if (allUnits.Contains(unit)) { 
            allUnits.Remove(unit);
        }
    }
    public void RebornUnitOnMap(Unit unit)
    {
        if (unit.playerID == 1)
        {
            if (!playerUnits.Contains(unit))
            {
                playerUnits.Add(unit);
            }
        }

        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
        }
    }

    public IEnumerator WaitAnimation(Animator animator,string aniName, int animationStateInfo = 0)
    {
        animationWaitting = true;
        while (!animator.GetCurrentAnimatorStateInfo(animationStateInfo).IsName(aniName))
        {
            yield return null;
        }
        //判断当前动画是否已经完成
        while (animator.GetCurrentAnimatorStateInfo(animationStateInfo).IsName(aniName))
        {
            yield return null; //卡在动画播放
        }
        animationWaitting = false;
    }
}
