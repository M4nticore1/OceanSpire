using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancesManager : MonoBehaviour
{
    public static InstancesManager Instance { get; private set; }
    private List<int> instanceIds = new List<int>();
    public IReadOnlyList<int> InstanceIds => instanceIds.AsReadOnly();

    private Dictionary<int, InstanceId> instances = new();

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
            Debug.Log($"Instance Id of {instance} is already registered as {id} by {InstancesManager.Instance.GetInstance(id)}!");
            instance.Init(GetNextInstanceId());
            return;
        }

        instanceIds.Add(id);
        instances.Add(id, instance);
    }

    public void UnregisterInstance(InstanceId instance)
    {
        instances.Remove(instance.Id);
    }

    public InstanceId GetInstance(int id)
    {
        var instance = instances.GetValueOrDefault(id);

        if (!instance) {
            Debug.Log($"Instance by Id {id} does not exitst!");
        }

        return instance;
    }

    public int GetNextInstanceId()
    {
        return instanceIds.Count > 0 ? instanceIds.Max() + 1 : 0;
    }
}