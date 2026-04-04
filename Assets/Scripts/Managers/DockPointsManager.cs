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

    private Dictionary<int, BoatDockPoint> dockPointsDict = new Dictionary<int, BoatDockPoint>();
    public Dictionary<int, BoatDockPoint> DockPointsDict => dockPointsDict;

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void RegisterDockPoint(BoatDockPoint dockPoint)
    {
        dockPointsDict.Add(dockPoint.InstanceId.id, dockPoint);
    }

    public void RegisterPierDockPoint(BoatDockPoint dockPoint)
    {
        pierDockPoints.Add(dockPoint);
    }
}