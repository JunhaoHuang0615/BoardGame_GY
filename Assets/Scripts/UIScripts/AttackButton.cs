using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackButton : ButtonFunction
{
    public override void Init()
    {
        this.GetComponent<Image>().sprite = buttnSprite;
        this.buttonType = ButtonType.ATTACK;
    }

    public override void OnButtonClick()
    {
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.ShowAttackRange(unit.standOnTile);
        unit.CloseButtonList();
        this.unit.canAttack = true;
    }

    public override void OnButtonEnter()
    {
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.ShowAttackRange(unit.standOnTile);
    }

    public override void RestButton()
    {

    }

}
