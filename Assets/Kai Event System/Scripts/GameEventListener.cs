using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KAI
{
    [ExecuteInEditMode]
    public class GameEventListener : MonoBehaviour
    {
        [KAIEvent] public CustomEvent Event;
        public UnityEvent response;

        private float Cooldown = 3f;
        private float Counter = 0;

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
    }
}