using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class BattleEventHandlder : MonoBehaviour
{
    public Unit attackUnit; //攻击者
    public Unit beattacked; //被攻击者
    public Animator attackUnitAnimator; //攻击方的动画组件
    List<BattleAttackActiveComp> objectsWithBattleAttackActiveComp = new List<BattleAttackActiveComp>();
    public Vector3 attackUnitOriPosition;
    public BattleHandler battleHandldler;
    private List<SpriteRendererState> spriteStates = new List<SpriteRendererState>();
    // 存储每个SpriteRenderer的初始颜色
    private struct SpriteRendererState
    {
        public SpriteRenderer renderer;
        public Color initialColor;
    }
    //public Dictionary<AttackType, AttackPathProfile> attackPathDict;
    //public GameObject pathgroup;


    
    private void Awake()
    {
        /*        attackPathDict = new Dictionary<AttackType, AttackPathProfile>();
                AttackPathProfile[] group = pathgroup.GetComponentsInChildren<AttackPathProfile>();
                foreach(var pathData in group)
                {
                    if (!attackPathDict.ContainsKey(pathData.attackType))
                    {
                        attackPathDict.Add(pathData.attackType, pathData);
                    }
                }*/
        // 获取所有子对象的SpriteRenderer并保存其初始值
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var renderer in renderers)
        {
            // 保存初始值的拷贝
            SpriteRendererState state = new SpriteRendererState
            {
                renderer = renderer,
                initialColor = new Color(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a)
            };
            spriteStates.Add(state);
        }
    }

    public IEnumerator AttackMovement()
    {
        bool flag = true;
        while (flag)
        {   
            //这里就是要把我们的角色的位置送到要攻击的对象附近
            //在这里写位置播放的动画
            yield return StartAttack();
            flag = false;
        }
    }
    //TODO: 一个距离目标太近了 2. 要有弧线  3. 速度与动画时间匹配问题，可能会导致人物没有归为就提前结束战斗。 4.打击点以及主动攻击物体
    // 5. 角色可以根据装备不同的武器，魔法，播放不同的动画
    public IEnumerator StartAttack()
    {
        //检测位置到达目标之后就可以返回
        GameObject target = beattacked.attackPrefab;
        BattleBeAttackedComp[] foundBeattackedPoint = target.GetComponentsInChildren<BattleBeAttackedComp>(); //Head, Body , Foot ......
        Dictionary<AttackPointType, BattleBeAttackedComp> attackpontDict = new Dictionary<AttackPointType, BattleBeAttackedComp> ();
        foreach (var item in foundBeattackedPoint)
        {
            attackpontDict.Add(item.attackPointType,item);
        }
        // 1.挂了 BattleAttackActiveComp 组件，并且与BattleEventHandlder同级
        // 2.挂了 BattleAttackActiveComp 组件，但在子级中
        // 3.过了 多个BattleAttackActiveComp组件。
        // 被攻击的对象，有多个受击部位 。。。 太复杂了，不考虑
        objectsWithBattleAttackActiveComp.Clear();

        BattleAttackActiveComp[] foundComponents = GetComponentsInChildren<BattleAttackActiveComp>(true);
        // 如果找到的组件数量大于 0，将它们所在的 GameObject 添加到 List 中
        if (foundComponents.Length > 0)
        {
            foreach (var component in foundComponents)
            {
                // 将拥有 BattleAttackActiveComp 组件的 GameObject 添加到列表中
                objectsWithBattleAttackActiveComp.Add(component);

                // 如果 needSaveOriginalPosition 为 true，则保存当前位置信息到 oriPosition
                if (component.needSaveOriginalPosition)
                {
                    component.oriPosition = component.transform.position;
                }
                component.targetPosition = attackpontDict[component.attack_animationData[attackUnit.attackType].targetAttackPointType].transform.position;
            }
        }
        else {
            yield break;
        
        }
        yield return MoveAllToTarget();
        //没挂情况就先不管了
        //执行每一个Comp身上的位移动画


    }

    //假如说全是移动， 这里做的是同一个动画中，有多个移动的对象
    private IEnumerator MoveAllToTarget()
    {
        // 遍历所有 GameObject 并为每个 GameObject 开启移动协程
        List<IEnumerator> moveCoroutines = new List<IEnumerator>();
        foreach (var obj in objectsWithBattleAttackActiveComp)
        {
            moveCoroutines.Add(MoveToPosition(obj));
        }

        // 同时启动所有移动协程
        foreach (var coroutine in moveCoroutines)
        {
            StartCoroutine(coroutine);
        }

        // 等待直到所有对象都到达目标位置
        yield return new WaitUntil(AllObjectsReachedTarget);
    }
    private IEnumerator MoveToPosition(BattleAttackActiveComp obj)
    {
        //Vector3 moveDirNormalized = (targetPos - obj.gameObject.transform.position).normalized; //变成单位向量,知道方向
        Vector3 curveStartPoint = obj.gameObject.transform.position; //距离
        AnimationCurve attackTypeAnimationCurve = obj.attack_animationData[attackUnit.attackType].animationCurve;
        AnimationCurve animation_correction_curve = obj.attack_attack_move_correctionCurve;
        float curve_max_height = obj.attack_animationData[attackUnit.attackType].curve_max_height;
        CalculateAnimationMoveSpeed(obj,0); //初始速度
        //obj（移动的物体）的transform.position 与 targetPosition的方向向量
        Vector3 objToTargetPositionDirection = obj.targetPosition - obj.gameObject.transform.position;
        //计算Obj 的右向量和 objToTargetPositionDirection 的点积
        float dotProduct = Vector3.Dot(obj.gameObject.transform.right, objToTargetPositionDirection);
        Vector3 targetPos = obj.targetPosition;
        if (dotProduct > 0)
        {   
            //被攻击对象在obj的右边
            targetPos = targetPos + new Vector3(-obj.attack_animationData[attackUnit.attackType].attackPointOffset.x, obj.attack_animationData[attackUnit.attackType].attackPointOffset.y, 0);
            
        }
        else if (dotProduct < 0)
        {
            //被攻击对象在obj的左边
            targetPos = targetPos + new Vector3(obj.attack_animationData[attackUnit.attackType].attackPointOffset.x, obj.attack_animationData[attackUnit.attackType].attackPointOffset.y,0);
        }
        else
        {   
            //=0   x=0
            //正前方或者正后方 --用不到
        }        //offset
        while (Vector3.Distance(obj.gameObject.transform.position, targetPos) > obj.distanceToTarget)
        {
            //说明还要继续移动
            Vector3 targetpositionToStartPoint = targetPos - curveStartPoint; //假如是负的，就说明需要往X轴的负方向移动，那么需要速度是负数
            float moveSpeed = obj.moveSpeed;
            if (targetpositionToStartPoint.x < 0)
            {
                moveSpeed = -moveSpeed;
            }
            float nextPositionX = obj.gameObject.transform.position.x + moveSpeed * Time.deltaTime ; //X轴的位移
            float nextPositionXNormalized = (nextPositionX - curveStartPoint.x) / targetpositionToStartPoint.x;// 通过计算X轴的增量百分比，来确定Y轴的百分比


            float nextPositionYNormalized = attackTypeAnimationCurve.Evaluate(nextPositionXNormalized); //Y轴的增量
            float nextPositionYCorrectionNormalizedValue = animation_correction_curve.Evaluate(nextPositionXNormalized);
            float nextPositionYAbsoluteValue = nextPositionYCorrectionNormalizedValue * targetpositionToStartPoint.y;
            float curve_height = Mathf.Abs(targetPos.x - obj.gameObject.transform.position.x) * curve_max_height; //曲线的放大倍数
            float nextPositionY = curveStartPoint.y + nextPositionYNormalized * curve_height + nextPositionYAbsoluteValue;
            Vector3 nextPosition = new Vector3(nextPositionX, nextPositionY, obj.gameObject.transform.position.z);
            CalculateAnimationMoveSpeed(obj,nextPositionXNormalized);
            obj.gameObject.transform.position  = nextPosition;
            //obj.gameObject.transform.position += moveDirNormalized * obj.movespeed;
            yield return null;
        }
        obj.gameObject.transform.position = targetPos;
    }
    private void CalculateAnimationMoveSpeed(BattleAttackActiveComp obj,float xNormalized)
    {
        //决定下一次的移动速度
        float nextMoveSpeedNormalized = obj.attack_animationData[attackUnit.attackType].speedCurve.Evaluate(xNormalized);
        obj.moveSpeed = nextMoveSpeedNormalized * obj.attack_animationData[attackUnit.attackType].speed_curve_MaxSpeed;
    }
    //检测是否到达位置
    private bool AllObjectsReachedTarget()
    {
        // 检查所有对象是否都已经到达目标位置
        foreach (var obj in objectsWithBattleAttackActiveComp)
        {
            if (Vector3.Distance(obj.transform.position, obj.targetPosition) > 0.1f)
            {
                return false;
            }
        }
        return true;
    }

    //假如说全是移动， 这里做的是同一个动画中，有多个移动的对象
    private IEnumerator MoveBackToTarget()
    {
        // 遍历所有 GameObject 并为每个 GameObject 开启移动协程
        List<IEnumerator> moveCoroutines = new List<IEnumerator>();
        foreach (var obj in objectsWithBattleAttackActiveComp)
        {
            if (obj.needSaveOriginalPosition)
            {
                moveCoroutines.Add(MoveBackToPosition(obj, obj.oriPosition));
            }
        }

        // 同时启动所有移动协程
        foreach (var coroutine in moveCoroutines)
        {
            StartCoroutine(coroutine);
        }

        // 等待直到所有对象都到达目标位置
        yield return new WaitUntil(AllObjectsBackToTarget);
    }
    //Todo: 子集回归位置会受到父级的影响，我们之前记录的位置是世界坐标的位置
    private IEnumerator MoveBackToPosition(BattleAttackActiveComp obj, Vector3 targetPos)
    {
        Vector3 moveDirNormalized = (targetPos - obj.gameObject.transform.position).normalized; //变成单位向量,知道方向
        AnimationCurve attackTypeAnimationCurve = obj.attack_animationData[attackUnit.attackType].animationCurve;
        float moveSpeed = obj.moveSpeed;
        float curve_max_height = obj.attack_animationData[attackUnit.attackType].curve_max_height;
        while (Vector3.Distance(obj.gameObject.transform.position, targetPos) > obj.distanceToTarget)
        {
            //说明还要继续移动
            obj.gameObject.transform.position += moveDirNormalized * moveSpeed * Time.deltaTime;
            yield return null;
        }
        obj.gameObject.transform.position = targetPos;
    }
    //检测是否到达位置
    private bool AllObjectsBackToTarget()
    {
        // 检查所有对象是否都已经到达目标位置
        foreach (var obj in objectsWithBattleAttackActiveComp)
        {
            if (!obj.needSaveOriginalPosition) {
                //不需要移动，所以直接视为已经达到目的地
                return true;
            }
            if (Vector3.Distance(obj.transform.position, obj.targetPosition) > 0.1f)
            {
                return false;
            }
        }
        return true;
    }


    //发生在动画过程中
    public IEnumerator AttackResult()
    {
        attackUnitAnimator.speed = 0;
        yield return ResultHandle();
        attackUnitAnimator.speed = 1;
    }
    public IEnumerator AttackUnitMoveBack()
    {
        yield return MoveBackToTarget();
    }

    public IEnumerator ResultHandle()
    {
        int randomSeed = Random.Range(0,101);// 0可以取得到， 101取不到， 需要值是0~100的整型

        if(randomSeed <= 10)
        {
            //闪避
            beattacked.attackPrefab.GetComponentInChildren<Animator>().Play("miss");
            while (!beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("miss"))
            {
                yield return null;
            }
            //判断当前动画是否已经完成
            while (beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("miss"))
            {
                yield return null; //卡在动画播放
            }

        }
        else
        {
            //扣血 播放扣血UI动画
            if(this.beattacked.playerID == 1)
            {
                yield return battleHandldler.player1_healthbar.GetComponent<HealthController>().DamageTaken_Smooth(attackUnit.CalculateDamage(beattacked));

            }
            else
            {
                yield return battleHandldler.player2_healthbar.GetComponent<HealthController>().DamageTaken_Smooth(attackUnit.CalculateDamage(beattacked));
            }
            if(this.beattacked.health <= 0)
            //检测是否需要播放死亡动画
            {
                beattacked.attackPrefab.GetComponentInChildren<Animator>().SetBool("Dead",true);
                while (!beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Dead"))
                {
                    yield return null;
                }
                //判断当前动画是否已经完成
                while (beattacked.attackPrefab.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Dead"))
                {
                    yield return null; //卡在动画播放
                }

            }
        }

    }
    public void SetBattleHandler(BattleHandler battleHandler)
    {
        this.battleHandldler = battleHandler;
    }
    private void OnDisable()
    {
        foreach (var state in spriteStates)
        {
            if (state.renderer != null)
            {
                // 重置为初始颜色
                state.renderer.color = state.initialColor;
            }
        }
    }
}
