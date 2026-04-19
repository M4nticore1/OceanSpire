using System.Collections.Generic;
using UnityEngine;

public class DockPointsManager : MonoBehaviour
{
    public static DockPointsManager instance;

    public List<BoatDockPoint> pierDockPoints { get; private set; } = new List<BoatDockPoint>();

    [SerializeField] private BoatDockPoint[] wandererDockPoints;
    public BoatDockPoint[] WandererDockPoints => wandererDockPoints;

    [SerializeField] private BoatDockPoint[] raiderDockPoints;
    public BoatDockPoint[] RaiderDockPoints => raiderDockPoints;

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void RegisterPierDockPoint(BoatDockPoint dockPoint)
    {
        pierDockPoints.Add(dockPoint);
    }
}