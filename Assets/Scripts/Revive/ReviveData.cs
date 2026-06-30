using System;
using UnityEngine;

[Serializable]
public class ReviveData
{
    public long? DieTime = null;

    public static ReviveData Default()
    {
        return new ReviveData();
    }

    public static ReviveData Create(ReviveComponent reviveComponent)
    {
        if (!reviveComponent) {
            Debug.LogError("reviveComponent is not valid");
            return null;
        }

        return new ReviveData()
        {
            DieTime = reviveComponent.DieTime,
        };
    }
}