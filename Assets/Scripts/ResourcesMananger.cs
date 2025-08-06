using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CSVResource
{
    WEAPON,
}
public class ResourcesMananger : MonoBehaviour
{
    //按钮资源
    public Dictionary<GameObjectType, GameObject> prefabDic;
    public Dictionary<BattlePrefabType, GameObject> battlePrefabDic;
    public Dictionary<CSVResource, byte[]> loadCSVBytesDataDic;

    //缓存图片
    public Dictionary<string, Sprite> weaponImageCache;

    private void Start()
    {
        prefabDic = new Dictionary<GameObjectType, GameObject>();
        prefabDic.Add(GameObjectType.MOVEBUTTON,(GameObject)Resources.Load("Prefabs/UI/MoveButton"));
        prefabDic.Add(GameObjectType.ATTACKBUTTON,(GameObject)Resources.Load("Prefabs/UI/AttackButton"));
        prefabDic.Add(GameObjectType.STANDBUTTON,(GameObject)Resources.Load("Prefabs/UI/StandButton"));
        prefabDic.Add(GameObjectType.BUTTONLIST,(GameObject)Resources.Load("Prefabs/UI/ButtonList"));
        prefabDic.Add(GameObjectType.EQUIPLIST, (GameObject)Resources.Load("Prefabs/UI/EquipUI/EquipButtonList"));
        prefabDic.Add(GameObjectType.ATTACK_EQUIP, (GameObject)Resources.Load("Prefabs/UI/EquipUI/AttackEquipButton"));

        battlePrefabDic = new Dictionary<BattlePrefabType, GameObject>();
        battlePrefabDic.Add(BattlePrefabType.Archer, (GameObject)Resources.Load("Prefabs/Arrow/ArrowBattleGroup"));
        battlePrefabDic.Add(BattlePrefabType.Solider, (GameObject)Resources.Load("Prefabs/Player/Player1BattleGroup"));

        loadCSVBytesDataDic = new Dictionary<CSVResource, byte[]>();
        weaponImageCache = new Dictionary<string, Sprite>();
    }
    public byte[] LoadCSVBytes(CSVResource csvtype, string filePath)
    {
        if (!loadCSVBytesDataDic.ContainsKey(csvtype))
        {
            TextAsset bytsAsset = Resources.Load<TextAsset>(filePath);
            if (bytsAsset != null) {
                loadCSVBytesDataDic[csvtype] = bytsAsset.bytes;
                Debug.Log($"File Loaded and cached: {filePath}");
            }
            else
            {
                Debug.LogError($"File Not Found: {filePath}");
            }
        }
        return loadCSVBytesDataDic.ContainsKey(csvtype)? loadCSVBytesDataDic[csvtype] : null;
    }

    public Sprite LoadWeaponImage(string imagePath)
    {
        if (weaponImageCache.ContainsKey(imagePath))
        {

            return weaponImageCache[imagePath]; 
        
        }
        Texture2D weaponTexture = Resources.Load<Texture2D>(imagePath);
        if (weaponTexture != null) {
            Sprite weaponSprite = Sprite.Create(weaponTexture, new Rect(0,0, weaponTexture.width, weaponTexture.height),new Vector2(0.5f,0.5f));
            weaponImageCache[imagePath] = weaponSprite;

            return weaponSprite;
        }

        Debug.LogError($"Faild to load image at path {imagePath}");
        return null;
    }
}
