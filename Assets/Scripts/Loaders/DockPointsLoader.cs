using UnityEngine;

public class DockPointsLoader : Loader
{
    [SerializeField] private DockPointsManager dockPointsManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.CitizenBoatDocks != null) {
            LoadDocks(dockPointsManager.CitizenBoatDocks.ToArray(), worldData?.CitizenBoatDocks);
        }
        else {
            InitDocks(dockPointsManager.CitizenBoatDocks.ToArray());
        }

        if (worldData != null && worldData.WandererBoatDocks != null) {
            LoadDocks(dockPointsManager.WandererDockPoints, worldData?.WandererBoatDocks);
        }
        else {
            InitDocks(dockPointsManager.WandererDockPoints);
        }

        if (worldData != null && worldData.RaiderBoatDocks != null) {
            LoadDocks(dockPointsManager.RaiderDockPoints, worldData?.RaiderBoatDocks);
        }
        else {
            InitDocks(dockPointsManager.RaiderDockPoints);
        }

        if (worldData != null && worldData.EvictBoatDocks != null) {
            LoadDocks(dockPointsManager.EvictDockPoints, worldData?.EvictBoatDocks);
        }
        else {
            InitDocks(dockPointsManager.EvictDockPoints);
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

    private void InitDocks(BoatDockPoint[] docks)
    {
        foreach (var dock in docks) {
            var dockData = new BoatDockData()
            {
                InstanceId = InstancesManager.Instance.GetNextInstanceId()
            };

            dock.Init(dockData);
        }
    }
}