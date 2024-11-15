using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentList : MonoBehaviour
{
    public List<EquipFunction> equipButtons;
    // public ButtonListTransform parrent;
    public EquipListTransform equipListOnUnit;
    private ObjectPool obp;
    public Unit unit;

    private void Awake()
    {
        equipButtons = new List<EquipFunction>();
    }
    private void Start()
    {
        obp = FindObjectOfType<ObjectPool>();
    }
    public void GenerateButtons()
    {
        equipListOnUnit = obp.GetGameObject(GameObjectType.EQUIPLIST).GetComponent<EquipListTransform>();
        equipListOnUnit.transform.position = this.GetComponent<Unit>().transform.position;
       /* if(buttonListOnUnit.transform.position.y > 2f)
        {
            buttonListOnUnit.transform.position = new Vector3(transform.position.x,transform.position.y - 4f,transform.position.z);
        }*/
        foreach(var button in equipButtons)
        {
            button.Init();
        }
        equipListOnUnit.ShowButtons(equipButtons);
    }

    public void AddButton(GameObjectType type,string equipName, ButtonFunction parentbutton)
    {   
        //根据type拿到对应武器的Prefab或者是物品的prefab
        if (equipButtons == null)
        {
            equipButtons = new List<EquipFunction>();
        }
        //obp.GetGameObject(type) 拿到了装备button的Prefab
        //.GetComponent<EquipFunction>() 获得的是Prefab身上的AttackEquipment组件
        EquipFunction tempButton = obp.GetGameObject(type).GetComponent<EquipFunction>();
        tempButton.unit = this.unit;
        //根据名字去索引对应的属性
        tempButton.equipName = equipName;
        tempButton.SetParrentButton(parentbutton);
        equipButtons.Add(tempButton);


    }
    public void RemoveButton(EquipFunction button)
    {
        if (equipButtons.Contains(button))
        {
            equipButtons.Remove(button);
        }
    }

    public void CloseButtons()
    {
        if (equipButtons.Count >0)
        {
            foreach(var button in equipButtons)
            {   

                obp.ReturnGameObject(GameObjectType.ATTACK_EQUIPMENT,button.gameObject);
                
            }
            obp.ReturnGameObject(GameObjectType.EQUIPLIST, equipListOnUnit.gameObject);
            equipButtons.Clear();
        }
    }
}


