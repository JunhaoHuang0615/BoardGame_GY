using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonListTransform : MonoBehaviour
{
    public Transform parentTransform; //为了作为子级
    public EventSystem eventSystem;
    public GraphicRaycaster raycaster;
    public UIManager um;
    public Transform center; //圆心的坐标
    public List<ButtonFunction> buttonOnThisList;
    public float radiaus; //旋转半径
    public float angle; //角度， 弧度制
    public float angularSpeed; //角速度
    public float gap;

    private void Awake()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        raycaster = this.GetComponent<GraphicRaycaster>();
        um = FindObjectOfType<UIManager>();
        um.buttonLists.Add(this);
        radiaus = 2.5f;
        buttonOnThisList = new List<ButtonFunction>();

    }

    public void ShowButtons(List<ButtonFunction> buttons)
    {
        //按钮间隔
        gap = (Mathf.PI * 2) / buttons.Count; //三个按钮，每个按钮间隔120， 两个按钮，180 ........
        int count = 0;
        foreach (var button in buttons)
        {
            button.transform.SetParent(parentTransform, false) ;
            button.GetComponent<RectTransform>().localPosition = new Vector2(center.GetComponent<RectTransform>().localPosition.x + Mathf.Cos(count * gap) * radiaus,
                                                                             center.GetComponent<RectTransform>().localPosition.y + Mathf.Sin(count * gap) * radiaus);
            buttonOnThisList.Add(button);
            count++;
        }
    }
    public bool CheckIsOnButtonList()
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.pressPosition = Input.mousePosition;
        eventData.position = Input.mousePosition;
        List<RaycastResult> list = new List<RaycastResult>();
        raycaster.Raycast(eventData , list);

        return list.Count > 0;
    }

    private void OnDisable()
    {
        parentTransform.DetachChildren();
        buttonOnThisList.Clear();
        angle = 0;
    }

    public void RotateButtonList()
    {

        int count = 0;
        foreach(var button in buttonOnThisList)
        {   
            button.GetComponent<RectTransform>().localPosition = new Vector2(center.GetComponent<RectTransform>().localPosition.x + Mathf.Cos(angle + count*gap) * radiaus,
                                                                             center.GetComponent<RectTransform>().localPosition.y + Mathf.Sin(angle + count * gap) * radiaus);
            count++;
        }
    }
    private void Update()
    {
        if(Input.mouseScrollDelta.y > 0)
        {
            angle += angularSpeed * Time.deltaTime;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            angle -= angularSpeed * Time.deltaTime;
        }
        RotateButtonList();
    }

}
