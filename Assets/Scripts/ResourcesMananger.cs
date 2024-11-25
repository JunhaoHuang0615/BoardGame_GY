using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;

public enum CSVResource
{
    Weapon,
    PlayerChracter,
    EnemyChracter,
}
public class ResourcesMananger : MonoBehaviour
{
    //按钮资源
    public Dictionary<GameObjectType, GameObject> prefabDic;
    public Dictionary<BattlePrefabType, GameObject> battlePrefabDic;
    public Dictionary<PlatformType, GameObject> platformPrefabDic;
    public Dictionary<PawnType, GameObject> pawnPrefabDic;
    // 画像のキャッシュを保持する辞書
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();

    private Dictionary<string, byte[]> loadedBytesData = new Dictionary<string, byte[]>(); //缓存从硬盘上获取的CSV-bytes的文档

    private void Awake()
    {
        prefabDic = new Dictionary<GameObjectType, GameObject>();
        prefabDic.Add(GameObjectType.MOVEBUTTON,(GameObject)Resources.Load("Prefabs/UI/MoveButton"));
        prefabDic.Add(GameObjectType.ATTACKBUTTON,(GameObject)Resources.Load("Prefabs/UI/AttackButton"));
        prefabDic.Add(GameObjectType.STANDBUTTON,(GameObject)Resources.Load("Prefabs/UI/StandButton"));
        prefabDic.Add(GameObjectType.BUTTONLIST,(GameObject)Resources.Load("Prefabs/UI/ButtonList"));
        prefabDic.Add(GameObjectType.EQUIPLIST, (GameObject)Resources.Load("Prefabs/UI/Equipment/EquipmentList"));
        prefabDic.Add(GameObjectType.ATTACK_EQUIPMENT, (GameObject)Resources.Load("Prefabs/UI/Equipment/Attack_equip"));
        prefabDic.Add(GameObjectType.HEALTHBAR_GROUP, (GameObject)Resources.Load("Prefabs/UI/HealBarGroup"));
        prefabDic.Add(GameObjectType.HEALTHBAR_SLIDER, (GameObject)Resources.Load("Prefabs/UI/HealthBarSlider")); 


        battlePrefabDic = new Dictionary<BattlePrefabType, GameObject>();
        battlePrefabDic.Add(BattlePrefabType.Archer, (GameObject)Resources.Load("Prefabs/Player/Player1BattleGroup"));
        battlePrefabDic.Add(BattlePrefabType.Solider, (GameObject)Resources.Load("Prefabs/Player/Player1BattleGroup"));

        platformPrefabDic = new Dictionary<PlatformType, GameObject>();
        platformPrefabDic.Add(PlatformType.STONE_ROAD, (GameObject)Resources.Load("Prefabs/UI/Platforms/Arena-Melee 1"));
        platformPrefabDic.Add(PlatformType.GRASS_TREE, (GameObject)Resources.Load("Prefabs/UI/Platforms/Arena-Melee 2"));
        platformPrefabDic.Add(PlatformType.GRASS, (GameObject)Resources.Load("Prefabs/UI/Platforms/Arena-Melee"));

        pawnPrefabDic = new Dictionary<PawnType, GameObject>();
        pawnPrefabDic.Add(PawnType.Saber, (GameObject)Resources.Load("Prefabs/Player/player"));

        //加载CSVbytes:

    }

    //如果字典里面已经存在了CSV-bytes的资源，那就直接返回字典的内容，如果没有，则需要Load进来
    public byte[] LoadBytes(string filePath)
    {
        if (!loadedBytesData.ContainsKey(filePath))
        {
            TextAsset bytesAsset = Resources.Load<TextAsset>(filePath);
            if (bytesAsset != null)
            {
                loadedBytesData[filePath] = bytesAsset.bytes;
                Debug.Log($"File loaded and cached: {filePath}");
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }

        return loadedBytesData.ContainsKey(filePath) ? loadedBytesData[filePath] : null;
    }

    public byte[] GetCSVBytes(CSVResource resourceType, string filePath)
    {
        return LoadBytes(filePath);
    }

    // 画像をロードするメソッド
    public Sprite LoadWeaponImage(string imagePath)
    {
        // まずキャッシュに存在するかを確認
        if (imageCache.ContainsKey(imagePath))
        {
            return imageCache[imagePath];
        }

        // キャッシュにない場合は新しくロード
        Texture2D texture = Resources.Load<Texture2D>(imagePath);
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // ロードした画像をキャッシュに追加
            imageCache[imagePath] = sprite;

            return sprite;
        }

        Debug.LogError($"Failed to load image at path: {imagePath}");
        return null;
    }

}
