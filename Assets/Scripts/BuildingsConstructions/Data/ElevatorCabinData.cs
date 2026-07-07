using System.Collections.Generic;
using UnityEngine;

public class ElevatorCabinData : BuildingConstructionData
{
    public float Height = 0f;
    public int TargetFloor = 0;

    public static ElevatorCabinData Create(ElevatorCabinConstruction construction)
    {
        if (!construction) {
            Debug.Log("Elevator cabin construction not found!");
            return null;
        }

        return new ElevatorCabinData()
        {
            OwnedBuildingInstanceId = construction.OwnedBuilding.InstanceId.GetGuid(),
            Height = construction.transform.position.y,
            TargetFloor = construction.TargetFloor,
        };
    }

    public static ElevatorCabinData[] Create(ElevatorCabinConstruction[] constructions)
    {
        List<ElevatorCabinData> cabins = new();

        foreach (var cabin in constructions) {
            if (!cabin) continue;

            cabins.Add(Create(cabin));
        }

        return cabins.ToArray();
    }
}