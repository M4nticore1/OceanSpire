using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public IReadOnlyList<BoatDockPoint> BoatDocks => boatDocks;

    BoatDocksManager boatDocksManager => BoatDocksManager.Instance;

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

    public BoatDockPoint GetBoatDock(int index)
    {
        if (index >= boatDocks.Count) {
            Debug.LogError($"{nameof(PierConstruction)} Index {index} is greater than dock's count at {name}!");
            return null;
        }

        var dock = boatDocks[index];
        if (dock == null) {
            Debug.LogError($"{nameof(PierConstruction)} Boat Dock is not valid at index {index} at {name}!");
            return null;
        }

        return boatDocks[index];
    }

    private void RegisterBoatDocks()
    {
        foreach (var dock in boatDocks) {
            if (dock == null) {
                Debug.LogError($"[{nameof(PierConstruction)}] Dock is not valid at {name}!");
            }

            dock.Init();

            if (boatDocksManager) {
                boatDocksManager.RegisterCitizenDockPoint(dock);
                continue;
            }
            else {
                Debug.LogError($"[{nameof(PierConstruction)}] Boat Docks Manager is not valid!");
            }
        }
    }

    private void UnregisterBoatDocks()
    {
        foreach (var dock in boatDocks) {
            if (dock == null) {
                Debug.LogError($"[{nameof(PierConstruction)}] Dock is not valid at {name}!");
                continue;
            }

            if (boatDocksManager) {
                boatDocksManager.UnregisterCitizenDockPoint(dock);
            }
        }
    }
}