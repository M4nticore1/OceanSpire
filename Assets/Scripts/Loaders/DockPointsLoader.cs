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
                dock.Init();
            }

            foreach (var dock in DockPointsManager.instance.WandererDockPoints) {
                dock.Init();
            }

            foreach (var dock in DockPointsManager.instance.RaiderDockPoints) {
                dock.Init();
            }
        }
    }
}
