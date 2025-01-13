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
    ATTACK_EQUIP, //WEAPON_EQUIP
    COMMON_EQUIP,
}
public class ObjectPool : MonoBehaviour
{
    public Dictionary<GameObjectType, Queue<GameObject>> gameObjectsPool;
    public Dictionary<BattlePrefabType, Queue<GameObject>> battleObjectsPool;
    private ResourcesMananger rm;

    private void Awake()
    {
        rm = FindObjectOfType<ResourcesMananger>();
        gameObjectsPool = new Dictionary<GameObjectType, Queue<GameObject>>();
        battleObjectsPool = new Dictionary<BattlePrefabType, Queue<GameObject>>();
        Invoke("PreLoad",0.2f); 
    }

    private void Copy(GameObjectType gameObjectType)
    {
        if (!gameObjectsPool.ContainsKey(gameObjectType))
        {
            //创建池子
            gameObjectsPool.Add(gameObjectType, new Queue<GameObject>());

        }
        Queue<GameObject> valuePool = gameObjectsPool[gameObjectType];
        valuePool.Enqueue(Instantiate(rm.prefabDic[gameObjectType]));
    }
    private void Copy(BattlePrefabType gameObjectType)
    {
        if (!battleObjectsPool.ContainsKey(gameObjectType))
        {
            //创建池子
            battleObjectsPool.Add(gameObjectType, new Queue<GameObject>());

        }
        Queue<GameObject> valuePool = battleObjectsPool[gameObjectType];
        GameObject tempGameObj = Instantiate(rm.battlePrefabDic[gameObjectType]);
        tempGameObj.SetActive(false);
        valuePool.Enqueue(tempGameObj);
    }

    //外界拿到对象的方法
    public GameObject GetGameObject(GameObjectType gameObjectType)
    {   
        //确认有这个池子
        if (gameObjectsPool.ContainsKey(gameObjectType))
        {   
            //池子内有对象
            if (gameObjectsPool[gameObjectType].Count > 0)
            {
                GameObject gobj = gameObjectsPool[gameObjectType].Dequeue();
                gobj.SetActive(true);
                return gobj;
            }
            else
            {
                //池子内的对象已经被取完了
                Copy(gameObjectType);
                GameObject gobj = gameObjectsPool[gameObjectType].Dequeue();
                return gobj;
            }
        }
        else
        {
            print("没有此对象");
            return null;
        }
    }
    public GameObject GetGameObject(BattlePrefabType gameObjectType)
    {
        //确认有这个池子
        if (battleObjectsPool.ContainsKey(gameObjectType))
        {
            //池子内有对象
            if (battleObjectsPool[gameObjectType].Count > 0)
            {
                GameObject gobj = battleObjectsPool[gameObjectType].Dequeue();
                gobj.SetActive(true);
                return gobj;
            }
            else
            {
                //池子内的对象已经被取完了
                Copy(gameObjectType);
                GameObject gobj = battleObjectsPool[gameObjectType].Dequeue();
                return gobj;
            }
        }
        else
        {
            print("没有此对象");
            return null;
        }
    }
    //外界归还对象的方法
    public void ReturnGameObject(GameObjectType gameObjectType, GameObject gobj)
    {
        gobj.SetActive(false);
        gameObjectsPool[gameObjectType].Enqueue(gobj);
    }
    public void ReturnGameObject(BattlePrefabType gameObjectType, GameObject gobj)
    {
        gobj.SetActive(false);
        battleObjectsPool[gameObjectType].Enqueue(gobj);
    }

    public void PreLoad()
    {
        for(int i = 0; i <5; i++)
        {
            Copy(GameObjectType.MOVEBUTTON);
            Copy(GameObjectType.ATTACKBUTTON);
            Copy(GameObjectType.STANDBUTTON);
            Copy(GameObjectType.BUTTONLIST);
            Copy(BattlePrefabType.Archer);
            Copy(BattlePrefabType.Solider);

            Copy(GameObjectType.EQUIPLIST);
            Copy(GameObjectType.ATTACK_EQUIP);
        }
    }
}
