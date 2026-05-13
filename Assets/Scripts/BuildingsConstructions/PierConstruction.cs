using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public List<BoatDockPoint> BoatDocks { get { return new List<BoatDockPoint>(boatDocks); } }

    protected override void Awake()
    {
        base.Awake();

        foreach (var dockPoint in boatDocks) {
            DockPointsManager.Instance.RegisterPierDockPoint(dockPoint);
        }
    }
}