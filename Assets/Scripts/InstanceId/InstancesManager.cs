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

    private InstancesManager()
    {

    }

    private Dictionary<Guid, InstanceId> instances = new();

    public void RegisterInstance(InstanceId instance)
    {
        var guid = instance.GetGuid();
        if (instances.TryGetValue(guid, out var value)) return;

        instances.Add(guid, instance);
    }

    public void UnregisterInstance(InstanceId instance)
    {
        var guid = instance.GetGuid();
        if (!instances.TryGetValue(guid, out var value)) return;

        instances.Remove(instance.GetGuid());
    }

    public InstanceId GetInstance(Guid guid)
    {
        var instance = instances.GetValueOrDefault(guid);

        if (!instance) {
            Debug.Log($"Instance by Id {guid} does not exitst by!");
        }

        return instance;
    }
}