﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public List<ButtonListTransform> buttonLists;



    private void Awake()
    {
        buttonLists = new List<ButtonListTransform>();
    }

    public bool isOnButtonList()
    {
        if(buttonLists.Count == 0)
        {
            return false;
        }
        else
        {
            foreach(var buttonlist in buttonLists)
            {
                if (buttonlist.CheckIsOnButtonList())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
