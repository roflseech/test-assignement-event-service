

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TestEventStorage : IEventStorage
{
    private SerializableEventList _storedEvents;
    private SerializableEventList _pendingEvents;

    public TestEventStorage()
    {
        _storedEvents = new();
        _pendingEvents = new();
    }

    public void AddEvent(string type, string data)
    {
        _storedEvents.AddEvent(type, data);
    }

    public void ClearPendingEvents()
    {
        _pendingEvents.ClearEvents();
    }

    public bool HasPendingOrStoredEvents()
    {
        return _pendingEvents.HasEvents() || _storedEvents.HasEvents();
    }

    public void MoveEventsToPending()
    {
        _pendingEvents.TakeEventsFrom(_storedEvents);
    }

    public string SerializePendingEventsToJson()
    {
        return JsonUtility.ToJson(_pendingEvents);
    }
}
