using System;
using System.Collections.Generic;
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
        Id = building.Definition.BuildingId;
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

    public static List<BuildingData> Create(IReadOnlyList<Building> buildings)
    {
        var buildingsData = new List<BuildingData>();

        foreach (var building in buildings) {
            if (!building) continue;

            var data = Create(building);
            if (data == null) continue;

            buildingsData.Add(data);
        }

        return buildingsData;
    }
}