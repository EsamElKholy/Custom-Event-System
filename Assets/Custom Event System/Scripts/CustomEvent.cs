﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CustomEventType
{
    Game_Event,
    Game_Event_Collection
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomEventAttribute : Attribute
{ }

public class CustomEvent : ScriptableObject
{
    [SerializeField] public List<GameEventListener> listeners = new List<GameEventListener>();

    public string Description;
    [HideInInspector]
    public CustomEventType Type;

    [HideInInspector]
    public bool UseInEditMode;

    public virtual void Raise()
    { }

    public virtual void RegisterListener(GameEventListener listener)
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] == null)
            {
                listeners.RemoveAt(i);

                i--;
            }
        }

        bool duplicate = false;
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listener && listeners[i] == listener)
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