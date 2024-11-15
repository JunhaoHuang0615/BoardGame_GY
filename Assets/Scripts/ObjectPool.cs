using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameObjectType
{
    MOVEBUTTON,
    ATTACKBUTTON,
    STANDBUTTON,
    BUTTONLIST,
    EQUIPLIST,
    ATTACK_EQUIPMENT,
    HEALTHBAR_GROUP,
    HEALTHBAR_SLIDER,
}

public class ObjectPool : MonoBehaviour
{
    public Dictionary<GameObjectType, Queue<GameObject>> gameObjectsPool;
    public Dictionary<BattlePrefabType, Queue<GameObject>> battleObjectsPool;
    public Dictionary<PlatformType, Queue<GameObject>> platformObjectsPool;
    private ResourcesMananger rm;

    private void Awake()
    {
        rm = FindObjectOfType<ResourcesMananger>();
        gameObjectsPool = new Dictionary<GameObjectType, Queue<GameObject>>();
        battleObjectsPool = new Dictionary<BattlePrefabType, Queue<GameObject>>();
        platformObjectsPool = new Dictionary<PlatformType, Queue<GameObject>>();
        Invoke("PreLoad", 0.2f);
    }

    private Transform GetOrCreateParentTransform(string typeName)
    {
        Transform parentTransform = transform.Find(typeName);
        if (parentTransform == null)
        {
            GameObject typeParent = new GameObject(typeName);
            typeParent.transform.SetParent(this.transform,false); //保持本地变换
            parentTransform = typeParent.transform;
        }
        return parentTransform;
    }

    private void Copy(GameObjectType gameObjectType)
    {
        if (!gameObjectsPool.ContainsKey(gameObjectType))
        {
            gameObjectsPool.Add(gameObjectType, new Queue<GameObject>());
        }
        Queue<GameObject> valuePool = gameObjectsPool[gameObjectType];
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());

        GameObject tempGameObj = Instantiate(rm.prefabDic[gameObjectType], parentTransform);
        tempGameObj.SetActive(false);
        valuePool.Enqueue(tempGameObj);
    }

    private void Copy(BattlePrefabType gameObjectType)
    {
        if (!battleObjectsPool.ContainsKey(gameObjectType))
        {
            battleObjectsPool.Add(gameObjectType, new Queue<GameObject>());
        }
        Queue<GameObject> valuePool = battleObjectsPool[gameObjectType];
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());

        GameObject tempGameObj = Instantiate(rm.battlePrefabDic[gameObjectType], parentTransform);
        tempGameObj.SetActive(false);
        valuePool.Enqueue(tempGameObj);
    }

    private void Copy(PlatformType gameObjectType)
    {
        if (!platformObjectsPool.ContainsKey(gameObjectType))
        {
            platformObjectsPool.Add(gameObjectType, new Queue<GameObject>());
        }
        Queue<GameObject> valuePool = platformObjectsPool[gameObjectType];
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());

        GameObject tempGameObj = Instantiate(rm.platformPrefabDic[gameObjectType], parentTransform);
        tempGameObj.SetActive(false);
        valuePool.Enqueue(tempGameObj);
    }

    public GameObject GetGameObject(GameObjectType gameObjectType)
    {
        if (gameObjectsPool.ContainsKey(gameObjectType) && gameObjectsPool[gameObjectType].Count > 0)
        {
            GameObject gobj = gameObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
        else
        {
            Copy(gameObjectType);
            GameObject gobj = gameObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
    }

    public GameObject GetGameObject(BattlePrefabType gameObjectType)
    {
        if (battleObjectsPool.ContainsKey(gameObjectType) && battleObjectsPool[gameObjectType].Count > 0)
        {
            GameObject gobj = battleObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
        else
        {
            Copy(gameObjectType);
            GameObject gobj = battleObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
    }

    public GameObject GetGameObject(PlatformType gameObjectType)
    {
        if (platformObjectsPool.ContainsKey(gameObjectType) && platformObjectsPool[gameObjectType].Count > 0)
        {
            GameObject gobj = platformObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
        else
        {
            Copy(gameObjectType);
            GameObject gobj = platformObjectsPool[gameObjectType].Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
    }

    public void ReturnGameObject(GameObjectType gameObjectType, GameObject gobj)
    {
        gobj.SetActive(false);
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());
        gobj.transform.SetParent(parentTransform, false);
        gameObjectsPool[gameObjectType].Enqueue(gobj);
    }

    public void ReturnGameObject(BattlePrefabType gameObjectType, GameObject gobj)
    {
        gobj.SetActive(false);
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());
        gobj.transform.SetParent(parentTransform, false); //false 代表保持本地变换
        battleObjectsPool[gameObjectType].Enqueue(gobj);
    }

    public void ReturnGameObject(PlatformType gameObjectType, GameObject gobj)
    {
        gobj.SetActive(false);
        Transform parentTransform = GetOrCreateParentTransform(gameObjectType.ToString());
        gobj.transform.SetParent(parentTransform,false);
        platformObjectsPool[gameObjectType].Enqueue(gobj);
    }

    public void PreLoad()
    {
        for (int i = 0; i < 5; i++)
        {
            Copy(GameObjectType.MOVEBUTTON);
            Copy(GameObjectType.ATTACKBUTTON);
            Copy(GameObjectType.STANDBUTTON);
            Copy(GameObjectType.BUTTONLIST);
            Copy(BattlePrefabType.Archer);
            Copy(BattlePrefabType.Solider);
            Copy(GameObjectType.EQUIPLIST);
            Copy(GameObjectType.ATTACK_EQUIPMENT);
            Copy(PlatformType.GRASS);
            Copy(PlatformType.GRASS_TREE);
            Copy(PlatformType.STONE_ROAD);
            Copy(GameObjectType.HEALTHBAR_GROUP);
            Copy(GameObjectType.HEALTHBAR_SLIDER);
        }
    }
}