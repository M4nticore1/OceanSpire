using UnityEngine;

public class DockPointsLoader : Loader
{
    protected override void Load(WorldData data)
    {
        InitDocks(DockPointsManager.Instance.CitizenBoatDocks.ToArray(), data?.CitizenBoatDocks);
        InitDocks(DockPointsManager.Instance.WandererDockPoints, data?.WandererBoatDocks);
        InitDocks(DockPointsManager.Instance.RaiderDockPoints, data?.RaiderBoatDocks);
    }

    private void InitDocks(BoatDockPoint[] docks, BoatDockData[] data)
    {
        for (int i = 0; i < docks.Length; i++) {
            BoatDockData dockData = data != null && i < data.Length ? data[i] : GetBoatDockData();

            docks[i].Init(dockData);
        }
    }

    private BoatDockData GetBoatDockData()
    {
        int instanceId = InstancesManager.Instance.GetNextInstanceId();

        BoatDockData dockData = new BoatDockData()
        {
            InstanceId = instanceId,
        };

        return dockData;
    }
}