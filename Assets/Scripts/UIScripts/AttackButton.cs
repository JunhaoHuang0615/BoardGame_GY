using System;
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
        /*        gm.ResetMoveableRange();
                gm.ResetMovePath();
                unit.ShowAttackRange(unit.standOnTile);
                unit.CloseButtonList();
                this.unit.canAttack = true;
                Action attackButtonResetAction = new Action(RestButton);
                gm.actions.Push(attackButtonResetAction);*/

        OpenWeaponList();
        Action attackButtonResetAction = new Action(RestButton);
        gm.actions.Push(attackButtonResetAction); 



    }
    public void OpenWeaponList()
    {
        gm.selectedUnit.CloseButtonList();
        DesideButton();
        this.unit.equipButtonList.GenerateButtons();
    }

    public void DesideButton()
    {
        //TODO: 后续需要根据游戏进程，或者角色拥有的武器来生成按钮
        foreach (string weapon in gm.selectedUnit.weaponList)
        {
            gm.selectedUnit.equipButtonList.AddButton(GameObjectType.ATTACK_EQUIP,weapon,this);
        }
    }

    public void CloseWeaponList()
    {

    }

    public override void OnButtonEnter()
    {
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.ShowAttackRange(unit.standOnTile);
    }

    public override void RestButton()
    {
        if (this.unit.stand == true)
        {
            if (gm.actions.Count > 0)
            {
                Action action = gm.actions.Pop();
                action();

            }
            return;
            //让下一个撤回的方法被执行
        }
        if (gm.selectedUnit != null)
        {
            gm.selectedUnit.CloseButtonList();
            gm.selectedUnit.canExcute = false;
            gm.selectedUnit.selected = false;
            gm.selectedUnit.playerAnimator.SetAnimationParam(gm.selectedUnit, 0, 0);
        }
        gm.selectedUnit = this.unit;
        this.unit.selected = true;
        this.unit.CloseEquipmentList();
        this.unit.playerAnimator.SetAnimationParam(this.unit, 0, -1);
        this.unit.OpenButtonList();
        this.unit.canExcute = false;
    }

}
