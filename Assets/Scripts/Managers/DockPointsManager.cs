using System.Collections.Generic;
using UnityEngine;

public class DockPointsManager : MonoBehaviour
{
    public static DockPointsManager Instance;

    public List<BoatDockPoint> CitizenBoatDocks { get; private set; } = new List<BoatDockPoint>();

    [SerializeField] private BoatDockPoint[] wandererDockPoints;
    public BoatDockPoint[] WandererDockPoints => wandererDockPoints;

    [SerializeField] private BoatDockPoint[] raiderDockPoints;
    public BoatDockPoint[] RaiderDockPoints => raiderDockPoints;

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
        CitizenBoatDocks.Add(dockPoint);
    }

    public void UnregisterCitizenDockPoint(BoatDockPoint dockPoint)
    {
        CitizenBoatDocks.Remove(dockPoint);
    }
}