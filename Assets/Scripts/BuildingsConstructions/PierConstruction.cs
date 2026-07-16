using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public List<BoatDockPoint> BoatDocks => boatDocks;

    protected override void OnEnable()
    {
        base.OnEnable();

        RegisterBoatDocks();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        UnregisterBoatDocks();
    }

    private void RegisterBoatDocks()
    {
        foreach (var dock in boatDocks) {
            dock.Init();
            DockPointsManager.Instance.RegisterCitizenDockPoint(dock);
        }
    }

    private void UnregisterBoatDocks()
    {
        foreach (var dock in boatDocks) {
            DockPointsManager.Instance.UnregisterCitizenDockPoint(dock);
        }
    }
}