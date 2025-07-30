using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    //发起攻击的Unit是哪一个
    private Unit activeUnit; //发起攻击的一方
    private Unit passiveUnit; //被发起攻击的一方
    private GameManager gm;
    private SceneLoader sl;
    private ObjectPool objectPool;
    private bool canCounterAttack;
    private bool isAdjacant = true;
    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        sl = FindObjectOfType<SceneLoader>();
        objectPool = FindObjectOfType<ObjectPool>();
        sl.SetActiveSceneByName("BattleScene");
        canCounterAttack = gm.passiveUnit.CanCounterAttacl(gm.activeUnit);
        isAdjacant = CheckIsAdjacant(gm.activeUnit, gm.passiveUnit);



        StartCoroutine (StartBattle(gm.activeUnit,gm.passiveUnit));


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator StartBattle(Unit activeUnit, Unit passiveUnit)
    {
        this.activeUnit = activeUnit;
        this.passiveUnit = passiveUnit;
        this.activeUnit.isAttacking = true;
        //生成攻击时所有的Prefab
        activeUnit.attackPrefab = objectPool.GetGameObject(activeUnit.battlePreType);
        passiveUnit.attackPrefab = objectPool.GetGameObject(passiveUnit.battlePreType);
        //让PlayerID是1的单位始终在右边
        if (activeUnit.playerID == 1)
        {
            if (isAdjacant)
            {
                //-5是为了让此单位更接近摄像机
                activeUnit.attackPrefab.transform.position = new Vector3(2.5f, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                passiveUnit.attackPrefab.transform.position = new Vector3(-2.5f, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                activeUnit.attackPrefab.transform.position = new Vector3(5, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                passiveUnit.attackPrefab.transform.position = new Vector3(-5, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }

        }
        else if (passiveUnit.playerID == 1)
        {
            if (isAdjacant)
            {
                //此时玩家ID1被作为了被发起攻击的对象
                passiveUnit.attackPrefab.transform.position = new Vector3(2.5f, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                activeUnit.attackPrefab.transform.position = new Vector3(-2.5f, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                //此时玩家ID1被作为了被发起攻击的对象
                passiveUnit.attackPrefab.transform.position = new Vector3(5, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                activeUnit.attackPrefab.transform.position = new Vector3(-5, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }

        }

        yield return new WaitForSeconds(5);

        //开始攻击动画
        StartCoroutine(StartAttacking());

    }

    IEnumerator StartAttacking()
    {
        //一个回合：双方均完成攻击之后叫做一个回合的完成
        //双方可以行动几个回合：
        int activePlayerTurns = 2; //发起攻击者可以行动两个回合
        int passivePlayerTurns = 1;
        
        while(activePlayerTurns >0 || passivePlayerTurns > 0)
        {
            //先发起攻击的人将率先行动
            if(activePlayerTurns > 0)
            {
                yield return AttackTurn(activeUnit, passiveUnit);
                activePlayerTurns--;
            }
            if(passivePlayerTurns > 0)
            {
                if (canCounterAttack)
                {
                    yield return AttackTurn(passiveUnit, activeUnit);
                    passivePlayerTurns--;
                }
                else
                {
                    passivePlayerTurns--;
                }

            }
        }
        objectPool.ReturnGameObject(activeUnit.battlePreType, activeUnit.attackPrefab);
        objectPool.ReturnGameObject(passiveUnit.battlePreType, passiveUnit.attackPrefab);
        CameraFollow.instance.ReturnCameraPosition();
        this.activeUnit.isAttacking = false;
        sl.UnLoadBattleScene();
        

    }

    //attackUnit : 攻击的单位
    //beAttactedUnit : 被攻击的单位
    //行动单位的操作逻辑
    IEnumerator AttackTurn(Unit attackUnit, Unit beAttackedUnit)
    {
        int attackCount = 1;
        while(attackCount > 0)
        {
            yield return attackUnit.Attack(beAttackedUnit);
            attackCount--;
        }
    }

    bool CheckIsAdjacant(Unit attackUnit, Unit passiveUnit)
    {
        foreach (var tile in attackUnit.standOnTile.neighbors)
        {
            if(tile == passiveUnit.standOnTile)
            {   
                return true;
            }
        }
        return false;
    }
}
