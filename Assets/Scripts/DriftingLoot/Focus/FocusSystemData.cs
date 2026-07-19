using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FocusSystemData
{
    public Guid[] focusedInstanceIds = Array.Empty<Guid>();

    public static FocusSystemData Default()
    {
        return new FocusSystemData();
    }

    public static FocusSystemData Create(FocusManager focusManager)
    {
        if (!focusManager) {
            Debug.LogError($"[{nameof(FocusSystemData)}] Focus Manager is not valid!");
            return null;
        }

        var instaceIds = new List<Guid>();
        var focusSystemData = new FocusSystemData();

        foreach (var component in focusManager.FocusComponentsList) {
            if (!component) continue;
            if (!component.InstanceId) continue;

            instaceIds.Add(component.InstanceId.GetGuid());
        }

        focusSystemData.focusedInstanceIds = instaceIds.ToArray();
        return focusSystemData;
    }
}