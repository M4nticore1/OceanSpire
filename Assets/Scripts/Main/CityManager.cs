using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
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

    public const int floorHeight = 5;
    public const int firstFloorHeight = 5;

    private Vector3 roomSize = new Vector3(8, 5, 8);
    private Vector3 hallSize = new Vector3(24, 5, 24);

    [HideInInspector] public const int roomsCountPerFloor = 8;
    [HideInInspector] public const int roomsCountPerSide = 3;
    [HideInInspector] public const int roomsWidth = 8;
    [HideInInspector] public const int floorWidth = 24;
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

        if (!startBuildingPlace)
            startBuildingPlace = spawnedFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCitybuildingPlace];

        allPaths.Add(new List<Building>());

        if (FindTargetBuildingOnFloor(startBuildingPlace, targetBuildingPlace, null, ref allPaths, ref pathIndex))
        {
            for (int i = 0; i < allPaths.Count; i++)
            {
                allPaths2.Add(new BuildingPath());

                allPaths2[i].paths = allPaths[i];

                if (allPaths[i].Count > 0 && allPaths[i][allPaths[i].Count - 1].GetFloorIndex() == targetBuildingPlace.floorIndex && allPaths[i][allPaths[i].Count - 1].GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                {
                    buildingsPath = allPaths[i];
                }
            }

            for (int i = 0; i < buildingsPath.Count; i++)
            {
                Building currentBuilding = buildingsPath[i];
                ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;

                Building nextBuilding = buildingsPath[i];
                ElevatorBuilding nextElevatorBuilding = currentBuilding as ElevatorBuilding;

                if (buildingsPath.Count > i + 2)
                {
                    if (buildingsPath[i].GetBuildingPlaceIndex() == buildingsPath[i + 1].GetBuildingPlaceIndex() && buildingsPath[i].GetBuildingPlaceIndex() == buildingsPath[i + 2].GetBuildingPlaceIndex())
                    {
                        buildingsPath.RemoveAt(i + 1);
                        i--;
                    }
                }
            }

            return true;
        }
        else
        {
            Debug.Log("Path wasn't found");
            return false;
        }
    }

    private bool FindTargetBuildingOnFloor(BuildingPlace startBuildingPlace, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex)
    {
        if (startBuildingPlace)
        {
            List <ElevatorBuilding> elevatorBuildingsOnFloor = new List<ElevatorBuilding>();
            Building startBuilding = null;
            Building leftBuilding = null;
            Building rightBuilding = null;
            Building finishBuilding = null;

            bool isNeededToCheckLeftSide = true;
            bool isNeededToCheckRightSide = true;

            int startPathIndex = pathIndex;

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

                    if (isNeededToCheckLeftSide)
                    {
                        int leftIndex = (startBuildingPlace.buildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;

                        leftBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, leftIndex);

                        if (leftBuilding)
                        {
                            ElevatorBuilding elevatorBuilding = leftBuilding as ElevatorBuilding;

                            if (elevatorBuilding)
                            {
                                if (AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                    return true;

                                if (AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                    return true;
                            }
                            else
                            {
                                allPaths[startPathIndex].Add(leftBuilding);

                                if (leftBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && leftBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            isNeededToCheckLeftSide = false;
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
                                if (AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                    return true;

                                if (AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                    return true;
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

                    finishBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, finishIndex);

                    if (finishBuilding)
                    {
                        ElevatorBuilding elevatorBuilding = finishBuilding as ElevatorBuilding;

                        if (elevatorBuilding)
                        {
                            if (AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                return true;

                            if (AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                                return true;
                        }
                        else
                        {
                            allPaths[startPathIndex].Add(finishBuilding);

                            if (finishBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && finishBuilding.GetBuildingPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                                return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool AddElevatorPath(ElevatorBuilding verticalElevatorBuilding, ElevatorBuilding startElevatorBuilding, BuildingPlace targetBuildingPlace, BuildingPlace lastBuildingPlace, ref int pathIndex)
    {
        if (verticalElevatorBuilding && verticalElevatorBuilding.buildingPlace != lastBuildingPlace)
        {
            if (verticalElevatorBuilding.buildingData.buildingIdName == startElevatorBuilding.buildingData.buildingIdName)
            {
                pathIndex++;
                allPaths.Add(new List<Building>());

                if (pathIndex > 1)
                {
                    int lastPathCount = pathIndex;

                    for (int j = 1; j < lastPathCount; j++)
                    {
                        allPaths[pathIndex].Add(allPaths[j][j - 1]);
                    }
                }

                allPaths[pathIndex].Add(startElevatorBuilding);
                allPaths[pathIndex].Add(verticalElevatorBuilding);

                return FindTargetBuildingOnFloor(verticalElevatorBuilding.buildingPlace, targetBuildingPlace, startElevatorBuilding.buildingPlace, ref allPaths, ref pathIndex);
            }
            else
                return false;
        }
        else
            return false;
    }
}
