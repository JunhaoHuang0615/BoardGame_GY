
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandButton : ButtonFunction
{
    public override void Init()
    {
        this.GetComponent<Image>().sprite = buttnSprite;
        this.buttonType = ButtonType.STAND;
    }

    public override void OnButtonClick()
    {
        this.unit.Stand();
    }

    public override void OnButtonEnter()
    {

    }

    public override void RestButton()
    {

    }

}
