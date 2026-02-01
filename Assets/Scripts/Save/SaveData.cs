using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Main
    public string worldName { get; private set; } = "new_world";

    // Player
    public float cameraYawRotation { get; private set; } = 0;
    public float cameraHeightPosition { get; private set; } = 0;

    // City
    public int builtFloorsCount { get; private set; } = 0;
    public int[] placedRoomIds { get; private set; } = new int[0];
    public int[] placedRoomLevels { get; private set; } = new int[0];
    public bool[] placedRoomsUnderConstruction { get; private set; } = new bool[0];
    public int[] placedRoomInteriorIds { get; private set; } = new int[0];

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
    public int[] residentTowerBuildingWorkIndexes { get; private set; } = new int[0];
    public int[] residentBuildingWorkIndexes { get; private set; } = new int[0];

    public int[] npcElevatorPassengerStates { get; private set; } = new int[0];

    public SaveData(PlayerController playerController)
    {
        if (!playerController) {
            Debug.LogError("playerController is NULL");
            return;
        }

        // Main
        worldName = SaveManager.Instance.saveWorldName;

        // Player
        cameraYawRotation = playerController.cameraYawRotateAlpha;
        cameraHeightPosition = playerController.cameraVerticalPosition.y;

        // City
        builtFloorsCount = CityManager.Instance.builtFloors.Count;
        int roomsCount = builtFloorsCount * CityManager.roomsCountPerFloor;
        placedRoomIds = new int[roomsCount + builtFloorsCount];
        placedRoomLevels = new int[roomsCount + builtFloorsCount];
        placedRoomsUnderConstruction = new bool[roomsCount + builtFloorsCount];
        placedRoomInteriorIds = new int[roomsCount + builtFloorsCount];
        buildingProductionTimers = new float[roomsCount + builtFloorsCount];

        elevatorPlatformHeights = new float[roomsCount];
        resourcesAmount = new int[CityManager.Instance.items.Length];

        int buildingIndex = 0;
        int lastElevatorGroupId = -1;
        for (int floorIndex = 0; floorIndex < builtFloorsCount; floorIndex++) {
            // Halls
            BuildingPlace hallPlace = CityManager.Instance.builtFloors[floorIndex].hallBuildingPlace;
            Building hall = hallPlace.placedBuilding;
            placedRoomIds[buildingIndex] = hall ? hall.BuildingData.BuildingId : -1;
            placedRoomLevels[buildingIndex] = hall ? hall.LevelIndex : -1;
            placedRoomsUnderConstruction[buildingIndex] = hall ? hall.ConstructionComponent.isUnderConstruction : false;
            buildingIndex++;

            // Rooms
            for (int placeIndex = 0; placeIndex < CityManager.roomsCountPerFloor; placeIndex++) {
                Building placedBuilding = CityManager.Instance.builtFloors[floorIndex].roomBuildingPlaces[placeIndex].placedBuilding;
                placedRoomIds[buildingIndex] = placedBuilding ? placedBuilding.BuildingData.BuildingId : -1;
                placedRoomLevels[buildingIndex] = placedBuilding ? placedBuilding.LevelIndex : 0;
                placedRoomsUnderConstruction[buildingIndex] = placedBuilding ? placedBuilding.ConstructionComponent.isUnderConstruction : false;
                placedRoomInteriorIds[buildingIndex] = placedBuilding ? placedBuilding.ConstructionComponent.interiorIndex : -1;

                ProductionBuildingModule productionBuilding = placedBuilding ? placedBuilding.GetComponent<ProductionBuildingModule>() : null;
                buildingProductionTimers[buildingIndex] = productionBuilding ? productionBuilding.currentProductionTime : 0;

                // Elevators
                ElevatorBuilding elevatorBuilding = placedBuilding as ElevatorBuilding;
                if (elevatorBuilding && elevatorBuilding.elevatorGroupId > lastElevatorGroupId) {
                    //lastElevatorGroupId = elevatorBuilding.elevatorGroupId;
                    //if (elevatorPlatformHeights.Length > lastElevatorGroupId)
                    //    elevatorPlatformHeights[lastElevatorGroupId] = elevatorBuilding.elevatorPlatform ? elevatorBuilding.elevatorPlatform.transform.position.y : elevatorBuilding.transform.position.y;
                    elevatorPlatformHeights[buildingIndex] = elevatorBuilding.spawnedElevatorCabin.transform.position.y;
                }
                buildingIndex++;
            }
        }

        for (int i = 0; i < CityManager.Instance.items.Length; i++) {
            resourcesAmount[i] = CityManager.Instance.items[i].Amount;
        }

        // Boats
        List<Boat> spawnedBoats = CityManager.Instance.spawnedBoats.ToList();
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
                spawnedBoatsHealth[i] = boat ? boat.CurrentHealth : 0;
                spawnedBoatPositionsX[i] = boat ? boat.transform.position.x : 0;
                spawnedBoatPositionsZ[i] = boat ? boat.transform.position.z : 0;
                spawnedBoatRotationsY[i] = boat ? boat.transform.rotation.eulerAngles.y : 0;
            }
        }

        // Residents
        residentsCount = CityManager.Instance.residents.Count;
        residentsIsMoving = new bool[residentsCount];
        residentPositionsX = new float[residentsCount];
        residentPositionsY = new float[residentsCount];
        residentPositionsZ = new float[residentsCount];
        residentFloorIndexes = new int[residentsCount];

        residentCurrentBuildingIndexes = new int[residentsCount];
        residentTargetBuildingIndexes = new int[residentsCount];
        residentTowerBuildingWorkIndexes = new int[residentsCount];
        residentBuildingWorkIndexes = new int[residentsCount];

        npcElevatorPassengerStates = new int[residentsCount];

        for (int i = 0; i < residentsCount; i++)
        {
            Creature resident = CityManager.Instance.residents[i];

            residentsIsMoving[i] = resident.isMoving;

            residentPositionsX[i] = resident.transform.position.x;
            residentPositionsY[i] = resident.transform.position.y;
            residentPositionsZ[i] = resident.transform.position.z;
            residentFloorIndexes[i] = resident.currentBuilding ? ((TowerBuilding)resident.currentBuilding ? ((TowerBuilding)resident.currentBuilding).floorIndex : -1) : -1;

            Building currentBuilding = resident.currentBuilding;
            if (currentBuilding) {
                TowerBuilding towerBuilding = (TowerBuilding)currentBuilding;
                if (towerBuilding)
                    residentCurrentBuildingIndexes[i] = towerBuilding.floorIndex * CityManager.roomsCountPerFloor + towerBuilding.placeIndex;
                else
                    residentCurrentBuildingIndexes[i] = -1;
            }
            else
                residentCurrentBuildingIndexes[i] = -1;

            Building targetBuilding = resident.TargetBuilding;
            if (targetBuilding) {
                TowerBuilding towerBuilding = (TowerBuilding)targetBuilding;
                if (towerBuilding)
                    residentTargetBuildingIndexes[i] = towerBuilding.floorIndex * CityManager.roomsCountPerFloor + towerBuilding.placeIndex;
                else
                    residentTargetBuildingIndexes[i] = -1;
            }
            else
                residentTargetBuildingIndexes[i] = -1;

            Building workBuilding = resident.workBuilding;
            if (workBuilding) {
                TowerBuilding towerBuilding = workBuilding as TowerBuilding;
                if (towerBuilding) {
                    residentTowerBuildingWorkIndexes[i] = towerBuilding.floorIndex * CityManager.roomsCountPerFloor + towerBuilding.placeIndex;
                    residentBuildingWorkIndexes[i] = -1;
                }
                else {
                    residentTowerBuildingWorkIndexes[i] = -1;
                    //residentBuildingWorkIndexes[i] = workBuilding
                }
            }
            else
                residentTowerBuildingWorkIndexes[i] = -1;

            npcElevatorPassengerStates[i] = (int)resident.elevatorPassengerState;
        }
    }
}
