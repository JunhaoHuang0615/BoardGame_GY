using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipButtonList : MonoBehaviour
{
    public List<EquipButtonFunction> equipButtons;
    public EquipButtonListTransform equipButtonListOnUnit;
    private ObjectPool obp;
    public Unit unit;

    private void Awake()
    {
        equipButtons = new List<EquipButtonFunction>();
    }
    private void Start()
    {
        obp = FindObjectOfType<ObjectPool>();
    }
    public void GenerateButtons()
    {
        equipButtonListOnUnit = obp.GetGameObject(GameObjectType.EQUIPLIST).GetComponent<EquipButtonListTransform>();
        equipButtonListOnUnit.transform.position = this.GetComponent<Unit>().transform.position;
       /* if(buttonListOnUnit.transform.position.y > 2f)
        {
            buttonListOnUnit.transform.position = new Vector3(transform.position.x,transform.position.y - 4f,transform.position.z);
        }*/
        foreach(var button in equipButtons)
        {
            button.Init();
        }
        equipButtonListOnUnit.ShowButtons(equipButtons);
    }

    public void AddButton(GameObjectType type)
    {   
        if (equipButtons == null)
        {
            equipButtons = new List<EquipButtonFunction>();
        }
        EquipButtonFunction tempButton = obp.GetGameObject(type).GetComponent<EquipButtonFunction>();
        tempButton.unit = this.unit;
        equipButtons.Add(tempButton);


    }
    public void RemoveButton(EquipButtonFunction button)
    {
        if (equipButtons.Contains(button))
        {
            equipButtons.Remove(button);
        }
    }

    public void CloseButtons()
    {
/*        if (buttons.Count >0)
        {
            foreach(var button in buttons)
            {   
                if(button.buttonType == ButtonType.MOVE)
                {
                    obp.ReturnGameObject(GameObjectType.MOVEBUTTON,button.gameObject);
                }
                if (button.buttonType == ButtonType.ATTACK)
                {
                    obp.ReturnGameObject(GameObjectType.ATTACKBUTTON, button.gameObject);
                }
                if (button.buttonType == ButtonType.STAND)
                {
                    obp.ReturnGameObject(GameObjectType.STANDBUTTON, button.gameObject);
                }
            }
            obp.ReturnGameObject(GameObjectType.BUTTONLIST, buttonListOnUnit.gameObject);
            buttons.Clear();
        }*/
    }
}


