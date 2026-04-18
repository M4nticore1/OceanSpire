using UnityEngine;

public class DockPointsLoader : MonoBehaviour
{
    private void Start()
    {
        WorldData data = WorldSaveManager.Instance.currentSaveWorldData;

        if (data != null) {

        }
        else {
            foreach (var dock in DockPointsManager.instance.pierDockPoints) {
                dock.Init(GetBoatDockData());
            }

            foreach (var dock in DockPointsManager.instance.WandererDockPoints) {
                dock.Init(GetBoatDockData());
            }

            foreach (var dock in DockPointsManager.instance.RaiderDockPoints) {
                dock.Init(GetBoatDockData());
            }
        }
    }

    private BoatDockData GetBoatDockData()
    {
        int id = InstancesManager.instance.GetNextInstanceId();
        BoatDockData dockData = new BoatDockData(id);

        return dockData;
    }
}