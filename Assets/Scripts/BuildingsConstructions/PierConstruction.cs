using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public List<BoatDockPoint> BoatDocks { get { return new List<BoatDockPoint>(boatDocks); } }

    public override void Init(Building ownedBuilding)
    {
        base.Init(ownedBuilding);

        foreach (var dockPoint in boatDocks) {
            DockPointsManager.instance.RegisterPierDockPoint(dockPoint);
        }
    }
}
