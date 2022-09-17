using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
public enum AttackType
{
    CLOSE_COMBAT, //近战
    ARROW,
}
//这里要写攻击方式


public class AttackPathProfile : MonoBehaviour
{
    public AttackType attackType;
    public PathCreator attackPath; //进攻路线
    public PathCreator backPath; //进攻后的退回路线
    public GameObject controlObj;
    public float attackStartTime; //进攻发生的时间
    public float attackEndTime; //进攻完成的时间
    public float backStartTime;
    public float backEndTIme;


    //近战攻击方式
    public IEnumerator Close_Combat()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(attackPath.path.length - distanceTravelled) > 0.5f)
        {
            controlObj.transform.position = attackPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (attackPath.path.length / (attackEndTime - attackStartTime)) * Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator AttackAni()
    {
        bool flag = true;
        
        if(attackType == AttackType.CLOSE_COMBAT)
        {
            while(flag == true)
            {
                yield return Close_Combat();
                flag = false;
            }
        }
        else if(attackType == AttackType.ARROW)
        {
            while (flag == true)
            {
                yield return Close_Combat();
                flag = false;
            }
        }
    }
}

