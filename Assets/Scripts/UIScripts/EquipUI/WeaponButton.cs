
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WeaponButton : EquipButtonFunction
{
    public Weapon weapon;
    public override void Init()
    {
        //this.GetComponent<Image>().sprite = buttnSprite;
        this.buttonType = EquipType.ATTACK_EQUIP;
        weapon = dataManager.GetWeapon(equipName);

        Transform image_obj = transform.Find("Image");
        if (image_obj != null)
        {
           Image image_comp = image_obj.GetComponent<Image>();
            if (image_comp != null) {
                image_comp.sprite = weapon.weaponImage;
            
            }
        }
       

    }

    public override void OnButtonClick()
    {
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        gm.selectedUnit.HoveredWeapon(weapon);
        unit.attackRange = weapon.range;
        unit.attackType = (AttackType)Enum.Parse(typeof(AttackType), weapon.WeaponType); //影响攻击动画
        unit.attackAbility = weapon.attackAbility;
        unit.ShowAttackRange(unit.standOnTile);


        unit.CloseEquipmentList();
        this.unit.canAttack = true;
        Action attackButtonResetAction = new Action(RestButton);
        gm.actions.Push(attackButtonResetAction);
    }

    public override void OnButtonEnter()
    {
        gm.selectedUnit.HoveredWeapon(weapon);
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.attackRange = weapon.range;
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
        this.unit.playerAnimator.SetAnimationParam(this.unit, 0, -1);
        parentButton.OnButtonClick();
        this.unit.canExcute = false;
    }

}
