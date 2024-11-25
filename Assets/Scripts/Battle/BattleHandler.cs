using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleHandler : MonoBehaviour
{
    //发起攻击的Unit是哪一个
    private Unit activeUnit; //发起攻击的一方
    private Unit passiveUnit; //被发起攻击的一方
    private GameManager gm;
    private SceneLoader sl;
    private Canvas canvas;
    private TileDataManager tileDataManager;
    private ObjectPool objectPool;
    private GameObject platform_player1;
    private GameObject platform_player2;
    public GameObject player1_healthbar;
    public GameObject player2_healthbar;
    public Unit deadUnit;
    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        sl = FindObjectOfType<SceneLoader>();
        objectPool = FindObjectOfType<ObjectPool>();
        tileDataManager = FindObjectOfType<TileDataManager>();
        gm.passiveUnitCanCounterAttack = gm.passiveUnit.CanCounterAttack(gm.activeUnit);
        sl.SetActiveSceneByName("BattleScene");
        if (canvas == null)
        {
            Scene battleScene = sl.GetSceneByName("BattleScene");
            canvas = sl.FindOrCreateCanvasInScene(battleScene);
        }
        StartCoroutine (StartBattle(gm.activeUnit,gm.passiveUnit));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator StartBattle(Unit activeUnit, Unit passiveUnit)
    {
        deadUnit = null;
        this.activeUnit = activeUnit;
        this.passiveUnit = passiveUnit;
        this.activeUnit.isAttacking = true;
        //生成攻击时所有的Prefab
        activeUnit.attackPrefab = objectPool.GetGameObject(activeUnit.battlePreType);
        passiveUnit.attackPrefab = objectPool.GetGameObject(passiveUnit.battlePreType);
        this.activeUnit.attackPrefab.GetComponentInChildren<Animator>().SetBool("Dead",false);
        this.passiveUnit.attackPrefab.GetComponentInChildren<Animator>().SetBool("Dead", false);
        activeUnit.attackPrefab.GetComponentInChildren<BattleEventHandlder>().SetBattleHandler(this);
        passiveUnit.attackPrefab.GetComponentInChildren<BattleEventHandlder>().SetBattleHandler(this);


        HealthController player1_healthbar_controller = null;
        HealthController player2_healthbar_controller = null;
        //创建HealBar实例
        if(activeUnit.playerID == 1)
        {
            //说明血条要创建在右边
            player1_healthbar = objectPool.GetGameObject(GameObjectType.HEALTHBAR_GROUP);
            player1_healthbar_controller = player1_healthbar.GetComponent<HealthController>();
            player1_healthbar_controller.Initialize_Healthbar(activeUnit.health,activeUnit.maxHealth);

        }
        else
        {
            player2_healthbar = objectPool.GetGameObject(GameObjectType.HEALTHBAR_GROUP);
            player2_healthbar_controller = player2_healthbar.GetComponent<HealthController>();
            player2_healthbar_controller.Initialize_Healthbar(activeUnit.health, activeUnit.maxHealth);
        }
        if (passiveUnit.playerID == 1)
        {

            player1_healthbar = objectPool.GetGameObject(GameObjectType.HEALTHBAR_GROUP);
            player1_healthbar_controller = player1_healthbar.GetComponent<HealthController>();
            player1_healthbar_controller.Initialize_Healthbar(passiveUnit.health, passiveUnit.maxHealth);

        }
        else {
            player2_healthbar = objectPool.GetGameObject(GameObjectType.HEALTHBAR_GROUP);
            player2_healthbar_controller = player2_healthbar.GetComponent<HealthController>();
            player2_healthbar_controller.Initialize_Healthbar(passiveUnit.health, passiveUnit.maxHealth);

        }
        player1_healthbar.transform.SetParent(canvas.transform, false);
        player2_healthbar.transform.SetParent(canvas.transform, false);

        //地形创建
        // 检测双方是近战攻击还是远程攻击：
        if (gm.CheckUnitAdjacent(this.activeUnit, this.passiveUnit))
        {
            //是近战
            //近战攻击： 右边：2.7 -1.5 -5  左边：-2.7，-1.5，-5
            //player的platform也放在右边
            if (activeUnit.playerID == 1)
            {
                platform_player1 = objectPool.GetGameObject(activeUnit.standOnTile.platformType);
                platform_player1.transform.position = new Vector3(2.7f, -1.5f, -5);
                platform_player1.transform.localScale = new Vector3(-1, 1, 1);
                platform_player2 = objectPool.GetGameObject(passiveUnit.standOnTile.platformType);
                platform_player2.transform.position = new Vector3(-2.7f, -1.5f, -5);
                platform_player2.transform.localScale = new Vector3(1, 1, 1);

                //创建角色
                //z=-5是为了让此单位更接近摄像机
                activeUnit.attackPrefab.transform.position = new Vector3(2.5f, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                passiveUnit.attackPrefab.transform.position = new Vector3(-2.5f, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                platform_player1 = objectPool.GetGameObject(passiveUnit.standOnTile.platformType);
                platform_player1.transform.position = new Vector3(2.7f, -1.5f, -5);
                platform_player1.transform.localScale = new Vector3(-1, 1, 1);
                platform_player2 = objectPool.GetGameObject(activeUnit.standOnTile.platformType);
                platform_player2.transform.position = new Vector3(-2.7f, -1.5f, -5);
                platform_player2.transform.localScale = new Vector3(1, 1, 1);

                passiveUnit.attackPrefab.transform.position = new Vector3(2.5f, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                activeUnit.attackPrefab.transform.position = new Vector3(-2.5f, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }

        }
        else 
        {
            //远程的情况
            //远程攻击： 右边：6 -1.5 -5    左边：-6 -1.5 -5
            if (activeUnit.playerID == 1) 
            {
                platform_player1 = objectPool.GetGameObject(activeUnit.standOnTile.platformType);
                platform_player1.transform.position = new Vector3(6f, -1.5f, -5);
                platform_player1.transform.localScale = new Vector3(-1, 1, 1);
                platform_player2 = objectPool.GetGameObject(passiveUnit.standOnTile.platformType);
                platform_player2.transform.position = new Vector3(-6f, -1.5f, -5);
                platform_player2.transform.localScale = new Vector3(1, 1, 1);

                //创建角色
                //z=-5是为了让此单位更接近摄像机
                activeUnit.attackPrefab.transform.position = new Vector3(5, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                passiveUnit.attackPrefab.transform.position = new Vector3(-5, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                platform_player1 = objectPool.GetGameObject(passiveUnit.standOnTile.platformType);
                platform_player1.transform.position = new Vector3(6, -1.5f, -5);
                platform_player1.transform.localScale = new Vector3(-1, 1, 1);
                platform_player2 = objectPool.GetGameObject(activeUnit.standOnTile.platformType);
                platform_player2.transform.position = new Vector3(-6, -1.5f, -5);
                platform_player2.transform.localScale = new Vector3(1, 1, 1);

                //创建角色
                //此时玩家ID1被作为了被发起攻击的对象
                passiveUnit.attackPrefab.transform.position = new Vector3(5, 0, -5);
                passiveUnit.attackPrefab.transform.localScale = new Vector3(1, 1, 1);

                activeUnit.attackPrefab.transform.position = new Vector3(-5, 0, -5);
                activeUnit.attackPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }


        }
        //放置血条
        if (activeUnit.playerID == 1)
        {
            player1_healthbar.transform.position = activeUnit.attackPrefab.transform.position + new Vector3(0, 2, 0);
            player2_healthbar.transform.position = passiveUnit.attackPrefab.transform.position + new Vector3(-2, 2, 0);
        }
        else
        {
            player1_healthbar.transform.position = passiveUnit.attackPrefab.transform.position + new Vector3(0, 2, 0);
            player2_healthbar.transform.position = activeUnit.attackPrefab.transform.position + new Vector3(-2, 2, 0);
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
                if (IsUnitDead(passiveUnit))
                {
                    break;
                }
                activePlayerTurns--;
            }
            if(passivePlayerTurns > 0)
            {
                if (gm.passiveUnitCanCounterAttack)
                {
                    yield return AttackTurn(passiveUnit,activeUnit);
                    if (IsUnitDead(activeUnit))
                    {
                        break;
                    }
                    passivePlayerTurns--;
                }
                else
                {
                    if (IsUnitDead(activeUnit))
                    {
                        break;
                    }
                    passivePlayerTurns--;
                }
            }
        }
        objectPool.ReturnGameObject(activeUnit.battlePreType, activeUnit.attackPrefab);
        objectPool.ReturnGameObject(passiveUnit.battlePreType, passiveUnit.attackPrefab);
        objectPool.ReturnGameObject(activeUnit.standOnTile.platformType, platform_player1);
        objectPool.ReturnGameObject(passiveUnit.standOnTile.platformType, platform_player2);
        player1_healthbar.GetComponent<HealthController>().ReturnSlider();
        objectPool.ReturnGameObject(GameObjectType.HEALTHBAR_GROUP, player1_healthbar);
        player2_healthbar.GetComponent<HealthController>().ReturnSlider();
        objectPool.ReturnGameObject(GameObjectType.HEALTHBAR_GROUP, player2_healthbar);
        CameraFollow.instance.ReturnCameraPosition();
        this.activeUnit.isAttacking = false;
        Action action = () => { 
            if(deadUnit != null)
            {
                //说明要执行死亡动画
                gm.StartCoroutine( deadUnit.Dead());
            }
        
        
        };
        sl.UnLoadBattleScene(action);
        

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

    public bool IsUnitDead(Unit unit)
    {
        if (unit.health <= 0 ){
            deadUnit = unit;
            gm.animationWaitting = true;
            return true;
        }
        else { 
        
            return false;
        
        }
    }
}
