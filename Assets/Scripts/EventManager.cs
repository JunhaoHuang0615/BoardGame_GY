using System;
using System.Collections.Generic;

public static class EventManager
{
    // key: 事件名, value: 委托
    private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    // 订阅事件
    public static void AddListener<T>(string eventName, Action<T> listener)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null;
        }

        // 防止类型不匹配
        if (eventTable[eventName] != null && eventTable[eventName].GetType() != typeof(Action<T>))
        {
            throw new Exception($"事件 {eventName} 的类型不匹配，已有类型 {eventTable[eventName].GetType()}，尝试添加 {typeof(Action<T>)}");
        }

        eventTable[eventName] = (Action<T>)eventTable[eventName] + listener;
    }

    // 移除事件
    public static void RemoveListener<T>(string eventName, Action<T> listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action<T>)eventTable[eventName] - listener;
        }
    }

    // 触发事件
    public static void TriggerEvent<T>(string eventName, T arg)
    {
        if (eventTable.ContainsKey(eventName))
        {
            var callback = eventTable[eventName] as Action<T>;
            callback?.Invoke(arg);
        }
    }
}