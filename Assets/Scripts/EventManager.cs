using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class EventManager
{
    private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    //注册事件
    public static void AddEventListener<T>(string eventName, Action<T> listener)
    {   
        //如果没有被注册过
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null; //把事件初始化

        }
        // 防止类型不匹配
        if (eventTable[eventName] != null && eventTable[eventName].GetType() != typeof(Action<T>))
        {
            throw new Exception("事件类型不匹配");
        }

        //加入监听
        eventTable[eventName] = (Action<T>)eventTable[eventName] + listener;
    }

    // 移除对某个事件的监听
    public static void RemoveEventListener<T>(string eventName, Action<T> listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action<T>)eventTable[eventName] - listener;
        }
    }

    //触发事件: 只接受一个参数的传递
    public static void TriggerEvent<T>(string eventName, T arg)
    {
        if (eventTable.ContainsKey(eventName))
        {
            var callback = eventTable[eventName] as Action<T>;
            callback?.Invoke(arg);
        }
    }


    // 不需要传递参数的情况
    public static void AddEventListener(string eventName, Action listener)
    {
        //如果没有被注册过
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null; //把事件初始化

        }
        // 防止类型不匹配
        if (eventTable[eventName] != null && eventTable[eventName].GetType() != typeof(Action))
        {
            throw new Exception("事件类型不匹配");
        }

        //加入监听
        eventTable[eventName] = (Action)eventTable[eventName] + listener;
    }

    // 移除对某个事件的监听
    public static void RemoveEventListener(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action)eventTable[eventName] - listener;
        }
    }

    //触发事件: 只接受一个参数的传递
    public static void TriggerEvent(string eventName)
    {
        if (eventTable.ContainsKey(eventName))
        {
            var callback = eventTable[eventName] as Action;
            callback?.Invoke();
        }
    }
}
