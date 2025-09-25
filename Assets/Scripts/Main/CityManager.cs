using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

enum Direction
{
    Forward,
    Back
}

[System.Serializable]
public class BuildingPath
{
    public List<Building> paths = new List<Building>();
}

public class CityManager : MonoBehaviour
{
    private GameManager gameManager;

    // Buildings
    [Header("Buildings")]
    //public int builtFloorsCount = 0;
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
    [HideInInspector] public const int firstBuildCityFloorIndex = 1;
    [HideInInspector] public const int firstBuildCitybuildingPlace = 1;
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

    public List<List<Building>> allPaths = new List<List<Building>>();
    public List<BuildingPath> allPaths2 = new List<BuildingPath>();

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();

        InitializeItems();

        int floorsCount = spawnedFloors.Count;

        if (spawnedFloors.Count > 0)
        {
            //spawnedFloors[0].AddFloor(this);

            spawnedFloors[0].InitializeFloor(0);

            currentRoomsNumberOnFloor.Add(0);
            hasHallOnFloors.Add(false);
            hasFloorFrameOnFloors.Add(false);

            //spawnedRoomPlaces.Add(spawnedFloors[i].roomBuildingPlaces);
            //spawnedHallPlaces.Add(spawnedFloors[i].hallBuildingPlace);

            List<RoomBuilding> rooms = new List<RoomBuilding>();
            for (int j = 0; j < roomsCountPerFloor; j++)
                rooms.Add(null);
            allRooms.Add(rooms);
        }

        //for (int i = 0; i < floorsCount; i++)
        //{
        //    spawnedFloors[i].InitializeFloor();

        //    currentRoomsNumberOnFloor.Add(0);
        //    hasHallOnFloors.Add(false);
        //    hasFloorFrameOnFloors.Add(false);

        //    spawnedRoomPlaces.Add(spawnedFloors[i].roomBuildingPlaces);
        //    spawnedHallPlaces.Add(spawnedFloors[i].hallBuildingPlace);
        //    //spawnedElevatorPlaces.Add(spawnedFloors[i].elevatorsBuildingPlaces);

        //    List<RoomBuilding> rooms = new List<RoomBuilding>();
        //    for (int j = 0; j < roomsCountPerFloor; j++)
        //        rooms.Add(null);
        //    allRooms.Add(rooms);
        //}

        cityHeight = spawnedFloors[spawnedFloors.Count - 1].transform.position.y + CityManager.floorHeight;

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
    public void AddFloorCount(FloorBuilding newFloor)
    {
        //builtFloorsCount++;
        spawnedFloors.Add(newFloor);
    }

    public void InitializeFloor(FloorBuilding newFloor)
    {
        //builtFloorsCount++;
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

        //Debug.Log(builtFloorsCount);

        for (int i = 0; i < spawnedFloors.Count; i++)
        {
            // Set room heights
            bool isRoomPlacedOnFloor = false;
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                if (spawnedFloors[i].roomBuildingPlaces[j].isBuildingPlaced)
                    isRoomPlacedOnFloor = true;

                if (spawnedFloors[i].roomBuildingPlaces[j].isBuildingPlaced)
                    lastPlacedRoomsFloorIndex[j] = i;

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
        //Debug.Log(builtFloorsCount - 1);

        cityHeight = spawnedFloors[spawnedFloors.Count - 1/* - (spawnedFloorPlaces.Count() - 1)*/].transform.position.y + CityManager.floorHeight;
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
            for (int i = 0; i < spawnedFloors.Count; i++)
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
            for (int i = 0; i < spawnedFloors.Count; i++)
            {
                if (spawnedFloors[i].hallBuildingPlace != null)
                {
                    spawnedFloors[i].hallBuildingPlace.HideBuildingPlace();
                }
            }
        }
        else if (buildingType == BuildingType.FloorFrame)
        {
            for (int i = 0; i < spawnedFloors.Count; i++)
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

    // Buildings
    public void PlaceBuilding(Building buildingToPlace, BuildingPlace buildingPlace)
    {
        bool canPlace = false;
        BuildingPlace currentBuildingPlace = null;

        int buildingHeight = buildingToPlace.buildingData.buildingFloors;

        if (buildingToPlace.buildingData.buildingType == BuildingType.Room)
        {
            int floorIndex = buildingPlace.floorIndex;
            int buildingPlaceIndex = buildingPlace.buildingPlaceIndex;

            allRooms[floorIndex][buildingPlace.buildingPlaceIndex] = buildingToPlace as RoomBuilding;
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

    // Get Buildings
    private Building GetBuildingByIndex(int floorIndex, int buildingPlaceIndex)
    {
        Building building = null;

        bool isFloorIndexMoreMin = floorIndex >= 0;
        bool isFloorIndexLessMax = floorIndex < spawnedFloors.Count;
        bool isBuildingPlaceIndexMoreMin = buildingPlaceIndex >= 0;
        bool isBuildingPlaceIndexLessMax = buildingPlaceIndex < roomsCountPerFloor;

        if (isFloorIndexMoreMin && isFloorIndexLessMax && isBuildingPlaceIndexMoreMin && isBuildingPlaceIndexLessMax)
        {
            building = spawnedFloors[floorIndex].roomBuildingPlaces[buildingPlaceIndex].placedBuilding;
        }
        else
        {

        }

        return building;
    }

    private Building GetVerticalBuilding(BuildingPlace startBuildingPlace, Direction direction)
    {
        Building verticalBuilding = null;

        if (startBuildingPlace)
        {
            if (direction == Direction.Forward)
            {
                if (startBuildingPlace.floorIndex < spawnedFloors.Count - 1)
                {
                    verticalBuilding = spawnedFloors[startBuildingPlace.floorIndex + 1].roomBuildingPlaces[startBuildingPlace.buildingPlaceIndex].placedBuilding;
                }
            }
            else
            {
                if ((startBuildingPlace.floorIndex > firstBuildCityFloorIndex + 1))
                {
                    verticalBuilding = spawnedFloors[startBuildingPlace.floorIndex - 1].roomBuildingPlaces[startBuildingPlace.buildingPlaceIndex].placedBuilding;

                    if (verticalBuilding)
                        Debug.Log("downElevator");
                }
            }
        }

        return verticalBuilding;
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

    // Path finding
    public bool FindPathToBuilding(BuildingPlace startBuildingPlace, BuildingPlace targetBuildingPlace, ref List<Building> buildingsPath)
    {
        buildingsPath.Clear();
        allPaths.Clear();
        allPaths2.Clear();

        int pathIndex = 0;

        //if ((!startBuildingPlace || startBuildingPlace && startBuildingPlace.floorIndex == 0 || startBuildingPlace.floorIndex == 1) && (targetBuildingPlace && targetBuildingPlace.floorIndex == 0 || targetBuildingPlace.floorIndex == 1))
        //{
        //    Debug.Log("A");

        //    AddPath(ref allPaths, ref pathIndex);

        //    buildingsPath.Add(targetBuildingPlace.placedBuilding);

        //    return true;
        //}
        //else
        if (true)
        {
            if (!startBuildingPlace)
                startBuildingPlace = spawnedFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCitybuildingPlace];

            allPaths.Add(new List<Building>());

            bool isPathFounded = FindTargetBuildingOnFloor(startBuildingPlace, targetBuildingPlace, null, ref allPaths, ref pathIndex);

            if (isPathFounded)
            {
                for (int i = 0; i < allPaths.Count; i++)
                {
                    allPaths2.Add(new BuildingPath());

                    for (int j = 0; j < allPaths[i].Count; j++)
                    {
                        allPaths2[i].paths.Add(allPaths[i][j]);
                    }

                    if (allPaths[i].Count > 0 && allPaths[i][allPaths[i].Count - 1].GetFloorIndex() == targetBuildingPlace.floorIndex && allPaths[i][allPaths[i].Count - 1].GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                    {
                        //buildingsPath = allPaths[i];

                        //for (int j = 0; j < allPaths[i].Count; j++)
                        //{
                        //    buildingsPath.Add(allPaths[i][j]);
                        //}
                    }
                }

                buildingsPath = allPaths[allPaths.Count - 1];
            }

            Debug.Log("return" + isPathFounded);

            return isPathFounded;
        }
    }

    private bool FindTargetBuildingOnFloor(BuildingPlace startBuildingPlace, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex)
    {
        if (startBuildingPlace)
        {
            //
            List <ElevatorBuilding> elevatorBuildingsOnFloor = new List<ElevatorBuilding>();
            Building startBuilding = null;
            Building leftBuilding = null;
            Building rightBuilding = null;
            Building finishBuilding = null;

            bool isNeededToCheckLeftSide = true;
            bool isNeededToCheckRightSide = true;

            int startPathIndex = pathIndex;

            // 
            //AddPath(ref allPaths, ref pathIndex);

            // Check Start Building
            startBuilding = startBuildingPlace.placedBuilding;

            if (startBuilding)
            {
                ElevatorBuilding elevatorBuilding = startBuilding as ElevatorBuilding;

                if (elevatorBuilding)
                {
                    AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                    AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                }
                else
                {
                    allPaths[startPathIndex].Add(startBuilding);

                    if (startBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && startBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                        return true;
                }
            }
            else
            {
                return false;
            }

            if (startBuilding)
            {
                for (int i = 1; i < roomsCountPerFloor / 2; i++)
                {
                    int index = (startBuildingPlace.buildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;

                    Debug.Log("startBuilding floorIndex: " + startBuilding.GetFloorIndex() + " buildingPlaceIndex: " + index);

                    if (isNeededToCheckLeftSide)
                    {
                        int leftIndex = (startBuildingPlace.buildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;

                        leftBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, leftIndex);

                        if (leftBuilding)
                        {
                            ElevatorBuilding elevatorBuilding = leftBuilding as ElevatorBuilding;

                            if (elevatorBuilding)
                            {
                                Debug.Log("leftElevatorBuilding: " + elevatorBuilding + " " + elevatorBuilding.GetFloorIndex() + " " + elevatorBuilding.GetBuildingPlaceIndex());

                                AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                                AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                            }
                            else
                            {
                                Debug.Log("leftBuilding: " + leftBuilding + " " + leftBuilding.GetFloorIndex() + " " + leftBuilding.GetBuildingPlaceIndex());

                                allPaths[startPathIndex].Add(leftBuilding);

                                if (leftBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && leftBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                                {
                                    Debug.Log("Target was found");
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            isNeededToCheckLeftSide = false;

                            Debug.Log(startBuilding.GetFloorIndex() + " " + isNeededToCheckLeftSide);
                        }
                    }

                    if (isNeededToCheckRightSide)
                    {
                        int rightIndex = (startBuildingPlace.buildingPlaceIndex - i + roomsCountPerFloor) % roomsCountPerFloor;

                        rightBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, rightIndex);

                        if (rightBuilding)
                        {
                            ElevatorBuilding elevatorBuilding = rightBuilding as ElevatorBuilding;

                            if (elevatorBuilding)
                            {
                                Debug.Log("rightElevatorBuilding: " + elevatorBuilding + " " + elevatorBuilding.GetFloorIndex() + " " + elevatorBuilding.GetBuildingPlaceIndex());

                                AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                                AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                            }
                            else
                            {
                                allPaths[startPathIndex].Add(rightBuilding);

                                if (rightBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && rightBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                                    return true;
                            }
                        }
                        else
                        {
                            isNeededToCheckRightSide = false;
                        }
                    }

                    if (!leftBuilding && !rightBuilding)
                    {
                        break;
                    }
                }

                // Check Finish Building
                if (isNeededToCheckLeftSide || isNeededToCheckRightSide)
                {
                    int finishIndex = (startBuildingPlace.buildingPlaceIndex + (roomsCountPerFloor / 2) + roomsCountPerFloor) % roomsCountPerFloor;

                    Debug.Log("finishBuilding floorIndex: " + startBuilding.GetFloorIndex() + " buildingPlaceIndex: " + finishIndex);

                    finishBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, finishIndex);

                    if (finishBuilding)
                    {
                        ElevatorBuilding elevatorBuilding = finishBuilding as ElevatorBuilding;

                        if (elevatorBuilding)
                        {
                            Debug.Log("finishElevatorBuilding: " + elevatorBuilding + " " + elevatorBuilding.GetFloorIndex() + " " + elevatorBuilding.GetBuildingPlaceIndex());

                            AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                            AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex);
                        }
                        else
                        {
                            Debug.Log("finishBuilding: " + finishBuilding + " " + finishBuilding.GetFloorIndex() + " " + finishBuilding.GetBuildingPlaceIndex());

                            allPaths[startPathIndex].Add(finishBuilding);

                            if (finishBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && finishBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                                return true;
                        }
                    }
                }

                if (finishBuilding || (!isNeededToCheckLeftSide && !isNeededToCheckRightSide))
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        return false;
    }


    private void AddElevatorPath(ElevatorBuilding verticalElevatorBuilding, ElevatorBuilding startElevatorBuilding, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref int pathIndex)
    {
        //ElevatorBuilding verticalElevatorBuilding = direction == Direction.Forward ? elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding : elevatorBuilding.belowConnectedBuilding as ElevatorBuilding;
        //GetVerticalBuilding(elevatorBuilding.buildingPlace, direction) as ElevatorBuilding;

        if (verticalElevatorBuilding && verticalElevatorBuilding.buildingPlace != lastBuildingPlace)
        {
            Debug.Log(verticalElevatorBuilding);

            if (verticalElevatorBuilding.buildingData.buildingIdName == startElevatorBuilding.buildingData.buildingIdName)
            {
                pathIndex++;
                allPaths.Add(new List<Building>());

                if (pathIndex > 1)
                {
                    int lastPathCount = pathIndex;

                    for (int j = 1; j < lastPathCount; j++)
                    {
                        Debug.Log("Add");
                        allPaths[pathIndex].Add(allPaths[j][j - 1]);
                    }
                }

                allPaths[pathIndex].Add(startElevatorBuilding);
                allPaths[pathIndex].Add(verticalElevatorBuilding);

                FindTargetBuildingOnFloor(verticalElevatorBuilding.buildingPlace, targetBuildingPlace, startElevatorBuilding.buildingPlace, ref allPaths, ref pathIndex);
            }
        }
    }

    //private bool FindTargetBuilding(BuildingPlace startBuildingPlace, BuildingPlace targetBuilding, BuildingPlace lastBuilding, ref List<List<Building>> allPaths, ref int pathIndex)
    //{
    //    // This function checks the new full floor after the path to it has been found.

    //    int startPathIndex = pathIndex;
    //    int currentIndex = startPathIndex;

    //    AddPath(ref allPaths, ref pathIndex);

    //    bool needToAddPath = false;

    //    bool hasStartBuilding = true;
    //    bool hasFinishBuilding = true;
    //    bool hasLeftBuilding = true;
    //    bool hasRightBuilding = true;

    //    Building startBuilding = null;
    //    Building finishBuilding = null;
    //    Building leftBuilding = null;
    //    Building rightBuilding = null;

    //    // Check start building
    //    if (!startBuilding)
    //    {
    //        startBuilding = FindPathPointByPlaceIndex(startBuildingPlace, targetBuilding, startBuildingPlace, ref allPaths, ref currentIndex, 0);

    //        if (startBuilding)
    //        {
    //            ElevatorBuilding elevatorBuilding = startBuilding as ElevatorBuilding;

    //            if (elevatorBuilding)
    //            {
    //                if (needToAddPath)
    //                    AddPath(ref allPaths, ref currentIndex);
    //                else
    //                    needToAddPath = true;
    //            }
    //            else
    //            {
    //                if (startBuilding.GetFloorIndex() == targetBuilding.floorIndex && startBuilding.GetBuildingPlaceIndex() == targetBuilding.buildingPlaceIndex)
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    // Check buildings between first and finish buildings
    //    for (int i = 1; i < roomsCountPerFloor / 2; i++)
    //    {
    //        if (!leftBuilding && hasLeftBuilding)
    //        {
    //            leftBuilding = FindPathPointByPlaceIndex(startBuildingPlace, targetBuilding, startBuildingPlace, ref allPaths, ref currentIndex, i);

    //            if (leftBuilding)
    //            {
    //                ElevatorBuilding elevatorBuilding = leftBuilding as ElevatorBuilding;

    //                if (elevatorBuilding)
    //                {
    //                    if (needToAddPath)
    //                        AddPath(ref allPaths, ref currentIndex);
    //                    else
    //                        needToAddPath = true;
    //                }
    //                else
    //                {
    //                    if (leftBuilding.GetFloorIndex() == targetBuilding.floorIndex && leftBuilding.GetBuildingPlaceIndex() == targetBuilding.buildingPlaceIndex)
    //                    {
    //                        return true;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                hasLeftBuilding = false;
    //            }
    //        }

    //        if (!rightBuilding && hasRightBuilding)
    //        {
    //            rightBuilding = FindPathPointByPlaceIndex(startBuildingPlace, targetBuilding, startBuildingPlace, ref allPaths, ref currentIndex, -i);

    //            if (rightBuilding)
    //            {
    //                ElevatorBuilding elevatorBuilding = rightBuilding as ElevatorBuilding;

    //                if (elevatorBuilding)
    //                {
    //                    if (needToAddPath)
    //                        AddPath(ref allPaths, ref currentIndex);
    //                    else
    //                        needToAddPath = true;
    //                }
    //                else
    //                {
    //                    if (rightBuilding.GetFloorIndex() == targetBuilding.floorIndex && rightBuilding.GetBuildingPlaceIndex() == targetBuilding.buildingPlaceIndex)
    //                    {
    //                        return true;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                hasRightBuilding = false;
    //            }
    //        }

    //        if (!leftBuilding && !rightBuilding)
    //            break;
    //    }

    //    // Check finish building
    //    if (!finishBuilding && (hasLeftBuilding || hasRightBuilding))
    //    {
    //        Debug.Log(hasRightBuilding);

    //        finishBuilding = FindPathPointByPlaceIndex(startBuildingPlace, targetBuilding, startBuildingPlace, ref allPaths, ref currentIndex, roomsCountPerFloor / 2);

    //        if (finishBuilding)
    //        {
    //            ElevatorBuilding elevatorBuilding = finishBuilding as ElevatorBuilding;

    //            if (elevatorBuilding)
    //            {
    //                if (needToAddPath)
    //                    AddPath(ref allPaths, ref currentIndex);
    //                else
    //                    needToAddPath = true;
    //            }
    //            else
    //            {
    //                if (finishBuilding.GetFloorIndex() == targetBuilding.floorIndex && finishBuilding.GetBuildingPlaceIndex() == targetBuilding.buildingPlaceIndex)
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    // If any building is found
    //    if (startBuilding || leftBuilding || rightBuilding || finishBuilding)
    //        return true;
    //    else
    //        return false;
    //}

    //private Building FindPathPointByPlaceIndex(BuildingPlace startBuildingPlace, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex, int sidebuildingPlaceIndexOffset)
    //{
    //    // This function checks the one building on new floor by index.

    //    bool hasUpTargetBuilding = true;
    //    bool hasDownTargetBuilding = true;

    //    Building upElevatorBuilding = null;
    //    Building downElevatorBuilding = null;

    //    int buildingPlaceIndex = (startBuildingPlace.buildingPlaceIndex + sidebuildingPlaceIndexOffset + roomsCountPerFloor) % (roomsCountPerFloor);

    //    Debug.Log(startBuildingPlace.buildingPlaceIndex + " " + sidebuildingPlaceIndexOffset);
    //    //Debug.Log("Check floor by index: " + startBuildingPlace.floorIndex + " building by index: " + buildingPlaceIndex);
    
    //    BuildingPlace offsetBuildingPlace = spawnedFloors[startBuildingPlace.floorIndex].roomBuildingPlaces[buildingPlaceIndex];

    //    if (offsetBuildingPlace)
    //    {
    //        Building building = offsetBuildingPlace.placedBuilding;

    //        if (building)
    //        {
    //            if (building.GetFloorIndex() == targetBuildingPlace.floorIndex && building.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
    //            {
    //                allPaths[pathIndex].Add(building);

    //                return building;
    //            }
    //            else
    //            {
    //                // Getting elevator on floor
    //                ElevatorBuilding elevatorBuilding = building as ElevatorBuilding;

    //                if (elevatorBuilding)
    //                {
    //                    allPaths[pathIndex].Add(elevatorBuilding);

    //                    if (!upElevatorBuilding)
    //                    {
    //                        upElevatorBuilding = FindConnectedElevator(offsetBuildingPlace, targetBuildingPlace, lastBuildingPlace, ref allPaths, ref pathIndex, Direction.Forward);

    //                        if (upElevatorBuilding)
    //                        {
    //                            //allPaths[pathIndex].Add(upElevatorBuilding);

    //                            hasUpTargetBuilding = FindTargetBuilding(upElevatorBuilding.buildingPlace, targetBuildingPlace, offsetBuildingPlace, ref allPaths, ref pathIndex);
    //                        }
    //                    }

    //                    //if (!downElevatorBuilding)
    //                    //{
    //                    //    downElevatorBuilding = FindConnectedElevator(offsetBuildingPlace, targetBuildingPlace, lastBuildingPlace, ref allPaths, ref pathIndex, Direction.Back);

    //                    //    if (downElevatorBuilding)
    //                    //    {
    //                    //        if (upElevatorBuilding)
    //                    //        {
    //                    //            AddPath(ref allPaths, ref pathIndex);
    //                    //            allPaths[pathIndex].Add(downElevatorBuilding);
    //                    //            pathIndex++;
    //                    //        }

    //                    //        hasDownTargetBuilding = FindTargetBuilding(downElevatorBuilding.buildingPlace, targetBuildingPlace, startBuildingPlace, ref allPaths, ref pathIndex);
    //                    //    }
    //                    //}

    //                    if (upElevatorBuilding || downElevatorBuilding)
    //                        return elevatorBuilding;
    //                    else
    //                        return null;
    //                }
    //                else
    //                {
    //                    return building;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //Debug.Log("false");
    //            return null;
    //        }
    //    }
    //    else
    //    {
    //        //Debug.Log("false");
    //        return null;
    //    }
    //}

    //private Building FindConnectedElevator(BuildingPlace startBuildingPlace, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex, Direction direction)
    //{
    //    // This function checks the elevators from above or below if an elevator has been found on the current floor.

    //    ElevatorBuilding verticalElevatorBuilding = null;

    //    if (startBuildingPlace)
    //    {
    //        ElevatorBuilding startElevatorBuilding = startBuildingPlace.placedBuilding as ElevatorBuilding;

    //        if (startElevatorBuilding)
    //        {
    //            if (direction == Direction.Forward ? (startBuildingPlace && startBuildingPlace.floorIndex < builtFloorsCount - 1) : (startBuildingPlace && startBuildingPlace.floorIndex > firstBuildCityFloorIndex + 1))
    //            {
    //                BuildingPlace verticalBuildingPlace = spawnedFloors[startBuildingPlace.floorIndex + (direction == Direction.Forward ? 1 : -1)].roomBuildingPlaces[startBuildingPlace.buildingPlaceIndex];
    //                Building verticalBuilding = verticalBuildingPlace.placedBuilding as ElevatorBuilding;

    //                if (verticalBuilding && verticalBuilding.buildingData.buildingIdName == startElevatorBuilding.buildingData.buildingIdName)
    //                {
    //                    verticalElevatorBuilding = verticalBuilding as ElevatorBuilding;

    //                    //AddPath(ref allPaths, ref pathIndex);
    //                }
    //            }
    //        }
    //    }

    //    return verticalElevatorBuilding;
    //}

    //private void AddPath(ref List<List<Building>> allPaths, ref int pathIndex)
    //{
    //    //pathIndex++;

    //    allPaths.Add(new List<Building>());

    //    //for (int i = 0; i < allPaths[pathIndex].Count; i++)
    //    //{
    //    //    allPaths[pathIndex + 1].Add(allPaths[pathIndex][i]);
    //    //}

    //    pathIndex++;
    //}
}
