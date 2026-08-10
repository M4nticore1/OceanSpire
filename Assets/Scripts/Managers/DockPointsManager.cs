using System;
using System.Collections.Generic;
using UnityEngine;

public class BoatDocksManager : MonoBehaviour
{
    public static BoatDocksManager Instance;

    [field: SerializeField] public List<BoatDockPoint> CitizenBoatDocks { get; private set; } = new();

    [SerializeField] private BoatDockPoint[] wandererDockPoints;
    public IReadOnlyList<BoatDockPoint> WandererDockPoints => wandererDockPoints;

    [SerializeField] private BoatDockPoint[] raiderDockPoints;
    public IReadOnlyList<BoatDockPoint> RaiderDockPoints => raiderDockPoints;

    [SerializeField] private BoatDockPoint[] evictDockPoints;
    public IReadOnlyList<BoatDockPoint> EvictDockPoints => evictDockPoints;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterCitizenDockPoint(BoatDockPoint dockPoint)
    {
        if (dockPoint == null) return;
        if (CitizenBoatDocks.Contains(dockPoint)) return;

        CitizenBoatDocks.Add(dockPoint);
    }

    public void UnregisterCitizenDockPoint(BoatDockPoint dockPoint)
    {
        if (dockPoint == null) return;
        if (!CitizenBoatDocks.Contains(dockPoint)) return;

        CitizenBoatDocks.Remove(dockPoint);
    }

    public BoatDockPoint GetCitizenBoatDock(int index)
    {
        return GetBoatDOckPoint(CitizenBoatDocks, index);
    }

    public BoatDockPoint GetWandererBoatDock(int index)
    {
        return GetBoatDOckPoint(wandererDockPoints, index);
    }

    public BoatDockPoint GetRaiderBoatDock(int index)
    {
        return GetBoatDOckPoint(raiderDockPoints, index);
    }

    public BoatDockPoint GetEvictBoatDock(int index)
    {
        return GetBoatDOckPoint(evictDockPoints, index);
    }

    private BoatDockPoint GetBoatDOckPoint(IReadOnlyList<BoatDockPoint> docksList, int index)
    {
        if (index < 0) {
            Debug.LogError($"[{nameof(BoatDocksManager)}] Index is less than 0!");
            return null;
        }
        if (docksList.Count <= index) {
            Debug.LogError($"[{nameof(BoatDocksManager)}] {nameof(docksList)} count is {docksList.Count} but index is greater or equal ({index})!");
            return null;
        }

        return docksList[index];
    }
}