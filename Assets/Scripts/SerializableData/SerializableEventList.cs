using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SerializableEventList
{
    [SerializeField]
    private List<AnalyticalEvent> events;

    public SerializableEventList()
    {
        events = new();
    }
    public void AddEvent(string type, string data)
    {
        events.Add(new AnalyticalEvent(type, data));
    }
    public void TakeEventsFrom(SerializableEventList eventList)
    {
        events.AddRange(eventList.events);
        eventList.ClearEvents();
    }
    public void ClearEvents()
    {
        events.Clear();
    }
    public bool HasEvents()
    {
        return events.Count > 0;
    }

    public bool IsSimilar(SerializableEventList other)
    {
        if (other.events.Count != events.Count) return false;

        for (int i = 0; i < events.Count; i++)
        {
            if (!events[i].Equals(other.events[i])) return false;
        }

        return true;
    }
}