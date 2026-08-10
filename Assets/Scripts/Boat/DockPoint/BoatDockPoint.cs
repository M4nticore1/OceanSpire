using System;
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

    public void Init()
    {
        Init(BoatDockData.Default());
    }

    public void Init(BoatDockData boatDockData)
    {
        if (boatDockData == null) {
            Debug.LogError($"[{nameof(BoatDockPoint)}] Boat Dock Data is not valid!");
            Init();
            return;
        }

        if (boatDockData.InstanceId == Guid.Empty) {
            Debug.LogError($"[{nameof(BoatDockPoint)}] Guid is empty!");
            Init();
            return;
        }

        instanceId.SetGuid(boatDockData.InstanceId);
    }

    public void AddBoat(Boat boat)
    {
        if (boat == null) return;

        if (Boats.Contains(boat)) {
            Debug.LogError("Boat is already in the list");
            return;
        }

        Boats.Add(boat);
    }

    public void RemoveBoat(Boat boat)
    {
        if (boat == null) return;

        if (!Boats.Contains(boat)) {
            Debug.LogError("Boat is not in the list");
            return;
        }

        Boats.Remove(boat);
    }
}