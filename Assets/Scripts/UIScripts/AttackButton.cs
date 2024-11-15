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
        /*      gm.ResetMoveableRange();
                gm.ResetMovePath();
                unit.ShowAttackRange(unit.standOnTile);
                unit.CloseButtonList();
                this.unit.canAttack = true;
                Action attackButtonResetAction = new Action(RestButton);
                gm.actions.Push(attackButtonResetAction);*/

        //显示装备的UI
        OpenAttackEquipmengList();
        Action attackButtonResetAction = new Action(RestButton);
        gm.actions.Push(attackButtonResetAction);


    }
    //打开装备栏
    public void OpenAttackEquipmengList()
    {
        gm.selectedUnit.CloseButtonList();
        DesideEquipmentList(); //先决定有哪些装备
        gm.selectedUnit.equipList.GenerateButtons();
    }

    //决定装备栏里面都有什么
    public void DesideEquipmentList()
    {
        //根据Unit身上的武器来生成Button
        foreach (var equip in unit.attackEquipList)
        {    
            gm.selectedUnit.equipList.AddButton(GameObjectType.ATTACK_EQUIPMENT, equip,this);
        }

    }

    //撤回
    public void CloseEquipmengList()
    {

    }

    public override void OnButtonEnter()
    {
/*        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.ShowAttackRange(unit.standOnTile);*/
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
        if (gm.selectedUnit != null) //其它的选择的角色
        {
            gm.selectedUnit.CloseButtonList();
            gm.selectedUnit.canExcute = false;
            gm.selectedUnit.selected = false;
            gm.selectedUnit.playerAnimator.SetAnimationParam(gm.selectedUnit, 0, 0);
        }
        gm.selectedUnit = this.unit;
        Debug.Log("123213");
        this.unit.CloseAttackEquipList();
        this.unit.selected = true;
        this.unit.playerAnimator.SetAnimationParam(this.unit, 0, -1);
        this.unit.OpenButtonList();
        this.unit.canExcute = false;
    }

}
