using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancesManager : MonoBehaviour
{
    public static InstancesManager instance { get; private set; }
    private List<int> instanceIds = new List<int>();
    public IReadOnlyList<int> InstanceIds => instanceIds.AsReadOnly();

    private Dictionary<int, InstanceId> instances = new();

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void RegisterInstance(InstanceId instance)
    {
        int id = GetNextInstanceId();
        RegisterInstance(instance, id);
    }

    public void RegisterInstance(InstanceId instance, int id)
    {
        instanceIds.Add(id);
        instances.Add(id, instance);
    }

    public void UnregisterInstance(InstanceId instance)
    {
        instanceIds.Remove(instance.id);
        instances.Remove(instance.id);
    }

    public bool TryGetInstance(int id, out InstanceId instanceId)
    {
        instanceId = GetInstance(id);

        return instanceId;
    }

    public InstanceId GetInstance(int id)
    {
        return instances.GetValueOrDefault(id);
    }

    public int GetNextInstanceId()
    {
        return instanceIds.Count > 0 ? instanceIds.Max() + 1 : 0;
    }
}