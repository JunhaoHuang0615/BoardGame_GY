
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : EquipButtonFunction
{
    public override void Init()
    {
        //this.GetComponent<Image>().sprite = buttnSprite;
        this.buttonType = EquipType.ATTACK_EQUIP;
    }

    public override void OnButtonClick()
    {
        
    }

    public override void OnButtonEnter()
    {

    }

    public override void RestButton()
    {

    }

}
