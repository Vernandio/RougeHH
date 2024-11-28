using UnityEngine;
using UnityEngine.Events;

public class EventChannelSO<T> : ScriptableObject
{
    public UnityEvent<T> OnEventRaised;

    public void RaiseEvent(T eventData)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised?.Invoke(eventData);
        }
    }
}
