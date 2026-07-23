using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public List<BoatDockPoint> BoatDocks => boatDocks;

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

    private void RegisterBoatDocks()
    {
        foreach (var dock in boatDocks) {
            if (!dock) {
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
            if (!dock) {
                Debug.LogError($"[{nameof(PierConstruction)}] Dock is not valid at {name}!");
                continue;
            }

            if (boatDocksManager) {
                boatDocksManager.UnregisterCitizenDockPoint(dock);
            }
        }
    }
}