using UnityEngine;

public class FoodDrainLoader : WorldLoader
{
    [SerializeField] private FoodDrainManager foodDrainManager;

    protected override void Load(WorldData worldData)
    {
        var foodDrainData = worldData?.FoodDrain;
        if (foodDrainData != null) {
            foodDrainManager.Init(foodDrainData);
        }
        else {
            foodDrainManager.Init();
        }
    }
}