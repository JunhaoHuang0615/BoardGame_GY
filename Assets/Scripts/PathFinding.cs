/***
 *
 * Title:"" 项目：AAA
 * 主题：
 * Description:
 * 功能：
 *
 * Date:2021/
 * Version:0.1v
 * Coder:Junhao Huang
 * email:huangjunhao0615@gmail.com
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private GameManager gm;
    public List<Tile> path;
    public static PathFinding Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        path = new List<Tile>();
    }

    public Tile AStarPathFind(Tile startTile, Tile targetTile)
    {
        ResetPath();
        List<Tile> openList = new List<Tile>();
        List<Tile> closeList = new List<Tile>();

        //将起始点放入OpenList
        openList.Add(startTile);
        //将所有的Tile初始化
        foreach (var tile in gm.tiles) 
        {
            tile.ResetCost();
        }
        //首先初始化起始点的Cost
        startTile.gcost = 0;
        startTile.hcost = CalculateDistanceCost(startTile,targetTile);

        while(openList.Count > 0)
        {
            //说明有监测点,将openList里面fcost最小的点拿出来进行评估
            Tile tile = GetLowestFCostTile(openList);
            //判断从openList内拿出的fcost最小的Tile是否是终点
            if(tile == targetTile)
            {
                return tile;
            }
            //如果还在寻路中
            //1.将监测点移出openList，加入closeList
            openList.Remove(tile);
            closeList.Add(tile);
            //2.获取邻边的Tile，并且计算gcost和hcost，fcost
            List<Tile> neighbors = tile.neighbors;
            foreach (var neighbor in neighbors)
            {
                if(closeList.Contains(neighbor) || openList.Contains(neighbor))
                {
                    continue;
                }
                //针对不可以动的点
                if(neighbor.isMoveableTile == false)
                {
                    closeList.Add(neighbor);
                    continue;
                }
                neighbor.gcost = CalculateDistanceCost(neighbor, startTile);
                neighbor.hcost = CalculateDistanceCost(neighbor, targetTile);
                neighbor.CalculateFCost();
                neighbor.parentTile = tile;
                //将此neighbor加入到OpenList中
                openList.Add(neighbor);
            }
           
        }
        print("路径空");
        //找不到路径
        return null;
    }
    public Tile AStarPathFindDistance(Tile startTile, Tile targetTile)
    {
        ResetPath();
        List<Tile> openList = new List<Tile>();
        List<Tile> closeList = new List<Tile>();

        //将起始点放入OpenList
        openList.Add(startTile);
        //将所有的Tile初始化
        foreach (var tile in gm.tiles)
        {
            tile.ResetCost();
        }
        //首先初始化起始点的Cost
        startTile.gcost = 0;
        startTile.hcost = CalculateDistanceCost(startTile, targetTile);

        while (openList.Count > 0)
        {
            //说明有监测点,将openList里面fcost最小的点拿出来进行评估
            Tile tile = GetLowestFCostTile(openList);
            //判断从openList内拿出的fcost最小的Tile是否是终点
            if (tile == targetTile)
            {
                return tile;
            }
            //如果还在寻路中
            //1.将监测点移出openList，加入closeList
            openList.Remove(tile);
            closeList.Add(tile);
            //2.获取邻边的Tile，并且计算gcost和hcost，fcost
            List<Tile> neighbors = tile.neighbors;
            foreach (var neighbor in neighbors)
            {
                if (closeList.Contains(neighbor) || openList.Contains(neighbor))
                {
                    continue;
                }
                neighbor.gcost = CalculateDistanceCost(neighbor, startTile);
                neighbor.hcost = CalculateDistanceCost(neighbor, targetTile);
                neighbor.CalculateFCost();
                neighbor.parentTile = tile;
                //将此neighbor加入到OpenList中
                openList.Add(neighbor);
            }

        }
        print("路径空");
        //找不到路径
        return null;
    }
    //计算gcost： 将监测点和起始点传入
    //计算hcost： 将监测点和目标点传入
    private int CalculateDistanceCost(Tile tileA, Tile tileB)
    {
        //横坐标之差
        int xdistance = (int)Mathf.Abs(tileA.transform.position.x - tileB.transform.position.x);
        int ydistance = (int)Mathf.Abs(tileA.transform.position.y - tileB.transform.position.y);
        return xdistance + ydistance;
    }

    private Tile GetLowestFCostTile(List<Tile> list)
    {
        Tile tile = list[0];
        foreach(var currentTile in list)
        {
            if(currentTile.fcost < tile.fcost)
            {
                tile = currentTile;
            }
        }
        //list列表中fcost最小的tile
        return tile; 
    }

    public void GetPath(Tile endTile)
    {
        //最终点必须要有父节点
        if(endTile.parentTile == null)
        {
            return;
        }
        else
        {
            path.Add(endTile);
            Tile tempTile = endTile.parentTile;
            while (tempTile != null)
            {
                path.Add(tempTile);
                tempTile = tempTile.parentTile;
            }
            path.Reverse();
        }
    }
    public void ResetPath()
    {
        path.Clear();
    }
}
