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
            BuildingInstanceId = construction.OwnedBuilding.InstanceId.Id,
            Height = construction.transform.position.y,
            TargetFloor = construction.TargetFloor,
        };
    }
}