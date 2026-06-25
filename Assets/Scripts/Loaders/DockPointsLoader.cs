using UnityEngine;

public class DockPointsLoader : WorldLoader
{
    [SerializeField] private DockPointsManager dockPointsManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.CitizenBoatDocks != null) {
            LoadDocks(dockPointsManager.CitizenBoatDocks.ToArray(), worldData?.CitizenBoatDocks);
        }

        if (worldData != null && worldData.WandererBoatDocks != null) {
            LoadDocks(dockPointsManager.WandererDockPoints, worldData?.WandererBoatDocks);
        }

        if (worldData != null && worldData.RaiderBoatDocks != null) {
            LoadDocks(dockPointsManager.RaiderDockPoints, worldData?.RaiderBoatDocks);
        }

        if (worldData != null && worldData.EvictBoatDocks != null) {
            LoadDocks(dockPointsManager.EvictDockPoints, worldData?.EvictBoatDocks);
        }
    }

    private void LoadDocks(BoatDockPoint[] docks, BoatDockData[] docksData)
    {
        for (int i = 0; i < docks.Length; i++) {
            if (i >= docksData.Length) return;

            var data = docksData[i];

            docks[i].Init(data);
        }
    }
}