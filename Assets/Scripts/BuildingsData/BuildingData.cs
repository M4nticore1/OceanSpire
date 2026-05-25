using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int Id = 0;
    public int InstanceId = 0;
    public LevelData Level;
    public UpgradeData Upgrade;
    public ConstructionData Construction;
    public CraftingModuleData Crafting;

    protected void Fill(Building building)
    {
        Id = building.BuildingData.BuildingId;
        InstanceId = building.InstanceId.Id;
        Level = LevelData.Create(building.LevelComponent);
        Upgrade = UpgradeData.Create(building.UpgradeComponent);
        Construction = ConstructionData.Create(building.ConstructionComponent);
        Crafting = CraftingModuleData.Create(building.GetComponent<ProductionModule>());
    }

    public static BuildingData Create(Building building)
    {
        BuildingData data = new BuildingData();
        data.Fill(building);

        return data;
    }
}