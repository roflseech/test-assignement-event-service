using UnityEngine;
using UnityEngine.Scripting;

public class PlayerPrefsEventStorage : IEventStorage
{
    private SerializableEventList _pendingEvents;
    private SerializableEventList _storedEvents;
    private bool _loaded;

    [Preserve]
    public PlayerPrefsEventStorage()
    {
    }

    public void AddEvent(string type, string data)
    {
        LoadIfNeeded();
        _storedEvents.AddEvent(type, data);
        Save();
    }
    public void MoveEventsToPending()
    {
        LoadIfNeeded();
        _pendingEvents.TakeEventsFrom(_storedEvents);
        Save();
    }
    public string SerializePendingEventsToJson()
    {
        LoadIfNeeded();
        return SerializeEventList(_pendingEvents);
    }
    public void ClearPendingEvents()
    {
        LoadIfNeeded();
        _pendingEvents.ClearEvents();
        Save();
    }
    public bool HasPendingOrStoredEvents()
    {
        LoadIfNeeded();
        return _pendingEvents.HasEvents() || _storedEvents.HasEvents();
    }
    private void Save()
    {
        LoadIfNeeded();
        SaveEventsToPlayerPrefs(_pendingEvents, SaveKeysConstants.PendingEvents);
        SaveEventsToPlayerPrefs(_storedEvents, SaveKeysConstants.StoredEvents);
        PlayerPrefs.Save();
    }
    private void LoadIfNeeded()
    {
        if (_loaded) return;
        
        Load();
        _loaded = true;
    }
    private void Load()
    {
        LoadEvents(ref _pendingEvents, SaveKeysConstants.PendingEvents);
        LoadEvents(ref _storedEvents, SaveKeysConstants.StoredEvents);
    }
    private void SaveEventsToPlayerPrefs(SerializableEventList list, string key)
    {
        var json = SerializeEventList(list);
        PlayerPrefs.SetString(key, json);
    }
    private void LoadEvents(ref SerializableEventList eventList, string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            eventList = new();
            return;
        }

        var json = PlayerPrefs.GetString(key);
        eventList = DeserializeEventList(json);
    }
    private string SerializeEventList(SerializableEventList eventList)
    {
        return JsonUtility.ToJson(eventList);
    }
    private SerializableEventList DeserializeEventList(string json)
    {
        return JsonUtility.FromJson<SerializableEventList>(json);
    }

}