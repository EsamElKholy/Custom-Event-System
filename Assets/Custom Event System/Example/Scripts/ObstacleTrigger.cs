using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    [CustomEventAttribute]
    public CustomEvent ColorChangeEvent;
    [CustomEventAttribute]
    public CustomEvent ColorResetEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (ColorChangeEvent)
        {
            ColorChangeEvent.Raise();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ColorResetEvent)
        {
            ColorResetEvent.Raise();
        }
    }
}
