
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AttackEquipment : EquipFunction
{

    public Weapon weapon;
    public override void Init()
    {   
        //要根据角色身上的武器列表来确定武器的图片
        // this.GetComponent<Image>().sprite = buttnSprite;
        this.equipType = EquipmentType.ATTACK_EQUIPMENT;
        //名字已经在创建的时候就已经获取到了：
        weapon = dataManager.GetWeapon(equipName);
        //图片给显示
        Transform image_obj = transform.Find("Image");
        
        if (image_obj != null)
        {
            // image_obj
            Image childImage = image_obj.GetComponent<Image>();

            if (childImage != null)
            {
                // ResourceManagerからSpriteをロード
                Sprite weaponSprite = resourcesMananger.LoadWeaponImage(weapon.ImagePath);

                if (weaponSprite != null)
                {
                    // 取得したImageコンポーネントにSpriteを設定
                    childImage.sprite = weaponSprite;
                    Debug.Log($"Sprite has been assigned to the Image component.");
                }
                else
                {
                    Debug.LogError("Failed to load sprite from ResourceManager.");
                }
            }
            else
            {
                Debug.LogError($"No Image component found .");
            }
        }

    }

    public override void OnButtonClick()
    {
         gm.ResetMoveableRange();
         gm.ResetMovePath();
         unit.selectEquip = weapon;
         unit.attackRange = weapon.Range;
         unit.attackType = (AttackType)Enum.Parse(typeof(AttackType), weapon.AttackType);
         unit.attackAbility = weapon.Attack;
         unit.ShowAttackRange(unit.standOnTile,true);
         unit.CloseAttackEquipList();
         this.unit.canAttack = true;
         Action attackButtonResetAction = new Action(RestButton);
         gm.actions.Push(attackButtonResetAction);
    }

    public override void OnButtonEnter()
    {
        unit.selectEquip = weapon;
        gm.ResetMoveableRange();
        gm.ResetMovePath();
        unit.attackRange = weapon.Range;
        unit.ShowAttackRange(unit.standOnTile);
    }

    // 选择了武器之后，按下右键，会撤回的操作
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
        this.unit.selected = true;
        this.unit.playerAnimator.SetAnimationParam(this.unit, 0, -1);
        this.parent_button.OnButtonClick();
        this.unit.canExcute = false;
    }

}
