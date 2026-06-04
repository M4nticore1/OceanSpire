using UnityEngine;

public class WindLoader : Loader
{
    [SerializeField] private WindManager windManager;

    protected override void Load(WorldData worldData)
    {
        windManager.Init(worldData != null ? worldData.Wind : null);
    }
}