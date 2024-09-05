using System.Collections.Generic;

public interface IEventStorage
{
    public void AddEvent(string type, string data);
    public void MoveEventsToPending();
    public string SerializePendingEventsToJson();
    public void ClearPendingEvents();
    public bool HasPendingOrStoredEvents();
}