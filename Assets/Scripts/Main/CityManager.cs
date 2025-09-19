using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    private GameManager gameManager;

    // Buildings
    [Header("Buildings")]
    public int builtFloorsCount = 0;
    public Transform towerRoot = null;
    private NavMeshSurface towerNavMeshSurface = null;
    public List<FloorBuilding> spawnedFloors = new List<FloorBuilding>();

    private List<List<BuildingPlace>> spawnedRoomPlaces = new List<List<BuildingPlace>>();
    [SerializeField] private List<BuildingPlace> spawnedHallPlaces = new List<BuildingPlace>();
    private List<List<BuildingPlace>> spawnedElevatorPlaces = new List<List<BuildingPlace>>();
    [SerializeField] private List<BuildingPlace> spawnedFloorPlaces = new List<BuildingPlace>();

    //[HideInInspector] public List<Building> allBuildings = new List<Building>();
    [HideInInspector] public List<List<RoomBuilding>> allRooms = new List<List<RoomBuilding>>();
    [HideInInspector] public List<List<ElevatorBuilding>> allElevators = new List<List<ElevatorBuilding>>();
    //[HideInInspector] public List<StorageBuildingComponent> spawnedStorageBuildings = new List<StorageBuildingComponent>();

    [HideInInspector] public List<int> currentRoomsNumberOnFloor = new List<int>();
    [HideInInspector] public List<bool> hasHallOnFloors = new List<bool>();
    [HideInInspector] public List<bool> hasFloorFrameOnFloors = new List<bool>();

    public const float floorHeight = 5.0f;
    public const float firstFloorHeight = 5.0f;

    private Vector3 roomSize = new Vector3(8, 5, 8);
    private Vector3 hallSize = new Vector3(24, 5, 24);

    [HideInInspector] public const int roomsCountPerFloor = 8;
    [HideInInspector] public const int elevatorsCountPerFloor = 12;
    [HideInInspector] public const int startBuildingCityFloorIndex = 1;
    [HideInInspector] public float cityHeight = 0;

    private bool areBuildingsInitialized = false;

    private const int secondFloorEntraceIndex = 1;

    // Items
    [HideInInspector] public List<ItemInstance> items = new List<ItemInstance>();

    [Header("NPC")]
    [HideInInspector] public List<Resident> residents = new List<Resident>();
    public int residentsCount = 0;
    public int employedResidentCount = 0;
    public int unemployedResidentCount = 0;
    public List<Transform> entitySpawnPositions = new List<Transform>();

    public static event Action OnStorageCapacityUpdated;
    public event Action OnResidentsAdded;
    public event Action<Resident> OnResidentAdded;
    public event Action<Resident> OnResidentRemoved;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();

        InitializeItems();

        int floorsCount = spawnedFloors.Count;
        for (int i = 0; i < floorsCount; i++)
        {
            builtFloorsCount++;
            spawnedFloors[i].InitializeFloor();

            currentRoomsNumberOnFloor.Add(0);
            hasHallOnFloors.Add(false);
            hasFloorFrameOnFloors.Add(false);

            spawnedRoomPlaces.Add(spawnedFloors[i].roomBuildingPlaces);
            spawnedHallPlaces.Add(spawnedFloors[i].hallBuildingPlace);
            //spawnedElevatorPlaces.Add(spawnedFloors[i].elevatorsBuildingPlaces);

            List<RoomBuilding> rooms = new List<RoomBuilding>();
            for (int j = 0; j < roomsCountPerFloor; j++)
                rooms.Add(null);
            allRooms.Add(rooms);
        }

        cityHeight = spawnedFloors[builtFloorsCount - 1].transform.position.y + CityManager.floorHeight;

        UpdateEmptyBuildingPlacesCount();

        areBuildingsInitialized = true;

        SpawnEntities();

        StartCoroutine(BakeNavMeshSurface());
    }

    private void OnEnable()
    {
        Building.OnBuildingPlaced += OnBuildingPlaced;
        Building.OnBuildingUpgraded += OnBuildingUpgraded;

        Resident.OnWorkerAdd += AddWorker;
        Resident.OnWorkerRemove += RemoveWorker;
    }

    private void OnDisable()
    {
        Building.OnBuildingPlaced -= OnBuildingPlaced;
        Building.OnBuildingUpgraded -= OnBuildingUpgraded;

        Resident.OnWorkerAdd -= AddWorker;
        Resident.OnWorkerRemove -= RemoveWorker;
    }

    private void InitializeBuildings()
    {
        //allBuildings.AddRange(FindObjectsByType<Building>(FindObjectsSortMode.None).ToList());

        //for (int i = 0; i < allBuildings.Count; i++)
        //{
        //    StorageBuildingComponent storageBuidling = allBuildings[i].GetComponent<StorageBuildingComponent>();

        //    if (storageBuidling)
        //    {
        //        spawnedStorageBuildings.Add(storageBuidling);
        //    }
        //}
    }

    // Entities
    private void SpawnEntities()
    {
        for (int i = 0; i < residentsCount; i++)
        {
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (entitySpawnPositions.Count > i && entitySpawnPositions[i])
            {
                spawnPosition = entitySpawnPositions[i].position;
                spawnRotation = entitySpawnPositions[i].rotation;
            }

            Resident resident = Instantiate(gameManager.residentPrefab, spawnPosition, spawnRotation);

            AddResident(resident);
        }

        OnResidentsAdded?.Invoke();
    }

    private void AddResident(Resident resident)
    {
        residents.Add(resident);
        unemployedResidentCount++;

        OnResidentAdded?.Invoke(resident);
    }

    private void RemoveResident(Resident resident)
    {
        OnResidentRemoved?.Invoke(resident);
        //residents.Remove(residents[]);
        Destroy(resident);
        unemployedResidentCount++;
    }

    public void AddWorker()
    {
        employedResidentCount++;
        unemployedResidentCount--;
    }

    public void RemoveWorker()
    {
        employedResidentCount--;
        unemployedResidentCount++;
    }

    // Building Places
    public void AddFloor(FloorBuilding newFloor)
    {
        builtFloorsCount++;
        spawnedFloors.Add(newFloor);

        currentRoomsNumberOnFloor.Add(0);
        hasHallOnFloors.Add(false);
        hasFloorFrameOnFloors.Add(false);

        spawnedRoomPlaces.Add(newFloor.roomBuildingPlaces);
        spawnedHallPlaces.Add(newFloor.hallBuildingPlace);
        spawnedFloorPlaces.Add(newFloor.floorBuildingPlace);

        List<RoomBuilding> rooms = new List<RoomBuilding>();
        for (int j = 0; j < roomsCountPerFloor; j++)
            rooms.Add(null);
        allRooms.Add(rooms);

        UpdateEmptyBuildingPlacesCount();
        UpdateCityHeight();
    }

    public void UpdateEmptyBuildingPlacesCount()
    {
        List<int> lastPlacedRoomsFloorIndex = new List<int>();
        for (int i = 0; i < CityManager.roomsCountPerFloor; i++)
            lastPlacedRoomsFloorIndex.Add(0);

        int lastPlacedHallFloorIndex = 0;

        for (int i = 0; i < builtFloorsCount; i++)
        {
            // Set room heights
            bool isRoomPlacedOnFloor = false;
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                if (spawnedFloors[i].roomBuildingPlaces[j].isBuildingPlaced){
                    isRoomPlacedOnFloor = true;}

                if (spawnedFloors[i].roomBuildingPlaces[j].isBuildingPlaced){
                    lastPlacedRoomsFloorIndex[j] = i;}

                for (int k = lastPlacedRoomsFloorIndex[j]; k <= i; k++)
                {
                    spawnedFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesAbove = i - k;

                    if (k != lastPlacedRoomsFloorIndex[j])
                        spawnedFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesBelow = k - lastPlacedRoomsFloorIndex[j] - 1;
                }
            }

            // Set hall heights
            if (spawnedFloors[i].hallBuildingPlace.isBuildingPlaced || isRoomPlacedOnFloor) {
                lastPlacedHallFloorIndex = i;}

            for (int k = lastPlacedHallFloorIndex; k <= i; k++)
            {
                spawnedFloors[k].hallBuildingPlace.emptyBuildingPlacesAbove = i - k;

                if (k != lastPlacedHallFloorIndex)
                    spawnedFloors[k].hallBuildingPlace.emptyBuildingPlacesBelow = k - lastPlacedHallFloorIndex - 1;
            }
        }
    }

    private void UpdateCityHeight()
    {
        cityHeight = spawnedFloors[builtFloorsCount - 1/* - (spawnedFloorPlaces.Count() - 1)*/].transform.position.y + CityManager.floorHeight;
    }

    public void ShowBuildingPlacesByType(Building building)
    {
        HideAllBuildigPlaces();

        for (int i = 0; i < spawnedFloors.Count; i++)
        {
            spawnedFloors[i].ShowBuildingPlacesByType(building);
        }
    }

    public void HideBuildingPlacesByType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.Room)
        {
            for (int i = 0; i < builtFloorsCount; i++)
            {
                for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
                {
                    if (spawnedFloors[i].roomBuildingPlaces[j] != null)
                    {
                        spawnedFloors[i].roomBuildingPlaces[j].HideBuildingPlace();
                    }
                }
            }
        }
        else if (buildingType == BuildingType.Hall)
        {
            for (int i = 0; i < builtFloorsCount; i++)
            {
                if (spawnedFloors[i].hallBuildingPlace != null)
                {
                    spawnedFloors[i].hallBuildingPlace.HideBuildingPlace();
                }
            }
        }
        else if (buildingType == BuildingType.FloorFrame)
        {
            for (int i = 0; i < builtFloorsCount; i++)
            {
                if (spawnedFloors[i].floorBuildingPlace != null)
                {
                    spawnedFloors[i].floorBuildingPlace.HideBuildingPlace();
                }
            }
        }
        //else if (buildingType == BuildingType.Elevator)
        //{
        //    for (int i = 0; i < buildedFloorsCount; i++)
        //    {
        //        for (int j = 0; j < spawnedFloors[i].elevatorsBuildingPlaces.Count; j++)
        //        {
        //            if (spawnedFloors[i].elevatorsBuildingPlaces[j])
        //            {
        //                spawnedFloors[i].elevatorsBuildingPlaces[j].HideBuildingPlace();
        //            }
        //        }
        //    }
        //}
    }

    private void HideAllBuildigPlaces()
    {
        for (int i = 0; i < spawnedFloors.Count; i++)
        {
            spawnedFloors[i].HideAllBuildingPlaces();
        }
    }

    // BUildings
    public void PlaceBuilding(Building buildingToPlace, BuildingPlace buildingPlace)
    {
        bool canPlace = false;
        BuildingPlace currentBuildingPlace = null;

        int buildingHeight = buildingToPlace.buildingData.buildingFloors;

        if (buildingToPlace.buildingData.buildingType == BuildingType.Room)
        {
            int floorIndex = buildingPlace.floorIndex;
            int buildingIndex = buildingPlace.buildingPlaceIndex;

            allRooms[floorIndex][buildingIndex] = buildingToPlace as RoomBuilding;
            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;

            buildingPlace.PlaceBuilding(buildingToPlace);
        }
        else if (buildingToPlace.buildingData.buildingType == BuildingType.Hall)
        {
            if (buildingPlace.emptyBuildingPlacesAbove >= buildingHeight - 1)
            {
                canPlace = true;
                currentBuildingPlace = buildingPlace;
            }
            else if (buildingPlace.emptyBuildingPlacesBelow >= buildingHeight - 1)
            {
                canPlace = true;
                currentBuildingPlace = spawnedFloors[buildingPlace.floorIndex - (buildingHeight + buildingPlace.emptyBuildingPlacesAbove - 1)].hallBuildingPlace;
            }

            if (canPlace)
            {
                int floorIndex = currentBuildingPlace.floorIndex;
                currentBuildingPlace.PlaceBuilding(buildingToPlace);
                for (int i = 0; i < roomsCountPerFloor; i++)
                    spawnedFloors[floorIndex].roomBuildingPlaces[i].AddPlacedBuilding(buildingToPlace);

                for (int i = floorIndex + 1; i < floorIndex + buildingHeight; i++)
                {
                    spawnedFloors[i].hallBuildingPlace.AddPlacedBuilding(buildingToPlace);

                    for (int j = 0; j < roomsCountPerFloor; j++)
                        spawnedFloors[i].roomBuildingPlaces[j].AddPlacedBuilding(buildingToPlace);

                    hasHallOnFloors[i] = true;
                }


            }
        }
        else if (buildingToPlace.buildingData.buildingType == BuildingType.FloorFrame)
        {
            buildingPlace.PlaceBuilding(buildingToPlace);

            hasFloorFrameOnFloors[buildingPlace.floorIndex] = true;
        }
    }

    private void OnBuildingPlaced(Building building)
    {
        int levelIndex = building.levelIndex;
        SpendItems(building.buildingLevelsData[levelIndex].ResourcesToBuild);

        //StorageBuildingComponent storageBuidling = GetComponent<StorageBuildingComponent>();

        //if (storageBuidling)
        //{
        //    spawnedStorageBuildings.Add(storageBuidling);
        //}

        UpdateEmptyBuildingPlacesCount();
        StartCoroutine(BakeNavMeshSurface());

        HideAllBuildigPlaces();
    }

    private void OnBuildingUpgraded(Building building)
    {
        int levelIndex = building.levelIndex;
        SpendItems(building.buildingLevelsData[levelIndex].ResourcesToBuild);

        HideAllBuildigPlaces();
    }

    public void TryToUpgradeBuilding(Building building)
    {
        int levelIndex = building.levelIndex + (building.isRuined ? 0 : 1);

        if (building.buildingLevelsData.Count() > levelIndex)
        {
            bool isResourcesToUpgradeEnough = true;

            int itemIndex = 0;
            int itemAmount = 0;
            List<ResourceToBuild> resourcesToUpgrade = building.buildingLevelsData[levelIndex].ResourcesToBuild;

            for (int i = 0; i < resourcesToUpgrade.Count; i++)
            {
                itemIndex = gameManager.GetItemIndexByIdName(resourcesToUpgrade[i].resourceData.itemIdName);
                itemAmount = resourcesToUpgrade[i].amount;

                if (items[itemIndex].amount < itemAmount)
                {
                    isResourcesToUpgradeEnough = false;
                    break;
                }
            }

            if (isResourcesToUpgradeEnough)
            {
                for (int i = 0; i < resourcesToUpgrade.Count; i++)
                {
                    itemIndex = gameManager.GetItemIndexByIdName(resourcesToUpgrade[i].resourceData.itemIdName);
                    itemAmount = resourcesToUpgrade[i].amount;
                    SpendItemById(itemIndex, itemAmount);
                }

                building.Upgrade();
            }
        }
    }

    public void DemolishBuilding(Building building)
    {
        building.Demolish();
    }

    private IEnumerator BakeNavMeshSurface()
    {
        if (areBuildingsInitialized)
        {
            yield return new WaitForEndOfFrame();
            towerNavMeshSurface.BuildNavMesh();
        }
    }

    // Resources
    public void AddStorageCapacity(StorageBuildingLevelData storageLevelData)
    {
        ChangeStorageCapacity(storageLevelData, true);
    }

    public void SubtractStorageCapacity(StorageBuildingLevelData storageLevelData)
    {
        ChangeStorageCapacity(storageLevelData, false);
    }

    private void ChangeStorageCapacity(StorageBuildingLevelData storageLevelData, bool isIncreasing)
    {
        for (int i = 0; i < storageLevelData.storageItems.Count(); i++)
        {
            int index = gameManager.GetItemIndexByIdName(storageLevelData.storageItems[i].itemdata.itemIdName);
            int changeValue = storageLevelData.storageItems[i].capacity;

            if (isIncreasing)
                items[index].AddMaxAmount(changeValue);
            else
                items[index].SubtractMaxAmount(changeValue);
        }

        for (int i = 0; i < storageLevelData.storageItemCategories.Count(); i++)
        {
            for (int j = 0; j < items.Count(); j++)
            {
                if (items[j].itemData.itemCategory == storageLevelData.storageItemCategories[i].itemCategory)
                {
                    int changeValue = storageLevelData.storageItemCategories[i].capacity;

                    if (isIncreasing)
                        items[j].AddMaxAmount(changeValue);
                    else
                        items[j].SubtractMaxAmount(changeValue);
                }
            }
        }

        OnStorageCapacityUpdated?.Invoke();
    }

    private void InitializeItems()
    {
        //itemsMaxAmount.Add(ItemType.Population, 8);
        //itemsMaxAmount.Add(ItemType.Food, 100);
        //itemsMaxAmount.Add(ItemType.Electricity, 100);
        //itemsMaxAmount.Add(ItemType.Building, 1000);
        //itemsMaxAmount.Add(ItemType.Crafting, 10);
        //itemsMaxAmount.Add(ItemType.Weapon, 0);

        for (int i = 0; i < gameManager.itemsData.Count; i++)
        {
            items.Add(new ItemInstance(gameManager.itemsData[i], 0, 0));
        }
    }

    public void AddItemByIndex(int index, int amount)
    {
        items[index].AddAmount(amount);
    }

    public void SpendItemById(int id, int amount)
    {
        int index = gameManager.GetItemIndexById(id);
        items[index].SubtractAmount(amount);
    }

    public void SpendItemByIdName(string idName, int amount)
    {
        int index = gameManager.GetItemIndexByIdName(idName);
        items[index].SubtractAmount(amount);
    }

    public void SpendItems(List<ResourceToBuild> itemsToSpend)
    {
        for (int i = 0; i < itemsToSpend.Count; i++)
        {
            int id = itemsToSpend[i].resourceData.itemId;
            int amount = itemsToSpend[i].amount;

            SpendItemById(id, amount);
        }
    }

    //Way
    public bool FindPathToBuilding(Building startBuilding, Building targetBuilding, List<Building> buildingsPath)
    {
        buildingsPath.Clear();

        bool isTargetFloorHigher = false;

        if (startBuilding && targetBuilding)
            isTargetFloorHigher = targetBuilding.floorIndex > startBuilding.floorIndex;
        else if (!startBuilding && targetBuilding)
            isTargetFloorHigher = true;
        else if (startBuilding && !targetBuilding)
            isTargetFloorHigher = false;

        int startFloorIndex = 0;
        int targetFloorIndex = 0;

        if (startBuilding)
            startFloorIndex = startBuilding.floorIndex;
        else
            startFloorIndex = 1;

        if (targetBuilding)
            targetFloorIndex = targetBuilding.floorIndex;

        Building lastEntranceBuilding = null;

        lastEntranceBuilding = startBuilding;

        if ((!startBuilding || startBuilding && startBuilding.floorIndex == 0 || startBuilding.floorIndex == 1) && (targetBuilding && targetBuilding.floorIndex == 0 || targetBuilding.floorIndex == 1)/* || (startBuilding && targetBuilding && startBuilding.floorIndex == targetBuilding.floorIndex)*/)
        {
            buildingsPath.Add(targetBuilding);

            //Debug.Log("Stairs");

            return true;
        }
        else
        {
            //Debug.Log("StartIndex = " + startFloorIndex);
            //Debug.Log("targetBuilding = " + targetFloorIndex);

            List<List<Building>> allPaths = new List<List<Building>>();

            int pathIndex = 0;

            if (FindTargetBuilding(startBuilding, targetBuilding, startBuilding, allPaths, pathIndex))
            {

            }
        }
    }

    private bool FindTargetBuilding(Building startBuilding, Building targetBuilding, Building lastBuilding, List<List<Building>> allPaths, int pathIndex)
    {
        int startFloorIndex = startBuilding.floorIndex;
        int startBuildingIndex = startBuilding.buildingIndex;

        //Building leftBuilding = null;
        //Building rightBuilding = null;

        // Vertical elevators

        bool hasUpDownElevators = false;
        bool isTargetBuildingFounded = false;

        if (!hasUpDownElevators)
            hasUpDownElevators = FindUpDownElevators(startBuilding, targetBuilding, startBuilding, allPaths, pathIndex);
        else
            FindUpDownElevators(startBuilding, targetBuilding, startBuilding, allPaths, pathIndex);

        // Check the buildings on the left side
        for (int i = 1; i < roomsCountPerFloor / 2; i++)
        {
            int leftBuildingIndex = (startBuildingIndex + i) % (roomsCountPerFloor - 1);
            Building leftBuilding = spawnedFloors[startFloorIndex].roomBuildingPlaces[leftBuildingIndex].placedBuilding;

            if (leftBuilding)
            {
                // Left side is not empty
                if (leftBuilding.floorIndex == targetBuilding.floorIndex && leftBuilding.buildingIndex == targetBuilding.buildingIndex)
                {
                    // Target building is founded
                    allPaths[pathIndex].Add(leftBuilding);

                    return true;
                }
                else
                {
                    // Left elevator is founded
                    ElevatorBuilding leftElevatorBuilding = leftBuilding as ElevatorBuilding;

                    if (leftElevatorBuilding)
                    {
                        if (!hasUpDownElevators)
                            hasUpDownElevators = FindUpDownElevators(leftElevatorBuilding, targetBuilding, startBuilding, allPaths, pathIndex);
                        else
                            FindUpDownElevators(leftElevatorBuilding, targetBuilding, startBuilding, allPaths, pathIndex);
                    }
                }
            }
            else
            {
                // Left side is empty
                break;
            }
        }

        // Check the buildings on the right side
        for (int i = 1; i < roomsCountPerFloor / 2; i++)
        {
            int rightBuildingIndex = (startBuildingIndex - i) % (roomsCountPerFloor - 1);
            Building rightBuilding = spawnedFloors[startFloorIndex].roomBuildingPlaces[rightBuildingIndex].placedBuilding;

            if (rightBuilding)
            {
                // Left side is not empty
                if (rightBuilding.floorIndex == targetBuilding.floorIndex && rightBuilding.buildingIndex == targetBuilding.buildingIndex)
                {
                    // Target building is founded

                    return true;
                }
                else
                {
                    // Left elevator is founded
                    ElevatorBuilding rightElevatorBuilding = rightBuilding as ElevatorBuilding;

                    if (rightElevatorBuilding)
                    {
                        ElevatorBuilding rightTopElevatorBuilding = spawnedFloors[startFloorIndex + 1].roomBuildingPlaces[rightBuildingIndex].placedBuilding as ElevatorBuilding;

                        if (!hasUpDownElevators)
                            hasUpDownElevators = FindUpDownElevators(rightTopElevatorBuilding, targetBuilding, startBuilding, allPaths, pathIndex);
                        else
                            FindUpDownElevators(rightTopElevatorBuilding, targetBuilding, startBuilding, allPaths, pathIndex);
                    }
                }
            }
            else
            {
                // Left side is empty
                break;
            }
        }

        //if ((!leftBuilding && !rightBuilding) || (leftBuilding && leftBuilding != targetBuilding && rightBuilding && rightBuilding != targetBuilding) || (leftBuilding && leftBuilding != targetBuilding && !rightBuilding) || (rightBuilding && rightBuilding != targetBuilding && !leftBuilding))
        //{
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}

        if (hasUpDownElevators || isTargetBuildingFounded)
            return true;
        else
            return false;
    }

    private bool FindUpDownElevators(Building startBuilding, Building targetBuilding, Building lastBuilding, List<List<Building>> allPaths, int pathIndex)
    {
        bool hasUpElevator = false;
        bool hasDownElevator = false;

        if (startBuilding.floorIndex < builtFloorsCount - 1){
            ElevatorBuilding upElevatorBuilding = spawnedFloors[startBuilding.floorIndex + 1].roomBuildingPlaces[startBuilding.buildingIndex].placedBuilding as ElevatorBuilding;

            if (upElevatorBuilding)
            {
                allPaths[pathIndex].Add(upElevatorBuilding);

                hasUpElevator = FindTargetBuilding(upElevatorBuilding, targetBuilding, lastBuilding, allPaths, pathIndex);

                pathIndex++;
            }
        }

        if (startBuilding.floorIndex > startBuildingCityFloorIndex)
        {
            ElevatorBuilding downElevatorBuilding = spawnedFloors[startBuilding.floorIndex - 1].roomBuildingPlaces[startBuilding.buildingIndex].placedBuilding as ElevatorBuilding;

            if (downElevatorBuilding)
            {
                allPaths[pathIndex].Add(downElevatorBuilding);

                hasDownElevator = FindTargetBuilding(downElevatorBuilding, targetBuilding, lastBuilding, allPaths, pathIndex);

                pathIndex++;
            }
        }

        if (hasUpElevator || hasDownElevator)
            return true;
        else
            return false;
    }
}
