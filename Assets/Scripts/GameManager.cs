
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
        nowPlayerID = 1;
        nextTurnPlayerID = 2;
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
            this.selectedUnit = ai;
            ai.selected = true;
            //
            while(ai.hasMoved == false)
            {
                yield return null;
            }
        }
        TurnEnd();
    }

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
    }
}
