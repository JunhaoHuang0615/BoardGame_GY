using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MoveButton : ButtonFunction {


    public override void Init()
    {
        this.GetComponent<Image>().sprite = buttnSprite;
        this.buttonType = ButtonType.MOVE;
    }

    public override void OnButtonClick()
    {
        unit.canExcute = true;
        unit.ShowMoveRangeTwo();
        unit.CloseButtonList();
        Action moveButtonResetAction = new Action(RestButton);
        gm.actions.Push(moveButtonResetAction);


    }

    public override void OnButtonEnter()
    {
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.ShowMoveRangeTwo();
    }

    public override void RestButton()
    {   
        if(this.unit.stand == true)
        {   
            if(gm.actions.Count > 0)
            {
                Action action = gm.actions.Pop();
                action();

            }
            return;
            //让下一个撤回的方法被执行
        }
        if(gm.selectedUnit != null)
        {
            gm.selectedUnit.CloseButtonList();
            gm.selectedUnit.canExcute = false;
            gm.selectedUnit.selected = false;
            gm.selectedUnit.playerAnimator.SetAnimationParam(gm.selectedUnit,0,0);
        }
        gm.selectedUnit = this.unit;
        this.unit.selected = true;
        this.unit.playerAnimator.SetAnimationParam(this.unit,0,-1);
        this.unit.ShowMoveRangeTwo();
        this.unit.OpenButtonList();
        this.unit.canExcute = false;

    }

}
