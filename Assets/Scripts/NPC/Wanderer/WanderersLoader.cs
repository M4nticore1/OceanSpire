using UnityEngine;

public class WanderersLoader : WorldLoader
{
    [SerializeField] private WanderersManager wanderersManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.WanderersSystem != null) {
            LoadWanderers(worldData.WanderersSystem);
        }
        else {
            InitWanderers();
        }
    }

    private void LoadWanderers(WanderersData wanderersData)
    {
        wanderersManager.Init(wanderersData);
    }

    private void InitWanderers()
    {
        WanderersData wanderersData = new WanderersData() {
            Cooldown = (int)wanderersManager.CalculateRandomCooldown(),
            TimeSinceLastSpawn = 0,
        };

        wanderersManager.Init(wanderersData);
    }
}
