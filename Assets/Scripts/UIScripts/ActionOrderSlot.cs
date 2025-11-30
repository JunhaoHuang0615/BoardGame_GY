using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionOrderSlot : MonoBehaviour
{
    [Header("UI组件")]
    public Image unitIcon; //单位图标
    public Image actionBarFill; //行动条填充
    public Text unitNameText; //单位名称文本
    public Text actionValueText; //行动值文本
    public GameObject highlightFrame; //高亮边框（用于显示下一个行动的单位）

    public Unit currentUnit; //当前显示的单位（改为public以便外部访问）

    //初始化槽位
    public void Initialize(Unit unit)
    {
        currentUnit = unit;
        if (unitNameText != null && unit != null)
        {
            unitNameText.text = unit.name;
        }
    }

    //更新槽位显示
    public void UpdateSlot(Unit unit, int index, GameManager gm)
    {
        currentUnit = unit;

        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        //更新单位名称
        if (unitNameText != null)
        {
            //使用单位名称，如果没有名称则使用默认名称
            string displayName = !string.IsNullOrEmpty(unit.name) ? unit.name : $"Unit_{unit.GetInstanceID()}";
            unitNameText.text = displayName;

            //确保文本可见
            if (!unitNameText.gameObject.activeSelf)
            {
                unitNameText.gameObject.SetActive(true);
            }

            //确保文本组件启用
            if (!unitNameText.enabled)
            {
                unitNameText.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning($"ActionOrderSlot: unitNameText is null for unit {unit.name}");
        }

        //更新行动值百分比
        float actionPercent = gm.GetUnitActionValuePercent(unit);
        if (actionBarFill != null)
        {
            actionBarFill.fillAmount = actionPercent;
        }

        //更新行动值文本
        if (actionValueText != null)
        {
            actionValueText.text = $"{unit.currentActionValue:F0}/{GameManager.roundDistance:F0}";
            if (!actionValueText.enabled)
            {
                actionValueText.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning($"ActionOrderSlot: actionValueText is null for unit {unit.name}");
        }

        //高亮显示当前正在行动的单位（最右边的单位）
        bool isCurrent = (gm.currentActiveUnit == unit);

        if (highlightFrame != null)
        {
            highlightFrame.SetActive(isCurrent);
        }

        //设置颜色（根据是否达到行动值）
        if (actionBarFill != null)
        {
            if (unit.currentActionValue >= GameManager.roundDistance)
            {
                //达到行动值，显示绿色
                actionBarFill.color = Color.green;
            }
            else
            {
                //未达到，显示蓝色
                actionBarFill.color = Color.blue;
            }
        }
    }
}

