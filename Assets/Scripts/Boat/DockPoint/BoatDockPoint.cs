using System.Collections.Generic;
using UnityEngine;

public class BoatDockPoint : MonoBehaviour
{
    public List<Boat> Boats { get; private set; } = new();

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Transform dockTransform;
    public Transform DockTransform => dockTransform;

    [SerializeField] private Transform entranceTransform;
    public Transform EntraceTransform => entranceTransform;

    public void Init(BoatDockData data)
    {
        instanceId.Register(data.InstanceId);
    }

    public void AddBoat(Boat boat)
    {
        if (Boats.Contains(boat)) {
            Debug.LogError("boat is already in the list");
            return;
        }

        Boats.Add(boat);
    }

    public void RemoveBoat(Boat boat)
    {
        if (!Boats.Contains(boat)) {
            Debug.LogError("boat is not in the list");
            return;
        }

        Boats.Remove(boat);
    }
}