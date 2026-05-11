using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int Id = 0;
    public int InstanceId = 0;
    public int Level = 1;
    public ConstructionData Construction;
    public CraftingModuleData Crafting;

    protected void Fill(Building building)
    {
        Id = building.BuildingData.BuildingId;
        InstanceId = building.InstanceId.Id;
        Level = building.LevelComponent.Level;
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