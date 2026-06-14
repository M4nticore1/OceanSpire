using UnityEngine;

public class WindLoader : WorldLoader
{
    [SerializeField] private WindManager windManager;

    protected override void Load(WorldData worldData)
    {
        windManager.Init(worldData != null ? worldData.Wind : null);
    }
}