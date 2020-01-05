using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KAI
{
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
                //Debug.Log(listeners[i].gameObject.name);
                listeners[i].OnEventRaised();
            }
        }

        public override IEnumerator RaiseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Raise();
        }
    }
}