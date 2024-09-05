

using System;
using UnityEngine;


//Предположительный класс монобех если нет необходимости конфигурировать или выдавать зависимости извне
public class EventService : MonoBehaviour, IEventService
{
    //и это можно в SO
    [SerializeField]
    private float _cdSeconds;
    [SerializeField]
    private string _url;

    private CachingEventService _eventService;

    public void TrackEvent(string type, string data)
    {
        _eventService.TrackEvent(type, data);
    }

    private void Awake()
    {
        _eventService = new(
            new UnityWebClient(), 
            new PlayerPrefsEventStorage(), 
            TimeSpan.FromSeconds(_cdSeconds), 
            _url);
    }

    private void Start()
    {
        _eventService.StartSendingTaskIfNeeded();
    }
    private void OnDestroy()
    {
        _eventService.Dispose();
    }

}
