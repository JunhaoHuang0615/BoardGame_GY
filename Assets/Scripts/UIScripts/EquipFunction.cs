

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum EquipmentType //武器，普通物品（伤药)
{
    ATTACK_EQUIPMENT,
    NORMAL_EQUIPMENT,
}
public abstract class EquipFunction : MonoBehaviour,IPointerEnterHandler,IPointerClickHandler
{

    public Sprite equipSprite; //装备的图片
    public string equipText;   //装备的名字
    public ButtonFunction parent_button;//EquipFunction的按钮，必须是ButtonFunction的按钮的子按钮 不需要
    [Multiline]
    public string description; //鼠标放到按钮上时，可以显示的文字
    public EquipmentType equipType;
    public string equipName; //所有的武器或者物品或者其他的Key
    public Action action; //代表是方法
    public Action<int> actionInt;
    public Unit unit;
    protected DataManager dataManager;
    protected ResourcesMananger resourcesMananger;
    protected GameManager gm;
    //public ButtonFunction buttonObj; //储存自己身上的实例
    public abstract void OnButtonEnter(); //鼠标放在按钮，或者预选的时候
    public abstract void OnButtonClick();
    public abstract void RestButton();
    public abstract void Init(); //按钮的初始化

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnButtonEnter();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnButtonClick();
    }
    private void Awake()
    {   
        gm = FindObjectOfType<GameManager>();
        dataManager = FindObjectOfType<DataManager>();
        resourcesMananger = FindObjectOfType<ResourcesMananger>();
    }
    private void OnDisable()
    {
        this.transform.position = new Vector3(0, 0, 0);
    }
    public void SetParrentButton( ButtonFunction parentButton)
    {
        this.parent_button = parentButton;
    }
}
