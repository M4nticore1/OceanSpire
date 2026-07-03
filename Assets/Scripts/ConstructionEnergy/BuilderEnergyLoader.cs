using UnityEngine;

public class BuilderEnergyLoader : WorldLoader
{
    [SerializeField] BuilderEnergyManager builderEnergyManager;

    protected override void Load(WorldData worldData)
    {
        var data = worldData?.BuilderEnergy;

        if (data != null) {
            builderEnergyManager.Init(data);
        }
        else {
            builderEnergyManager.Init();
        }
    }
}