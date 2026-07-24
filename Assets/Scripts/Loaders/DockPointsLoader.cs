using System.Collections.Generic;
using UnityEngine;

public class DockPointsLoader : WorldLoader
{
    [SerializeField] private BoatDocksManager dockPointsManager;

    protected override void Load(WorldData worldData)
    {
        if (dockPointsManager == null) {
            Debug.LogError($"[{nameof(DockPointsLoader)}] DockPointsManager reference is missing!");
            return;
        }

        var citizenDocksData = worldData?.CitizenBoatDocks;
        var citizenDocks = dockPointsManager.CitizenBoatDocks;

        if (citizenDocksData != null && citizenDocks != null) {
            LoadDocks(citizenDocks, citizenDocksData);
        }
        else {
            InitDocks(citizenDocks);
        }

        var wandererDocksData = worldData?.WandererBoatDocks;
        var wandererDocks = dockPointsManager.WandererDockPoints;

        if (wandererDocksData != null && wandererDocks != null) {
            LoadDocks(wandererDocks, wandererDocksData);
        }
        else {
            InitDocks(wandererDocks);
        }

        var raiderDocksData = worldData?.RaiderBoatDocks;
        var raiderDocks = dockPointsManager.RaiderDockPoints;

        if (raiderDocksData != null && raiderDocks != null) {
            LoadDocks(raiderDocks, raiderDocksData);
        }
        else {
            InitDocks(raiderDocks);
        }

        var evictDocksData = worldData?.EvictBoatDocks;
        var evictDocks = dockPointsManager.EvictDockPoints;

        if (evictDocksData != null && evictDocks != null) {
            LoadDocks(evictDocks, evictDocksData);
        }
        else {
            InitDocks(evictDocks);
        }
    }

    private void InitDocks(IReadOnlyList<BoatDockPoint> docks)
    {
        foreach (var dock in docks) {
            dock.Init();
        }
    }

    private void LoadDocks(IReadOnlyList<BoatDockPoint> docks, List<BoatDockData> docksData)
    {
        for (int i = 0; i < docks.Count; i++) {
            if (i >= docksData.Count) break;

            var data = docksData[i];
            if (data == null) {
                Debug.LogError($"[{nameof(DockPointsLoader)}] Dock data at index {i} is null! Create default.");
                data = BoatDockData.Default();
            }

            if (docks[i] != null) {
                docks[i].Init(data);
            }
        }
    }
}