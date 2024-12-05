
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class GameManager : MonoBehaviour
{   
    public static GameManager Instance { get; private set; }

    public string level_name;

    public Unit selectedUnit;
    public List<Unit> allUnits;
    public List<Tile> tiles;
    public List<Tile> moveableTiles; //存储可移动的Tile
    public List<Tile> attackRangeTiles; //存储处于可攻击范围的格子
    public List<Unit> playerUnits; //AI Unit + 玩家Unit
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
    private ObjectPool obp;
    private DataManager dataManager;

    //AI相关
    public Unit aiTarget;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        obp = FindObjectOfType<ObjectPool>();
        dataManager = FindObjectOfType<DataManager>();
        isAnimating = false;
        actions = new Stack<Action>();
        tiles = new List<Tile>();
        attackRangeTiles = new List<Tile>();
        deadList = new List<Unit>();
        allUnits = new List<Unit>();
        playerUnits = new List<Unit>();
        PrePareGame();
        //GameStart();

    }
    //取消选择的时候执行
    public void PrePareGame()
    {   

        allUnits.Clear();
        isPrepareing = true;
        Tile[] tempTiles = FindObjectsOfType<Tile>();
        foreach (var tile in tempTiles)
        {
            tiles.Add(tile);
        }
        LoadLevelData();
        Unit[] tempUnits = FindObjectsOfType<Unit>();
        foreach (var unit in tempUnits)
        {
            allUnits.Add(unit);
        }
        GetEdgeTile();
    }

    public void LoadLevelData()
    {
        //LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Levels/Level1.asset"); 此方法只可以在编辑器模式中使用
        LevelData levelData = Resources.Load<LevelData>($"Levels/{level_name}");
        DeployPieces(levelData);
    }

    public void DeployPieces(LevelData levelData)
    {
        foreach (var item in levelData.pieces)
        {   
            //TODO: 1. 添加FSM组件  2. AI的武器
            Tile tile = GetTileByWorldPosition(item.position);
            Vector3 tilePosition = tile.transform.position;
            //TODO: 应该放置的棋子，由用户通过UI选择后决定
            GameObject pawn = obp.GetGameObject(item.pawnType);
            pawn.transform.position = new Vector3(tilePosition.x, tilePosition.y, -1);
            //TODO: 应该放置的棋子的Data名字，由用户过UI选择后决定
            Unit placedUnit = pawn.GetComponent<Unit>();
            placedUnit.InitializeUnit(dataManager.GetChracterData(item.unitName, CSVResource.EnemyChracter), 2);
            placedUnit.gameObject.AddComponent<FSM>();
            tile.unitOnTile = placedUnit;
            placedUnit.standOnTile = tile;
            this.allUnits.Add(placedUnit);
            this.playerUnits.Add(placedUnit);
        }
    }
    public void GameStart()
    {
        isAITurnRuning = false;
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

    bool isAITurnRuning;
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
            if(isAITurnRuning == false)
            {
                StartCoroutine(AITurn());
            }
        }
    }

    public IEnumerator AITurn()
    {
        isAITurnRuning = true;
        List<Unit> aiList = new List<Unit>();
        foreach(var unit in playerUnits)
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

    public Tile GetTileByWorldPosition(Vector3 position)
    {
        // 一時的に障害物のコライダーを無効にする
        foreach (var unit in this.allUnits)
        {
            unit.GetComponent<Collider2D>().enabled = false;
        }

        // 指定された方向にRaycastを飛ばす
        /*        Vector2 raypoint = new Vector2(transform.position.x, transform.position.y);
                RaycastHit2D hit = Physics2D.Raycast(raypoint, direction);*/
        Vector2 raypoint = new Vector2(position.x,position.y);
        //相邻位置上方的Tile
        LayerMask tileLayerMask = LayerMask.GetMask("TileLayer");

        var hit = Physics2D.OverlapCircle(position, 0.2f, tileLayerMask);

        // コライダーがTileである場合、隣接タイルとして取得
        if (hit != null && hit.CompareTag("Tile"))
        {
            EnablePlayerCollider(true);
            Tile tile = hit.GetComponent<Tile>();
            return tile;
        }

        // 見つからなかった場合は null を返す
        EnablePlayerCollider(true);
        return null;
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
        isAITurnRuning = false;
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
        if (playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);
        }

        if (allUnits.Contains(unit)) { 
            allUnits.Remove(unit);
        }
    }
    public void RebornUnitOnMap(Unit unit)
    {
        if (!playerUnits.Contains(unit))
        {
            playerUnits.Add(unit);
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
