using System;
using UnityEngine;

public class BuildingConstructionData
{
    public Guid OwnedBuildingInstanceId = Guid.NewGuid();

    public BuildingConstructionData Default()
    {
        return new BuildingConstructionData();
    }

    public static BuildingConstructionData Create(BuildingConstruction construction)
    {
        if (!construction) {
            Debug.Log("Building construction not found!");
            return null;
        }

        return new BuildingConstructionData()
        {
            OwnedBuildingInstanceId = construction.OwnedBuilding.InstanceId.GetGuid()
        };
    }
}