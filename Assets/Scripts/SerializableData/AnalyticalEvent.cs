

using System;
using UnityEngine;

[Serializable]
public struct AnalyticalEvent
{
    [SerializeField]
    private string type;
    [SerializeField]
    private string data;

    public AnalyticalEvent(string type, string data)
    {
        this.type = type;
        this.data = data;
    }
}