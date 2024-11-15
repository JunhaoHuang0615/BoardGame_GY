using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileType
{
    Plant,
    Mountain,
    Water,
    Wall,

}
public class Tile : MonoBehaviour
{
    // public Sprite[] sprites;
    public TileDataManager tileDataManager;
    public PlatformType platformType;
    private SpriteRenderer render;
    public TileType tileType;
    private PathFinding pm;

    public LayerMask layerMask;
    private GameManager gm;
    private UIManager um;
    public int tileHeight; //格子的海拔高度，低于次高度的单位不可移动到此格子上
    public Unit unitOnTile;
    public bool isMoveableTile; //可被移动的格子，鼠标点击时用
    public Color moveableHightColor;
    public Color attackableHightColor;
    public Color enterTileColor;

    //A星算法参数
    public int gcost;
    public int hcost;
    public int fcost;
    public Tile parentTile; //记录上一个点
    public bool needToBeReset = true;

    //深度算法属性
    public int needAcrtionPoint; //所需行动点数

    //箭头显示
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private GameObject corner;
    [SerializeField]
    private GameObject line;
    public List<Tile> neighbors;




    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        pm = FindObjectOfType<PathFinding>();
        um = FindObjectOfType<UIManager>();
        tileDataManager = FindObjectOfType<TileDataManager>();
        render = GetComponent<SpriteRenderer>();
        int r = Random.Range(0, tileDataManager.tileSprites.Count);
        platformType = tileDataManager.platforms[r];
        render.sprite = tileDataManager.tileSprites[r];

        UnitOnTile();
        //让此方法晚一点点执行
        Invoke("GetNeighborTiles",0.1f);
    }
    private void OnMouseDown()
    {
        if (gm.isAnimating == true)
        {
            return;
        }
        if (um.isOnButtonList())
        {
            return;
        }
        //判定是否点在了可移动格子的上面
        if (isMoveableTile)
        {
            SendUnitHere(gm.selectedUnit,pm.path);
        }
    }

    private void OnMouseEnter() 
    {   
        
        if(isMoveableTile == true)
        {
            //鼠标经过时候执行的脚本
            Tile endTile = pm.AStarPathFind(gm.selectedUnit.standOnTile, this);
            ShowArrow(endTile);
            pm.GetPath(endTile);
            return;
        }
        if (needToBeReset == true) { 
            HighlightEnterTile();
        }


    }
    private void OnMouseExit()
    {
        if (this.needToBeReset)
        {  
            ResetEnterTile();
        }
    }
    public void UnitOnTile()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, layerMask);
        if(collider != null)
        {
            if (collider.CompareTag("Mountain"))
            {
                tileType = TileType.Mountain;
                tileHeight = 2;
            }
            //每一个Tile上的单位要知道自己站在哪一个格子上
            collider.GetComponent<Unit>().standOnTile = this;
            unitOnTile = collider.GetComponent<Unit>();
            if (collider.CompareTag("Player"))
            {
                print("Player比较");
                gm.playerUnits.Add(collider.GetComponent<Unit>());
            }

        }
        else
        {
            unitOnTile = null;
        }
        
    }
    //返回此Tile相邻的四个Tile
    public List<Tile> GetNeighborTiles()
    {   
        //让阻碍物的碰撞器暂时失效
        foreach (var unit in gm.allUnits)
        {
            unit.GetComponent<Collider2D>().enabled = false;
        }
        List<Tile> neighbors = new List<Tile>();
        //发一个射线，碰撞上面的Tile
        Vector2 raypoint = new Vector2(transform.position.x, transform.position.y);
        //相邻位置上方的Tile
        RaycastHit2D hit = Physics2D.Raycast(raypoint + Vector2.up, Vector2.up);
        if(hit !=null && hit.collider!=null && hit.collider.CompareTag("Tile"))
        {
            neighbors.Add(hit.collider.GetComponent<Tile>());
        }
        hit = Physics2D.Raycast(raypoint + Vector2.right, Vector2.right);
        if (hit != null && hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            neighbors.Add(hit.collider.GetComponent<Tile>());
        }
        hit = Physics2D.Raycast(raypoint + Vector2.down, Vector2.down);
        if (hit != null && hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            neighbors.Add(hit.collider.GetComponent<Tile>());
        }
        hit = Physics2D.Raycast(raypoint + Vector2.left, Vector2.left);
        if (hit != null && hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            neighbors.Add(hit.collider.GetComponent<Tile>());
        }
        gm.EnablePlayerCollider(true);
        this.neighbors = neighbors;
        return neighbors;

    }

    public void HightMoveableTile()
    {
        needToBeReset = false;
        render.color = moveableHightColor;
        isMoveableTile = true;
    }
    public void HightAttackableTile()
    {
        needToBeReset = false;
        render.color = attackableHightColor;
    }
    public void HighlightEnterTile()
    {
        render.color = enterTileColor;
    }
    public void ResetEnterTile()
    {
        render.color = Color.white;
    }
    public void RestHightMovableTile()
    {   
        render.color = Color.white;
        isMoveableTile = false;
        needToBeReset = true;
    }
    public int GetNeedMoveAbility(Unit selectedUnit)
    {   
        //如果是弓兵
        if(selectedUnit.type == CharacterType.Archer)
        {
            return 1;
        }
        //如果是战士
        else
        {   
            //如果战士要通过水
            if(this.tileType == TileType.Water)
            {
                return 2;
            }
            //如果战士要通过山
            else if (this.tileType == TileType.Mountain)
            {
                return 99;
            }
            //如果战士要通过平原
            else
            {
                return 1;
            }
        }
    }

    public void CalculateFCost()
    {
        fcost = gcost + hcost;
    }

    public void ResetCost()
    {
        gcost = int.MaxValue;
        hcost = int.MaxValue;
        parentTile = null;
        ResetArrow();


    }

    public void ShowArrow(Tile endTile)
    {
        if (endTile.parentTile == null)
        {
            return;
        }

        Tile nextTile = endTile.parentTile;

        //先判断这个点是否是从左边或者右边来的
        if (endTile.transform.position.y == nextTile.transform.position.y)
        {
            if (endTile.transform.position.x > nextTile.transform.position.x)
            {
                //说明从左边来的
                endTile.arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                endTile.arrow.SetActive(true);
            }
            else
            {
                endTile.arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                endTile.arrow.SetActive(true);
            }
        }
        if (endTile.transform.position.x == nextTile.transform.position.x)
        {
            if (endTile.transform.position.y > nextTile.transform.position.y)
            {
                //说明从下边来的
                endTile.arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                endTile.arrow.SetActive(true);
            }
            else
            {
                //从上面来的
                endTile.arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                endTile.arrow.SetActive(true);
            }
        }


        //当前判定格子的前一个格子nexttile

        //判定格子
        Tile now = nextTile;
        nextTile = endTile;
        Tile lastTile = now.parentTile;
        while (now != gm.selectedUnit.standOnTile)
        {
            //是否是直线的情况
            if (nextTile.transform.position.y == lastTile.transform.position.y || nextTile.transform.position.x == lastTile.transform.position.x)
            {
                if (nextTile.transform.position.y == lastTile.transform.position.y)
                {
                    //左右直线
                    now.line.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    now.line.SetActive(true);
                }
                else
                {
                    //上下直线
                    now.line.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                    now.line.SetActive(true);
                }
            }
            //是拐角的情况
            if (nextTile.transform.position.y != lastTile.transform.position.y && nextTile.transform.position.x != lastTile.transform.position.x)
            {
                //判断拐角的4类
                if (nextTile.transform.position.y > lastTile.transform.position.y && nextTile.transform.position.x > lastTile.transform.position.x)
                {
                    if (now.transform.position.x == nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
                        now.corner.SetActive(true);
                    }
                    if (now.transform.position.x != nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        now.corner.SetActive(true);
                    }
                }
                else if (nextTile.transform.position.y < lastTile.transform.position.y && nextTile.transform.position.x < lastTile.transform.position.x)
                {
                    if (now.transform.position.x == nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        now.corner.SetActive(true);
                    }
                    if (now.transform.position.x != nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -270));
                        now.corner.SetActive(true);
                    }
                }
                else if (nextTile.transform.position.y < lastTile.transform.position.y && nextTile.transform.position.x > lastTile.transform.position.x)
                {
                    if (now.transform.position.x == nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                        now.corner.SetActive(true);
                    }
                    if (now.transform.position.x != nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        now.corner.SetActive(true);
                    }

                }
                else if (nextTile.transform.position.y > lastTile.transform.position.y && nextTile.transform.position.x < lastTile.transform.position.x)
                {
                    if (now.transform.position.x == nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        now.corner.SetActive(true);
                    }
                    if (now.transform.position.x != nextTile.transform.position.x)
                    {
                        now.corner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                        now.corner.SetActive(true);
                    }
                }

            }//判断是否是拐角

            //一次while循环结束时候要处理的代码
            nextTile = nextTile.parentTile;
            now = now.parentTile;
            lastTile = lastTile.parentTile;
        }
    }

    public void ResetArrow()
    {
        arrow.SetActive(false);
        line.SetActive(false);
        corner.SetActive(false);
    }
    public void SendUnitHere(Unit unit, List<Tile> path)
    {
        if (unit.canExcute)
        {
            unit.Move(path);
            //之前角色所占格子记录下来
            unit.standOnTile.unitOnTile = null;
            unit.standOnTile = this;
            this.unitOnTile = unit;
            gm.ResetMoveableRange();
            gm.ResetMovePath();
            //gm.selectedUnit.selected = false;
            //gm.selectedUnit = null;
        }

    }
    //上：Vector2.up 下：Vector2.down
    public Tile GetNeighbourInDirection(Vector2 direction)
    {
        // 一時的に障害物のコライダーを無効にする
        foreach (var unit in gm.allUnits)
        {
            unit.GetComponent<Collider2D>().enabled = false;
        }

        // 指定された方向にRaycastを飛ばす
/*        Vector2 raypoint = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(raypoint, direction);*/
        Vector2 raypoint = new Vector2(transform.position.x, transform.position.y);
        //相邻位置上方的Tile
        RaycastHit2D hit = Physics2D.Raycast(raypoint + direction, direction);

        // コライダーがTileである場合、隣接タイルとして取得
        if (hit.collider != null && hit.collider.CompareTag("Tile"))
        {
            gm.EnablePlayerCollider(true);
            Tile tile = hit.collider.GetComponent<Tile>();
            return tile;
        }

        // 見つからなかった場合は null を返す
        gm.EnablePlayerCollider(true);
        return null;
    }




}
