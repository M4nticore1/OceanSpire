using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancesManager : MonoBehaviour
{
    public static InstancesManager instance { get; private set; }
    private List<int> instanceIds = new List<int>();
    public IReadOnlyList<int> InstanceIds => instanceIds.AsReadOnly();

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void AddInstanceId(int id)
    {
        instanceIds.Add(id);
    }

    public void RemoveInstanceId(int id)
    {
        instanceIds.Remove(id);
    }

    public int GetNextInstanceId()
    {        
        return instanceIds.Count > 0 ? instanceIds.Max() + 1 : 0;
    }
}