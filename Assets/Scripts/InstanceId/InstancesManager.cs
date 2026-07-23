using System;
using System.Collections.Generic;
using UnityEngine;

public class InstancesManager
{
    private static InstancesManager instance;
    public static InstancesManager Instance
    {
        get {
            if (instance == null) {
                instance = new InstancesManager();
            }

            return instance;
        }
    }

    private Dictionary<Guid, InstanceId> instances = new();

    private InstancesManager()
    {

    }

    public void RegisterInstance(InstanceId instance)
    {
        if (!instance) return;

        var guid = instance.GetGuid();
        if (instances.TryGetValue(guid, out var value)) return;

        instances.Add(guid, instance);
    }

    public void UnregisterInstance(InstanceId instance)
    {
        if (!instance) return;

        var guid = instance.GetGuid();
        if (!instances.TryGetValue(guid, out var value)) return;

        instances.Remove(instance.GetGuid());
    }

    public InstanceId GetInstance(Guid guid)
    {
        if (guid == Guid.Empty) {
            Debug.LogError($"[{nameof(InstancesManager)}] Guid to get instance is empty!");
            return null;
        }

        var instance = instances.GetValueOrDefault(guid);
        if (!instance) {
            Debug.LogError($"[{nameof(InstancesManager)}] Instance by Id {guid} does not exitst!");
            return null;
        }

        return instance;
    }
}