using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.ComponentModel;

public class LevelEditor : MonoBehaviour
{
    [MenuItem("Tools/Save Chess Pieces to LevelData")]
    public static void SaveChessPiecesToLevelData()
    {
        // 弹出选择目标 LevelData 的窗口
        string path = EditorUtility.OpenFilePanel("Select LevelData", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        // 转换路径为 Unity 相对路径
        path = "Assets" + path.Substring(Application.dataPath.Length);

        // 加载选中的 LevelData
        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (levelData == null)
        {
            Debug.LogError("Selected file is not a valid LevelData asset!");
            return;
        }

        // 清空现有的棋子列表
        levelData.pieces = new List<ChracterMapData>();

        // 查找场景中的所有棋子对象
        ChracterTransform[] chessPieces = FindObjectsOfType<ChracterTransform>();

        foreach (var chessPiece in chessPieces)
        {
            // 创建棋子数据
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(chessPiece.gameObject);
            ChracterMapData pieceData = new ChracterMapData
            {
                unitName = chessPiece.chessMapData.unitName,
                position = new Vector3(chessPiece.transform.position.x, chessPiece.transform.position.y,-1),
                teamID = chessPiece.chessMapData.teamID,
                pawnType = chessPiece.chessMapData.pawnType,
                prefabPath = prefabPath,
            };

            // 添加到 LevelData
            levelData.pieces.Add(pieceData);
        }

        // 标记为已修改并保存
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();

        Debug.Log("Chess pieces saved to LevelData: " + path);
    }
    [MenuItem("Tools/Load Chess Pieces from LevelData")]
    public static void LoadChessPiecesFromLevelData()
    {
        // 弹出选择目标 LevelData 的窗口
        string path = EditorUtility.OpenFilePanel("Select LevelData", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        // 转换路径为 Unity 相对路径
        path = "Assets" + path.Substring(Application.dataPath.Length);

        // 加载选中的 LevelData
        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (levelData == null)
        {
            Debug.LogError("Selected file is not a valid LevelData asset!");
            return;
        }

        // 删除场景中现有的棋子对象
        ChracterTransform[] existingPieces = FindObjectsOfType<ChracterTransform>();
        foreach (var piece in existingPieces)
        {
            DestroyImmediate(piece.gameObject);
        }

        // 根据 LevelData 创建新的棋子
        foreach (var pieceData in levelData.pieces)
        {
            // 根据类型加载预制体（假设你的棋子有对应的预制体）
            GameObject piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pieceData.prefabPath);
            if (piecePrefab == null)
            {
                Debug.LogError($"Prefab for PawnType {pieceData.pawnType} not found!");
                continue;
            }

            // 实例化棋子
            GameObject piece = PrefabUtility.InstantiatePrefab(piecePrefab) as GameObject;
            piece.transform.position = pieceData.position;

            // 设置棋子的脚本属性
            // 设置 ChessPieceComponent 数据
            ChracterTransform component = piece.GetComponent<ChracterTransform>();
            if (component == null) component = piece.AddComponent<ChracterTransform>();

            component.chessMapData = new ChracterMapData
            {
                unitName = pieceData.unitName,
                teamID = pieceData.teamID,
                pawnType = pieceData.pawnType,
                prefabPath = pieceData.prefabPath // 确保数据一致性
            };
        }

        Debug.Log("Chess pieces loaded from LevelData: " + path);
    }
}