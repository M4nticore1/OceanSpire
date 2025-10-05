using System.Linq;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Player
    public float cameraYawRotation = 0;
    public float cameraHeightPosition = 0;

    // City
    public int floorsCount = 0;
    public int[] buildingIds = new int[0];
    public int[] buildingDetailsIds = new int[0];

    public float[] elevatorPlatformHeights = new float[0];

    // Residents
    public int residentsCount = 0;
    public float[] residentPositionsX = new float[0];
    public float[] residentPositionsY = new float[0];
    public float[] residentPositionsZ = new float[0];
    public int[] residentFloorIndexes = new int[0];

    public int[] residentCurrentBuildingIndexes = new int[0];
    public int[] residentTargetBuildingIndexes = new int[0];

    public bool[] residentsRidingOnElevator = new bool[0];
    public bool[] residentsWalkingToElevator = new bool[0];
    public bool[] residentsWaitingForElevator = new bool[0];

    public SaveData(PlayerController playerController, CityManager cityManager)
    {
        // Player
        cameraYawRotation = playerController.cameraYawRotateAlpha;
        cameraHeightPosition = playerController.cameraVerticalPosition.y;

        // City
        floorsCount = cityManager.builtFloors.Count;
        buildingIds = new int[floorsCount * CityManager.roomsCountPerFloor];
        elevatorPlatformHeights = new float[cityManager.elevatorGroups.Count];

        int placeIndex = 0;
        int lastElevatorGroupId = -1;
        for (int i = 0; i < floorsCount; i++)
        {
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                Building placedBuilding = cityManager.builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                buildingIds[placeIndex] = placedBuilding ? (int)placedBuilding.buildingData.buildingId : -1;

                // Elevators
                ElevatorBuilding elevatorBuilding = placedBuilding as ElevatorBuilding;

                if (elevatorBuilding && elevatorBuilding.elevatorGroupId > lastElevatorGroupId)
                {
                    lastElevatorGroupId = elevatorBuilding.elevatorGroupId;
                    elevatorPlatformHeights[lastElevatorGroupId] = elevatorBuilding.spawnedElevatorPlatform.transform.position.y;
                }

                placeIndex++;
            }
        }

        // Residents
        residentsCount = cityManager.residents.Count;
        residentPositionsX = new float[residentsCount];
        residentPositionsY = new float[residentsCount];
        residentPositionsZ = new float[residentsCount];
        residentFloorIndexes = new int[residentsCount];

        residentCurrentBuildingIndexes = new int[residentsCount];
        residentTargetBuildingIndexes = new int[residentsCount];

        residentsRidingOnElevator = new bool[residentsCount];
        residentsWalkingToElevator = new bool[residentsCount];
        residentsWaitingForElevator = new bool[residentsCount];

        for (int i = 0; i < residentsCount; i++)
        {
            residentPositionsX[i] = cityManager.residents[i].transform.position.x;
            residentPositionsY[i] = cityManager.residents[i].transform.position.y;
            residentPositionsZ[i] = cityManager.residents[i].transform.position.z;
            residentFloorIndexes[i] = cityManager.residents[i].currentFloorIndex;

            Building currentBuilding = cityManager.residents[i].currentBuilding;
            if (currentBuilding)
                residentCurrentBuildingIndexes[i] = currentBuilding.GetFloorIndex() * CityManager.roomsCountPerFloor + currentBuilding.GetPlaceIndex();
            else
                residentCurrentBuildingIndexes[i] = -1;

            Building targetBuilding = cityManager.residents[i].targetBuilding;
            if (targetBuilding)
                residentTargetBuildingIndexes[i] = targetBuilding.GetFloorIndex() * CityManager.roomsCountPerFloor + targetBuilding.GetPlaceIndex();
            else
                residentTargetBuildingIndexes[i] = -1;

            residentsRidingOnElevator[i] = cityManager.residents[i].isRidingOnElevator;
            residentsWalkingToElevator[i] = cityManager.residents[i].isWalkingToElevator;
            residentsWaitingForElevator[i] = cityManager.residents[i].isWaitingForElevator;
        }
    }
}
