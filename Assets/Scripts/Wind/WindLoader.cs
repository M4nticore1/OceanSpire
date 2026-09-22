using UnityEngine;

public class WindLoader : WorldLoader
{
    [SerializeField] private WindManager windManager;

    protected override void Load(WorldData worldData)
    {
        var windData = worldData?.Wind;

        if (windData != null) {
            windManager.Init(windData);
        }
        else {
            windManager.Init();
        }
    }
}