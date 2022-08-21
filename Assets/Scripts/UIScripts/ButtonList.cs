using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonList : MonoBehaviour
{
    public List<ButtonFunction> buttons;
    public ButtonListTransform buttonListOnUnit;
    private ObjectPool obp;
    public Unit unit;

    private void Awake()
    {
        buttons = new List<ButtonFunction>();
    }
    private void Start()
    {
        obp = FindObjectOfType<ObjectPool>();
    }
    public void GenerateButtons()
    {
        buttonListOnUnit = obp.GetGameObject(GameObjectType.BUTTONLIST).GetComponent<ButtonListTransform>();
        buttonListOnUnit.transform.position = this.GetComponent<Unit>().transform.position;
       /* if(buttonListOnUnit.transform.position.y > 2f)
        {
            buttonListOnUnit.transform.position = new Vector3(transform.position.x,transform.position.y - 4f,transform.position.z);
        }*/
        foreach(var button in buttons)
        {
            button.Init();
        }
        buttonListOnUnit.ShowButtons(buttons);
    }

    public void AddButton(GameObjectType type)
    {   
        if (buttons == null)
        {
            buttons = new List<ButtonFunction>();
        }
        ButtonFunction tempButton = obp.GetGameObject(type).GetComponent<ButtonFunction>();
        tempButton.unit = this.unit;
        buttons.Add(tempButton);


    }
    public void RemoveButton(ButtonFunction button)
    {
        if (buttons.Contains(button))
        {
            buttons.Remove(button);
        }
    }

    public void CloseButtons()
    {
        if (buttons.Count >0)
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
        }
    }
}


