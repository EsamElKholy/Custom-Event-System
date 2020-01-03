using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Events/Game Event Collection", fileName = "New Game Event Collection")]
public class GameEventCollection : CustomEvent
{
    public List<GameEvent> Events = new List<GameEvent>();

    private void OnEnable()
    {
        Type = CustomEventType.Game_Event_Collection;
    }

    public override void Raise()
    {
        for (int i = 0; i < Events.Count; i++)
        {
            if (Events[i])
            {
                Events[i].Raise();
            }
        }

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }

    public override IEnumerator RaiseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Raise();
    }

    public void RemoveEvent(GameEvent e)
    {
        if (e)
        {
            int index = -1;

            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].name.Equals(e.name))
                {
                    index = i;

                    break;
                }
            }

            if (index >= 0)
            {
                Events.RemoveAt(index);
            }
        }
    }
}
