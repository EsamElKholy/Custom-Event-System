using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KAI
{
    [ExecuteInEditMode]
    public class GameEventListener : MonoBehaviour
    {
        [EventAttribute] public CustomEvent Event;
        public UnityEvent response;

        public void OnEventRaised()
        {
            response.Invoke();
        }

        private void Awake()
        {
            if (!Event)
                return;
            Event.RegisterListener(this);
        }

        private void Update()
        {
            if (!Event)
                return;
            Event.RegisterListener(this);
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
    }
}