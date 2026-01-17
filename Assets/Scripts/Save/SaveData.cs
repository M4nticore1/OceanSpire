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

    public int[] npcElevatorPassengerStates { get; private set; } = new int[0];

    public SaveData(PlayerController playerController)
    {
        if (!playerController) {
            Debug.LogError("playerController is NULL");
            return;
        }

        // Player
        cameraYawRotation = playerController.cameraYawRotateAlpha;
        cameraHeightPosition = playerController.cameraVerticalPosition.y;

        // City
        builtFloorsCount = GameManager.Instance.builtFloors.Count;
        int roomsCount = builtFloorsCount * GameManager.roomsCountPerFloor;
        placedBuildingIds = new int[roomsCount];
        placedBuildingLevels = new int[roomsCount];
        placedBuildingsUnderConstruction = new bool[roomsCount];
        placedBuildingInteriorIds = new int[roomsCount];
        buildingProductionTimers = new float[roomsCount];
        elevatorPlatformHeights = new float[roomsCount];
        resourcesAmount = new int[GameManager.Instance.items.Count];

        int placeIndex = 0;
        int lastElevatorGroupId = -1;
        for (int i = 0; i < builtFloorsCount; i++) {
            for (int j = 0; j < GameManager.roomsCountPerFloor; j++) {
                Building placedBuilding = GameManager.Instance.builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                placedBuildingIds[placeIndex] = placedBuilding ? placedBuilding.BuildingData.BuildingId : -1;
                placedBuildingLevels[placeIndex] = placedBuilding ? placedBuilding.LevelIndex : 0;
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
                    elevatorPlatformHeights[placeIndex] = elevatorBuilding.spawnedElevatorCabin.transform.position.y;
                }
                placeIndex++;
            }
        }

        for (int i = 0; i < GameManager.Instance.items.Count; i++) {
            resourcesAmount[i] = GameManager.Instance.items[i].Amount;
        }

        // Boats
        List<Boat> spawnedBoats = GameManager.Instance.spawnedBoats.ToList();
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
        residentsCount = GameManager.Instance.residents.Count;
        residentsIsMoving = new bool[residentsCount];
        residentPositionsX = new float[residentsCount];
        residentPositionsY = new float[residentsCount];
        residentPositionsZ = new float[residentsCount];
        residentFloorIndexes = new int[residentsCount];

        residentCurrentBuildingIndexes = new int[residentsCount];
        residentTargetBuildingIndexes = new int[residentsCount];
        residentWorkBuildingIndexes = new int[residentsCount];

        npcElevatorPassengerStates = new int[residentsCount];

        for (int i = 0; i < residentsCount; i++)
        {
            Creature resident = GameManager.Instance.residents[i];

            residentsIsMoving[i] = resident.isMoving;

            residentPositionsX[i] = resident.transform.position.x;
            residentPositionsY[i] = resident.transform.position.y;
            residentPositionsZ[i] = resident.transform.position.z;
            residentFloorIndexes[i] = resident.CurrentBuilding ? ((TowerBuilding)resident.CurrentBuilding ? ((TowerBuilding)resident.CurrentBuilding).floorIndex : -1) : -1;

            Building currentBuilding = resident.CurrentBuilding;
            if (currentBuilding) {
                TowerBuilding towerBuilding = (TowerBuilding)currentBuilding;
                if (towerBuilding)
                    residentCurrentBuildingIndexes[i] = towerBuilding.floorIndex * GameManager.roomsCountPerFloor + towerBuilding.placeIndex;
                else
                    residentCurrentBuildingIndexes[i] = -1;
            }
            else
                residentCurrentBuildingIndexes[i] = -1;

            Building targetBuilding = resident.TargetBuilding;
            if (targetBuilding) {
                TowerBuilding towerBuilding = (TowerBuilding)targetBuilding;
                if (towerBuilding)
                    residentTargetBuildingIndexes[i] = towerBuilding.floorIndex * GameManager.roomsCountPerFloor + towerBuilding.placeIndex;
                else
                    residentTargetBuildingIndexes[i] = -1;
            }
            else
                residentTargetBuildingIndexes[i] = -1;

            Building workBuilding = resident.workBuilding;
            if (workBuilding) {
                TowerBuilding towerBuilding = (TowerBuilding)workBuilding;
                if (towerBuilding)
                    residentWorkBuildingIndexes[i] = towerBuilding.floorIndex * GameManager.roomsCountPerFloor + towerBuilding.placeIndex;
                else
                    residentWorkBuildingIndexes[i] = -1;
            }
            else
                residentWorkBuildingIndexes[i] = -1;

            npcElevatorPassengerStates[i] = (int)resident.elevatorPassengerState;
        }
    }
}
