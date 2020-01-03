using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CustomEventType
{
    Game_Event,
    Game_Event_Collection
}

[AttributeUsage(AttributeTargets.Field)]
public class EventAttribute : Attribute
{}

public class CustomEvent : ScriptableObject
{
    [SerializeField] public List<GameEventListener> listeners = new List<GameEventListener>();

    public string Description;
    [HideInInspector]
    public CustomEventType Type;

    public virtual void Raise()
    { }
    
    public virtual IEnumerator RaiseAfterDelay(float delay) { yield return null; }

    public virtual void RegisterListener(GameEventListener listener)
    {
        bool duplicate = false;
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] == listener)
            {
                duplicate = true;

                break;
            }
        }

        if (!duplicate)
        {
            listeners.Add(listener);
        }
    }

    public virtual void UnRegisterListener(GameEventListener listener)
    {
        listeners.Remove(listener);
    }
}
