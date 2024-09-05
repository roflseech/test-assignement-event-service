using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

public class UnityWebClient : IWebClient
{
    [Preserve]
    public UnityWebClient()
    {
    }

    public async UniTask<bool> SendJsonDataAsync(string url, string data, CancellationToken token)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                var operation = await request.SendWebRequest().WithCancellation(token);

                if (operation.result == UnityWebRequest.Result.Success)
                {
                    return request.responseCode == 200;
                }
                else
                {
                    Debug.LogError("Failed to send events: " + request.error);
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception during sending events: " + e.Message);
                return false;
            }
        }
    }
}