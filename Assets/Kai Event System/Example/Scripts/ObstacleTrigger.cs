using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KAI;

public class ObstacleTrigger : MonoBehaviour
{
    [KAIEvent]
    public CustomEvent ColorChangeEvent;
    [KAIEvent]
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
