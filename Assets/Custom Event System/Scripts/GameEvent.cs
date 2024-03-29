﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Events/Game Event", fileName = "New Game Event")]
public class GameEvent : CustomEvent
{
    private void OnEnable()
    {
        Type = CustomEventType.Game_Event;
    }

    public override void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }

    public void DelayedRaise(float delay) 
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnDelayedEventRaised(delay);
        }
    }
}