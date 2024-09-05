using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[TestFixture]
public class BaseTests
{
    [SetUp]
    public void SetUp()
    {

    }

    [TearDown]
    public void TearDown()
    {

    }

    [Test]
    public void SerializableData_ToFromJson()
    {
        var data = new SerializableEventList();

        data.AddEvent("test1", "test2");

        var json = JsonUtility.ToJson(data);
        var newData = JsonUtility.FromJson<SerializableEventList>(json);

        Assert.IsTrue(newData.IsSimilar(data));
    }

    [Test]
    public void SerializableData_ToJson()
    {
        var data = new SerializableEventList();

        string testJson = @"
{
    ""events"": [
        {
            ""type"": ""test1"",
            ""data"": ""test2""
        }
    ]
}";
        data.AddEvent("test1", "test2");

        var newData = JsonUtility.FromJson<SerializableEventList>(testJson);

        Assert.IsTrue(newData.IsSimilar(data));
    }
    [Test]
    public void SerializableData_ToJsonWrong()
    {
        var data = new SerializableEventList();

        string testJson = @"
{
    ""events"": [
        {
            ""type"": ""test1"",
            ""data"": ""test3""
        }
    ]
}";
        data.AddEvent("test1", "test2");

        var newData = JsonUtility.FromJson<SerializableEventList>(testJson);

        Assert.IsFalse(newData.IsSimilar(data));
    }

    [Test]
    public void TestEventStorage_Add_Clear()
    {
        var data = new SerializableEventList();
        data.AddEvent("test1", "test2");

        var eventStorage = new TestEventStorage();

        eventStorage.AddEvent("test1", "test2");
        eventStorage.MoveEventsToPending();

        var extractedJson = eventStorage.SerializePendingEventsToJson();
        var extractedData = JsonUtility.FromJson<SerializableEventList>(extractedJson);

        Assert.IsTrue(extractedData.IsSimilar(data));

        eventStorage.ClearPendingEvents();

        Assert.IsFalse(eventStorage.HasPendingOrStoredEvents());
    }

    [Test]
    public async Task TestWebClient_EventsSent()
    {
        var webClient = new TestWebClient(0.5f);

        var data1 = new SerializableEventList();
        data1.AddEvent("test1", "test2");

        var data2 = new SerializableEventList();
        data2.AddEvent("test3", "test4");

        CancellationTokenSource c = new();
        await webClient.SendJsonDataAsync("url", JsonUtility.ToJson(data1), c.Token);
        await webClient.SendJsonDataAsync("url", JsonUtility.ToJson(data2), c.Token);
        c.Dispose();

        var extractedData1 = JsonUtility.FromJson<SerializableEventList>(webClient.ReceivedMessages[0]);
        var extractedData2 = JsonUtility.FromJson<SerializableEventList>(webClient.ReceivedMessages[1]);

        Assert.IsTrue(extractedData1.IsSimilar(data1));
        Assert.IsTrue(extractedData2.IsSimilar(data2));
    }


    [Test]
    public async Task EventService_TrackSeparate()
    {
        var data1 = new SerializableEventList();
        data1.AddEvent("test1", "test2");
        data1.AddEvent("test3", "test4");

        var data2 = new SerializableEventList();
        data2.AddEvent("test5", "test6");

        var webClient = new TestWebClient(0.1f);
        var eventStorage = new TestEventStorage();
        var eventService = new CachingEventService(
            webClient,
            eventStorage,
            TimeSpan.FromSeconds(0.5f), 
            "aa");

        //придет в одно кд
        eventService.TrackEvent("test1", "test2");
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        eventService.TrackEvent("test3", "test4");

        //после кд
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
        eventService.TrackEvent("test5", "test6");

        //дождемся когда придет последний
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

        var extractedData1 = JsonUtility.FromJson<SerializableEventList>(webClient.ReceivedMessages[0]);
        var extractedData2 = JsonUtility.FromJson<SerializableEventList>(webClient.ReceivedMessages[1]);

        Assert.IsTrue(extractedData1.IsSimilar(data1));
        Assert.IsTrue(extractedData2.IsSimilar(data2));

        eventService.Dispose();
    }

    [Test]
    public async Task EventService_TrackSendFail()
    {
        var data1 = new SerializableEventList();
        data1.AddEvent("test1", "test2");
        data1.AddEvent("test3", "test4");


        var webClient = new TestWebClient(0.1f);
        var eventStorage = new TestEventStorage();

        var eventService = new CachingEventService(
            webClient,
            eventStorage,
            TimeSpan.FromSeconds(0.5f),
            "aa");

        //первая попытка отправки провалится
        webClient.FailNextDataReceive();

        eventService.TrackEvent("test1", "test2");
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        eventService.TrackEvent("test3", "test4");

        //ожидаем попытку отправыки
        await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
        //попытка отправить данные не прошла
        //нужно првоерить, что в хранилище ивенты на месте
        Assert.IsTrue(eventStorage.HasPendingOrStoredEvents());

        await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
        //должна была быть вторая попытка

        var extractedData1 = JsonUtility.FromJson<SerializableEventList>(webClient.ReceivedMessages[0]);

        Assert.IsTrue(extractedData1.IsSimilar(data1));
        Assert.IsFalse(eventStorage.HasPendingOrStoredEvents());

        eventService.Dispose();
    }
}