using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesMananger : MonoBehaviour
{
    //按钮资源
    public Dictionary<GameObjectType, GameObject> prefabDic;
    public Dictionary<BattlePrefabType, GameObject> battlePrefabDic;

    private void Start()
    {
        prefabDic = new Dictionary<GameObjectType, GameObject>();
        prefabDic.Add(GameObjectType.MOVEBUTTON,(GameObject)Resources.Load("Prefabs/UI/MoveButton"));
        prefabDic.Add(GameObjectType.ATTACKBUTTON,(GameObject)Resources.Load("Prefabs/UI/AttackButton"));
        prefabDic.Add(GameObjectType.STANDBUTTON,(GameObject)Resources.Load("Prefabs/UI/StandButton"));
        prefabDic.Add(GameObjectType.BUTTONLIST,(GameObject)Resources.Load("Prefabs/UI/ButtonList"));

        battlePrefabDic = new Dictionary<BattlePrefabType, GameObject>();
        battlePrefabDic.Add(BattlePrefabType.Archer, (GameObject)Resources.Load("Prefabs/Player/Player1BattleGroup"));
        battlePrefabDic.Add(BattlePrefabType.Solider, (GameObject)Resources.Load("Prefabs/Player/Player1BattleGroup"));
    }
}
