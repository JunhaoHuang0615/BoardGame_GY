using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName; // 关卡名称
    public string winCondition; // 胜利条件 (可根据需求扩展为更复杂的类型)
    public List<ChracterMapData> pieces; // 棋子列表
}
