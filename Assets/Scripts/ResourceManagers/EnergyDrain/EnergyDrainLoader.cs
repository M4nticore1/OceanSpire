using UnityEngine;

public class EnergyDrainLoader : WorldLoader
{
    [SerializeField] private EnergyDrainManager energyDrainManager;

    protected override void Load(WorldData worldData)
    {
        var energyDrainData = worldData?.EnergyDrain;
        if (energyDrainData != null) {
            energyDrainManager.Init(energyDrainData);
        }
        else {
            energyDrainManager.Init();
        }
    }
}