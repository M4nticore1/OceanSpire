using UnityEngine;

public class WanderersLoader : WorldLoader
{
    [SerializeField] private WanderersManager wanderersManager;

    protected override void Load(WorldData worldData)
    {
        var wanderersData = worldData?.WanderersSystem;

        if (wanderersData != null) {
            wanderersManager.Init(wanderersData);
        }
        else {
            wanderersManager.Init();
        }
    }
}
