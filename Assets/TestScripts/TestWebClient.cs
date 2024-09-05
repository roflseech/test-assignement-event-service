

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public class TestWebClient : IWebClient
{
    private List<string> _receivedMessages;
    private float _delaySeconds;
    private bool _failNextDataReceive;

    public IReadOnlyList<string> ReceivedMessages => _receivedMessages;

    public TestWebClient(float delaySeconds)
    {
        _receivedMessages = new();
        _delaySeconds = delaySeconds;
    }

    public async UniTask<bool> SendJsonDataAsync(string url, string data, CancellationToken token)
    {
        bool shouldFail = _failNextDataReceive;
        _failNextDataReceive = false;

        await UniTask.Delay(TimeSpan.FromSeconds(_delaySeconds), cancellationToken: token);
        _receivedMessages.Add(data);

        return !shouldFail;
    }

    public void FailNextDataReceive()
    {
        _failNextDataReceive = true;
    }
}
