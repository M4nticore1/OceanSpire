using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public int Level = 1;
    public UpgradeData Upgrade = UpgradeData.Default();
    public ConstructionData Construction = ConstructionData.Default();
    public CraftingModuleData Crafting = CraftingModuleData.Default();

    protected void Fill(Building building)
    {
        Id = building.BuildingData.BuildingId;
        InstanceId = building.InstanceId.GetGuid();
        Level = building.LevelComponent.Level;
        Upgrade = UpgradeData.Create(building.UpgradeComponent);
        Construction = ConstructionData.Create(building.ConstructionComponent);
        Crafting = CraftingModuleData.Create(building.GetComponent<CraftingModule>());
    }

    public static BuildingData Create(Building building)
    {
        var data = new BuildingData();
        data.Fill(building);

        return data;
    }

    public static BuildingData[] Create(Building[] buildings)
    {
        var buildingsData = new BuildingData[buildings.Length];

        for (int i = 0; i < buildings.Length; i++) {
            buildingsData[i] = Create(buildings[i]);
        }

        return buildingsData;
    }
}