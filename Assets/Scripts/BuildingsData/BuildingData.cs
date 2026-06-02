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
        InstanceId = building.InstanceId.GetInstanceId();
        Level = LevelData.Create(building.LevelComponent);
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