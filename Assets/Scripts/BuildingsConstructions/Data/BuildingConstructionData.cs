using System;
using UnityEngine;

public class BuildingConstructionData
{
    public Guid BuildingInstanceId;

    public static BuildingConstructionData Create(BuildingConstruction construction)
    {
        if (!construction) {
            Debug.Log("Building construction not found!");
            return null;
        }

        return new BuildingConstructionData()
        {
            BuildingInstanceId = construction.OwnedBuilding.InstanceId.GetGuid()
        };
    }
}