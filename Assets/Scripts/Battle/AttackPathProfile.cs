using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class AttackPathProfile : MonoBehaviour
{
    public AttackType attackType;
    public GameObject controlObject; //路径的控制对象
    public PathCreator attackPath; //进攻路线
    public PathCreator backPath; //进攻后的退回路线
    public float attackStartTime; //进攻发生的时间
    public float attackEndTime; //进攻完成的时间
    public float backStartTime;
    public float backEndTIme;

    public IEnumerator StartAttack()
    {
        switch (attackType)
        {
            case AttackType.Close_Combat:

                yield return Close_Combat();

                break;

            case AttackType.Arrow:

                yield return Arrow();
                break;

        }
    }

    private IEnumerator Close_Combat()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(attackPath.path.length - distanceTravelled) > 0.5f)
        {
            controlObject.transform.position = attackPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (attackPath.path.length / (attackEndTime - attackStartTime)) * Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator Arrow()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(attackPath.path.length - distanceTravelled) > 0.5f)
        {
            controlObject.transform.position = attackPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (attackPath.path.length / (attackEndTime - attackStartTime)) * Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator ReturnAttack()
    {
        switch (attackType)
        {
            case AttackType.Close_Combat:

                yield return ReturnCloseCombat();

                break;

            case AttackType.Arrow:

                yield return ReturnArrow();
                break;

        }
    }

    private IEnumerator ReturnCloseCombat()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(backPath.path.length - distanceTravelled) > 0.5f)
        {
            controlObject.transform.position = backPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (backPath.path.length / (backEndTIme - backStartTime)) * Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator ReturnArrow()
    {
        float distanceTravelled = 0; //记录单位在路径上移动了多少

        while (Mathf.Abs(backPath.path.length - distanceTravelled) > 0.5f)
        {
            controlObject.transform.position = backPath.path.GetPointAtDistance(distanceTravelled);
            distanceTravelled = distanceTravelled + (backPath.path.length / (backEndTIme - backStartTime)) * Time.deltaTime;
            yield return null;
        }
    }


}


