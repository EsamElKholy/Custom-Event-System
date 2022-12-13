using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class GameEventListener : MonoBehaviour
{
    [CustomEventAttribute] public CustomEvent Event;
    public UnityEvent response;

    private float Cooldown = 3f;
    private float Counter = 0;

    public void OnEventRaised()
    {
        response.Invoke();
    }

    public void OnDelayedEventRaised(float delay) 
    {
        DelayedInvoke(this.GetCancellationTokenOnDestroy(), delay).Forget();
    }

    private void Awake()
    {
        if (!Event)
            return;
        Event.RegisterListener(this);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (!Event)
                return;

            if (Counter >= Cooldown)
            {
                Counter = 0;
                Event.RegisterListener(this);
            }

            Counter += Time.deltaTime;
        }
    }

    void OnEnable()
    {
        if (!Event)
            return;
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (!Event)
            return;
        Event.UnRegisterListener(this);
    }

    private async UniTaskVoid DelayedInvoke(CancellationToken cancellationToken, float delay) 
    {
        await UniTask.Delay((int)(delay * 1000f));
        response.Invoke();
    }
}