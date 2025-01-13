

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EquipType
{
    ATTACK_EQUIP,
    COMMON_EQUIP,
}
public abstract class EquipButtonFunction : MonoBehaviour,IPointerEnterHandler,IPointerClickHandler
{
    //这个类确定控制角色Button的功能
    //public Sprite buttnSprite;
    public string euipText; // 显示按钮的文字
    [Multiline]
    public string description; //鼠标放到按钮上时，可以显示的文字
    public EquipType buttonType;
    public Action action; //代表是方法
    public Action<int> actionInt;
    public Unit unit;
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
    }
    private void OnDisable()
    {
        this.transform.position = new Vector3(0, 0, 0);
    }
}
