using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Player
    public float cameraYawRotation = 0;
    public float cameraHeightPosition = 0;

    // City
    public int builtFloorsCount = 0;
    public int[] placedBuildingIds = new int[0];
    public int[] placedBuildingLevels = new int[0];
    public bool[] placedBuildingsUnderConstruction = new bool[0];
    public int[] placedBuildingInteriorIds = new int[0];

    public float[] elevatorPlatformHeights = new float[0];

    // Resources
    public int[] resourcesAmount = new int[0];

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
        builtFloorsCount = cityManager.builtFloors.Count;
        int roomsCount = builtFloorsCount * CityManager.roomsCountPerFloor;
        placedBuildingIds = new int[roomsCount];
        placedBuildingLevels = new int[roomsCount];
        placedBuildingsUnderConstruction = new bool[roomsCount];
        placedBuildingInteriorIds = new int[roomsCount];
        elevatorPlatformHeights = new float[cityManager.elevatorGroups.Count];
        resourcesAmount = new int[cityManager.items.Count];

        int placeIndex = 0;
        int lastElevatorGroupId = -1;
        for (int i = 0; i < builtFloorsCount; i++)
        {
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                Building placedBuilding = cityManager.builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                placedBuildingIds[placeIndex] = placedBuilding ? (int)placedBuilding.BuildingData.BuildingId : -1;
                placedBuildingLevels[placeIndex] = placedBuilding ? placedBuilding.levelComponent.LevelIndex : 0;
                placedBuildingsUnderConstruction[placeIndex] = placedBuilding ? placedBuilding.constructionComponent.isUnderConstruction : false;
                placedBuildingInteriorIds[placeIndex] = placedBuilding ? placedBuilding.constructionComponent.interiorIndex : -1;

                // Elevators
                ElevatorBuilding elevatorBuilding = placedBuilding as ElevatorBuilding;

                if (elevatorBuilding && elevatorBuilding.elevatorGroupId > lastElevatorGroupId)
                {
                    lastElevatorGroupId = elevatorBuilding.elevatorGroupId;

                    if (elevatorPlatformHeights.Length > lastElevatorGroupId)
                        elevatorPlatformHeights[lastElevatorGroupId] = elevatorBuilding.spawnedElevatorPlatform ? elevatorBuilding.spawnedElevatorPlatform.transform.position.y : elevatorBuilding.transform.position.y;
                }

                placeIndex++;
            }
        }

        for (int i = 0; i < cityManager.items.Count; i++)
        {
            resourcesAmount[i] = cityManager.items[i].Amount;
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
            //residentsWalkingToElevator[i] = cityManager.residents[i].isWalkingToElevator;
            residentsWaitingForElevator[i] = cityManager.residents[i].isWaitingForElevator;
        }
    }
}
