using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using System.Threading;
using System;

public class CachingEventService : IEventService, IDisposable
{
    private IWebClient _webClient;
    private IEventStorage _eventStorage;
    private TimeSpan _cooldown;
    private string _url;

    private CancellationTokenSource _sendingCancellationTokenSource;

    private bool _isSendingTaskActive;

    public CachingEventService(
        IWebClient eventClient, 
        IEventStorage eventStorage,
        TimeSpan cooldown,
        string url)
    {
        _webClient = eventClient;
        _eventStorage = eventStorage;
        _cooldown = cooldown;
        _url = url;
    }
    public void Dispose()
    {
        _sendingCancellationTokenSource?.Dispose();
    }
    public void TrackEvent(string type, string data)
    {
        _eventStorage.AddEvent(type, data);

        StartSendingTaskIfNeeded();
    }
    /// <summary>
    /// Начинает отправку событий, если они есть в очереди
    /// </summary>
    /// <returns>true, если были не отправленные события</returns>
    public bool StartSendingTaskIfNeeded()
    {
        if (_isSendingTaskActive)
        {
            return false;
        }

        _isSendingTaskActive = true;

        _sendingCancellationTokenSource?.Cancel();
        _sendingCancellationTokenSource?.Dispose();
        _sendingCancellationTokenSource = new();
        SendEventsWithCooldown(_sendingCancellationTokenSource.Token).Forget();
        return true;
    }
    private async UniTask SendEventsWithCooldown(CancellationToken token)
    {
        _isSendingTaskActive = true;

        try
        {
            await TryToSendAvailableEventsAsync(token);
        }
        finally
        {
            _isSendingTaskActive = false;
        }
    }
    private async UniTask TryToSendAvailableEventsAsync(CancellationToken token)
    {
        if (!_eventStorage.HasPendingOrStoredEvents())
        {
            return;
        }

        bool isSent = false;

        while (!token.IsCancellationRequested && !isSent)
        {
            await UniTask.Delay(_cooldown, cancellationToken: token);

            _eventStorage.MoveEventsToPending();
            var json = _eventStorage.SerializePendingEventsToJson();
            isSent = await _webClient.SendJsonDataAsync(_url, json, token);

            if (isSent)
            {
                _eventStorage.ClearPendingEvents();
            }
        }
    }
}