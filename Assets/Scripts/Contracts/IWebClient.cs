using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public interface IWebClient
{
    public UniTask<bool> SendJsonDataAsync(string url, string data, CancellationToken token);
}