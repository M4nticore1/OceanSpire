using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Player
    public float cameraYawRotation { get; private set; } = 0;
    public float cameraHeightPosition { get; private set; } = 0;

    // City
    public int builtFloorsCount { get; private set; } = 0;
    public int[] placedBuildingIds { get; private set; } = new int[0];
    public int[] placedBuildingLevels { get; private set; } = new int[0];
    public bool[] placedBuildingsUnderConstruction { get; private set; } = new bool[0];
    public int[] placedBuildingInteriorIds { get; private set; } = new int[0];

    public float[] elevatorPlatformHeights { get; private set; } = new float[0];

    public float[] buildingProductionTimers { get; private set; } = new float[0];

    // Boats
    public int[] spawnedBoatIds { get; private set; } = new int[0];
    public bool[] spawnedBoatsAreUnderConstruction { get; private set; } = new bool[0];
    public bool[] spawnedBoatsAreFloating { get; private set; } = new bool[0];
    public bool[] spawnedBoatsAreReturning { get; private set; } = new bool[0];
    public float[] spawnedBoatsHealth { get; private set; } = new float[0];
    public float[] spawnedBoatPositionsX { get; private set; } = new float[0];
    public float[] spawnedBoatPositionsZ { get; private set; } = new float[0];
    public float[] spawnedBoatRotationsY { get; private set; } = new float[0];

    // Resources
    public int[] resourcesAmount { get; private set; } = new int[0];

    // Residents
    public int residentsCount { get; private set; } = 0;
    public bool[] residentsIsMoving { get; private set; } = new bool[0];
    public float[] residentPositionsX { get; private set; } = new float[0];
    public float[] residentPositionsY { get; private set; } = new float[0];
    public float[] residentPositionsZ { get; private set; } = new float[0];
    public int[] residentFloorIndexes { get; private set; } = new int[0];

    public int[] residentCurrentBuildingIndexes { get; private set; } = new int[0];
    public int[] residentTargetBuildingIndexes { get; private set; } = new int[0];
    public int[] residentWorkBuildingIndexes { get; private set; } = new int[0];

    public bool[] residentsRidingOnElevator { get; private set; } = new bool[0];
    public bool[] residentsWalkingToElevator { get; private set; } = new bool[0];
    public bool[] residentsWaitingForElevator { get; private set; } = new bool[0];

    public SaveData(PlayerController playerController, CityManager cityManager)
    {
        if (!playerController || !cityManager) {
            if (!playerController) Debug.LogError("playerController is NULL");
            if (!cityManager) Debug.LogError("cityManager is NULL");
            return; }

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
        buildingProductionTimers = new float[roomsCount];
        elevatorPlatformHeights = new float[roomsCount];
        resourcesAmount = new int[cityManager.items.Count];

        int placeIndex = 0;
        int lastElevatorGroupId = -1;
        for (int i = 0; i < builtFloorsCount; i++)
        {
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                Building placedBuilding = cityManager.builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                placedBuildingIds[placeIndex] = placedBuilding ? placedBuilding.BuildingData.BuildingId : -1;
                placedBuildingLevels[placeIndex] = placedBuilding ? placedBuilding.levelIndex : 0;
                placedBuildingsUnderConstruction[placeIndex] = placedBuilding ? placedBuilding.constructionComponent.isUnderConstruction : false;
                placedBuildingInteriorIds[placeIndex] = placedBuilding ? placedBuilding.constructionComponent.interiorIndex : -1;

                ProductionBuilding productionBuilding = placedBuilding ? placedBuilding.GetComponent<ProductionBuilding>() : null;
                buildingProductionTimers[placeIndex] = productionBuilding ? productionBuilding.currentProductionTime : 0;

                // Elevators
               ElevatorBuilding elevatorBuilding = placedBuilding as ElevatorBuilding;
                if (elevatorBuilding && elevatorBuilding.elevatorGroupId > lastElevatorGroupId) {
                    //lastElevatorGroupId = elevatorBuilding.elevatorGroupId;
                    //if (elevatorPlatformHeights.Length > lastElevatorGroupId)
                    //    elevatorPlatformHeights[lastElevatorGroupId] = elevatorBuilding.elevatorPlatform ? elevatorBuilding.elevatorPlatform.transform.position.y : elevatorBuilding.transform.position.y;
                    elevatorPlatformHeights[placeIndex] = elevatorBuilding.elevatorPlatform.transform.position.y;
                }
                placeIndex++;
            }
        }

        for (int i = 0; i < cityManager.items.Count; i++)
        {
            resourcesAmount[i] = cityManager.items[i].Amount;
        }

        // Boats
        List<Boat> spawnedBoats = cityManager.SpawnedBoats.ToList();
        int boatsCount = spawnedBoats.Count;
        spawnedBoatIds = new int[boatsCount];
        spawnedBoatsAreUnderConstruction = new bool[boatsCount];
        spawnedBoatsHealth = new float[boatsCount];
        spawnedBoatsAreFloating = new bool[boatsCount];
        spawnedBoatsAreReturning = new bool[boatsCount];
        spawnedBoatPositionsX = new float[boatsCount];
        spawnedBoatPositionsZ = new float[boatsCount];
        spawnedBoatRotationsY = new float[boatsCount];
        for (int i = 0; i < boatsCount; i++)
        {
            Boat boat = spawnedBoats[i];
            if (boat)
            {
                ConstructionComponent construction = boat.GetComponent<ConstructionComponent>();
                spawnedBoatIds[i] = boat ? boat.BoatData.BoatId : -1;
                spawnedBoatsAreUnderConstruction[i] = boat ? construction.isUnderConstruction : false;
                spawnedBoatsAreFloating[i] = boat ? boat.isFloating : false;
                spawnedBoatsAreReturning[i] = boat ? boat.isReturningToDock : false;
                spawnedBoatsHealth[i] = boat ? boat.currentHealth : 0;
                spawnedBoatPositionsX[i] = boat ? boat.transform.position.x : 0;
                spawnedBoatPositionsZ[i] = boat ? boat.transform.position.z : 0;
                spawnedBoatRotationsY[i] = boat ? boat.transform.rotation.eulerAngles.y : 0;
            }
        }

        // Residents
        residentsCount = cityManager.residents.Count;
        residentsIsMoving = new bool[residentsCount];
        residentPositionsX = new float[residentsCount];
        residentPositionsY = new float[residentsCount];
        residentPositionsZ = new float[residentsCount];
        residentFloorIndexes = new int[residentsCount];

        residentCurrentBuildingIndexes = new int[residentsCount];
        residentTargetBuildingIndexes = new int[residentsCount];
        residentWorkBuildingIndexes = new int[residentsCount];

        residentsRidingOnElevator = new bool[residentsCount];
        residentsWalkingToElevator = new bool[residentsCount];
        residentsWaitingForElevator = new bool[residentsCount];

        for (int i = 0; i < residentsCount; i++)
        {
            Entity resident = cityManager.residents[i];

            residentsIsMoving[i] = resident.isMoving;

            residentPositionsX[i] = resident.transform.position.x;
            residentPositionsY[i] = resident.transform.position.y;
            residentPositionsZ[i] = resident.transform.position.z;
            residentFloorIndexes[i] = resident.currentBuilding ? resident.currentBuilding.floorIndex : 0;

            Building currentBuilding = resident.currentBuilding;
            if (currentBuilding)
                residentCurrentBuildingIndexes[i] = currentBuilding.floorIndex * CityManager.roomsCountPerFloor + currentBuilding.placeIndex;
            else
                residentCurrentBuildingIndexes[i] = -1;

            Building targetBuilding = resident.targetBuilding;
            if (targetBuilding)
                residentTargetBuildingIndexes[i] = targetBuilding.floorIndex * CityManager.roomsCountPerFloor + targetBuilding.placeIndex;
            else
                residentTargetBuildingIndexes[i] = -1;

            Building workBuilding = resident.workBuilding;
            if (workBuilding)
                residentWorkBuildingIndexes[i] = workBuilding.floorIndex * CityManager.roomsCountPerFloor + workBuilding.placeIndex;
            else
                residentWorkBuildingIndexes[i] = -1;

            residentsRidingOnElevator[i] = resident.isRidingOnElevator;
            //residentsWalkingToElevator[i] = resident.isWalkingToElevator;
            residentsWaitingForElevator[i] = resident.isWaitingForElevator;
        }
    }
}
