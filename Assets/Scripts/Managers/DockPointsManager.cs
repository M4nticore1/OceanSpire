using System.Collections.Generic;
using UnityEngine;

public class BoatDocksManager : MonoBehaviour
{
    public static BoatDocksManager Instance;

    public List<BoatDockPoint> CitizenBoatDocks { get; private set; } = new List<BoatDockPoint>();

    [SerializeField] private BoatDockPoint[] wandererDockPoints;
    public BoatDockPoint[] WandererDockPoints => wandererDockPoints;

    [SerializeField] private BoatDockPoint[] raiderDockPoints;
    public BoatDockPoint[] RaiderDockPoints => raiderDockPoints;

    [SerializeField] private BoatDockPoint[] evictDockPoints;
    public BoatDockPoint[] EvictDockPoints => evictDockPoints;

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
        if (!dockPoint) return;
        if (CitizenBoatDocks.Contains(dockPoint)) return;

        CitizenBoatDocks.Add(dockPoint);
    }

    public void UnregisterCitizenDockPoint(BoatDockPoint dockPoint)
    {
        if (!dockPoint) return;
        if (!CitizenBoatDocks.Contains(dockPoint)) return;

        CitizenBoatDocks.Remove(dockPoint);
    }
}