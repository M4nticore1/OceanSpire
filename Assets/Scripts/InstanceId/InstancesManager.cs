using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancesManager : MonoBehaviour
{
    public static InstancesManager Instance { get; private set; }

    private List<int> instanceIds = new List<int>();
    public IReadOnlyList<int> InstanceIds => instanceIds.AsReadOnly();

    private Dictionary<int, InstanceId> instances = new();
    private int maxId = 0;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterInstance(InstanceId instance)
    {
        int id = GetNextInstanceId();
        RegisterInstance(id, instance);
    }

    public void RegisterInstance(int id, InstanceId instance)
    {
        if (instances.ContainsKey(id)) {
            Debug.Log($"Instance Id of {instance} is already registered as {id} by {GetInstance(id)}!");
            instance.Register(GetNextInstanceId());
            return;
        }

        instanceIds.Add(id);
        instances.Add(id, instance);

        maxId = id > maxId ? id : maxId;
    }

    public void UnregisterInstance(InstanceId instance)
    {
        instances.Remove(instance.GetId());
    }

    public InstanceId GetInstance(int id)
    {
        var instance = instances.GetValueOrDefault(id);

        if (!instance) {
            Debug.Log($"Instance by Id {id} does not exitst by!");
        }

        return instance;
    }

    public int GetNextInstanceId()
    {
        return instanceIds.Count > 0 ? instanceIds.Max() + 1 : 0;
    }
}