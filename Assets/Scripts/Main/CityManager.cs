using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

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
    public List<FloorBuilding> builtFloors = new List<FloorBuilding>();

    //[HideInInspector] public int floorsCount = 0;
    //[HideInInspector] public int[] buildingIds = { };

    //[HideInInspector] public List<List<RoomBuilding>> allRooms = new List<List<RoomBuilding>>();

    [HideInInspector] public List<int> currentRoomsNumberOnFloor = new List<int>();

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

    [HideInInspector] public List<List<ElevatorBuilding>> elevatorGroups = new List<List<ElevatorBuilding>>();

    // Items
    public List<ItemAmountEntry> startResources = new List<ItemAmountEntry>();
    public List<ItemInstance> items = new List<ItemInstance>();

    [Header("NPC")]
    [HideInInspector] public List<Resident> residents = new List<Resident>();
    private int startResidentsCount = 4;
    //[HideInInspector] public int residentsCount = 0;
    [HideInInspector] public int employedResidentCount = 0;
    [HideInInspector] public int unemployedResidentsCount = 0;
    [SerializeField] private List<Transform> entitySpawnPositions = new List<Transform>();
    //[HideInInspector] public Vector3[] residentPositions = { };
    //[HideInInspector] public int[] residentCurrentBuildingIndexes = { };
    //[HideInInspector] public int[] residentTargetBuildingIndexes = { };

    public static event Action OnStorageCapacityUpdated;
    public event Action OnResidentsAdded;
    public event Action<Resident> OnResidentAdded;
    public event Action<Resident> OnResidentRemoved;

    public List<List<Building>> allPaths = new List<List<Building>>();
    public List<BuildingPath> allPaths2 = new List<BuildingPath>();

    public Coroutine bakeNavMeshSurfaceCoroutine;

    // Saved Data
    //[HideInInspector] public float[] elevatorPlatformHeights = new float[0];
    //[HideInInspector] public bool[] residentsElevatorRiding = new bool[0];

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();
    }

    private void OnEnable()
    {
        Building.onAnyBuildingStartConstructing += OnBuildingStartConstructing;
        Building.onAnyBuildingFinishConstructing += OnBuildingFinishConstructing;

        Resident.OnWorkerAdd += AddWorker;
        Resident.OnWorkerRemove += RemoveWorker;
    }

    private void OnDisable()
    {
        Building.onAnyBuildingStartConstructing -= OnBuildingStartConstructing;
        Building.onAnyBuildingFinishConstructing -= OnBuildingFinishConstructing;

        Resident.OnWorkerAdd -= AddWorker;
        Resident.OnWorkerRemove -= RemoveWorker;
    }

    public void LoadCity(SaveData data)
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();

        InitializeItems();

        if (builtFloors.Count > 0)
        {
            // Build saved buildings
            int lastElevatorGropId = -1;
            int builtFloorsCount = data != null ? data.builtFloorsCount : builtFloors.Count;
            for (int i = 0; i < builtFloorsCount; i++)
            {
                if (i < builtFloors.Count)
                {
                    builtFloors[i].Place(i - 1 >= 0 ? builtFloors[i - 1].floorBuildingPlace : null, 0, false);
                }
                else
                {
                    builtFloors[i - 1].floorBuildingPlace.PlaceBuilding(gameManager.buildingPrefabs[1], 0, false);
                }

                if (data != null)
                {
                    if (data.placedBuildingIds != null)
                    {
                        for (int j = 0; j < roomsCountPerFloor; j++)
                        {
                            int placeIndex = (i * roomsCountPerFloor) + j;

                            int buildingId = data.placedBuildingIds[placeIndex];
                            int buildingLevelIndex = data.placedBuildingLevels != null ? data.placedBuildingLevels[placeIndex] : 0;
                            bool buildingIsUnderConstruction = data.placedBuildingsUnderConstruction != null ? data.placedBuildingsUnderConstruction[placeIndex] : false;

                            if (data.placedBuildingIds[placeIndex] >= 0)
                            {
                                BuildingType buildingType = gameManager.buildingPrefabs[buildingId].buildingData.buildingType;

                                if (buildingType == BuildingType.Hall)
                                {
                                    if (!builtFloors[i].hallBuildingPlace.placedBuilding || (builtFloors[i].hallBuildingPlace.placedBuilding && !builtFloors[i].hallBuildingPlace.placedBuilding.isInitialized))
                                    {
                                        builtFloors[i].hallBuildingPlace.PlaceBuilding(gameManager.buildingPrefabs[buildingId], buildingLevelIndex, buildingIsUnderConstruction);
                                    }

                                    break;
                                }
                                else if (buildingType == BuildingType.Room)
                                {
                                    builtFloors[i].roomBuildingPlaces[j].PlaceBuilding(gameManager.buildingPrefabs[buildingId], buildingLevelIndex, buildingIsUnderConstruction);
                                }
                            }
                            else
                            {
                                if (builtFloors[i].hallBuildingPlace.placedBuilding)
                                    builtFloors[i].hallBuildingPlace.DestroyBuilding();
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < roomsCountPerFloor; j++)
                    {
                        int placeIndex = i * roomsCountPerFloor + j;

                        if (builtFloors[i].hallBuildingPlace.placedBuilding && builtFloors[i].hallBuildingPlace.placedBuilding.buildingData.buildingType == BuildingType.Hall)
                        {
                            builtFloors[i].hallBuildingPlace.InitializePlacedBuilding();

                            break;
                        }
                        else if (builtFloors[i].roomBuildingPlaces[j].placedBuilding && builtFloors[i].roomBuildingPlaces[j].placedBuilding.buildingData.buildingType == BuildingType.Room)
                        {
                            builtFloors[i].roomBuildingPlaces[j].InitializePlacedBuilding();
                        }
                    }
                }
            }

            if (data != null)
            {
                for (int i = builtFloors.Count - 1; i > data.builtFloorsCount; i--)
                {
                    builtFloors[i].Demolish();
                }
            }

            cityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + CityManager.floorHeight;
        }
        else
            Debug.LogError("The count of builtFloors is 0");

        UpdateEmptyBuildingPlacesCount();

        if (bakeNavMeshSurfaceCoroutine == null)
            bakeNavMeshSurfaceCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());

        StartCoroutine(LoadCityCoroutine(data));
    }

    // Entities
    private void LoadEntities(SaveData data)
    {
        if (data != null)
        {
            // Load entities
            for (int i = 0; i < data.residentsCount; i++)
            {
                Vector3 spawnPosition = new Vector3(data.residentPositionsX[i], data.residentPositionsY[i], data.residentPositionsZ[i]);
                Quaternion spawnRotation = Quaternion.identity;

                Resident resident = Instantiate(gameManager.residentPrefab, spawnPosition, spawnRotation);
                AddResident(resident);

                // Set Current Building
                if (data.residentCurrentBuildingIndexes != null && data.residentCurrentBuildingIndexes.Length > i && data.residentCurrentBuildingIndexes[i] >= 0)
                {
                    Building currentBuilding = builtFloors[(int)(data.residentCurrentBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentCurrentBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (currentBuilding)
                        resident.EnterBuilding(currentBuilding);
                }

                // Set Riding Elevator
                if (data.residentsRidingOnElevator != null && data.residentsRidingOnElevator.Length > i && data.residentsRidingOnElevator[i])
                {
                    ElevatorBuilding elevatorBuilding = resident.currentBuilding as ElevatorBuilding;
                    if (elevatorBuilding)
                        resident.StartElevatorRiding(elevatorBuilding);

                    resident.transform.position = spawnPosition;
                }

                // Set Walking Elevator
                if (data.residentsWalkingToElevator != null && data.residentsWalkingToElevator.Length > i && data.residentsWalkingToElevator[i])
                {
                    ElevatorBuilding elevatorBuilding = resident.currentBuilding as ElevatorBuilding;
                    if (elevatorBuilding)
                        resident.StartElevatorWalking(elevatorBuilding);
                }

                // Set Waiting Elevator
                if (data.residentsWaitingForElevator != null && data.residentsWaitingForElevator.Length > i && data.residentsWaitingForElevator[i])
                {
                    ElevatorBuilding elevatorBuilding = resident.currentBuilding as ElevatorBuilding;
                    if (elevatorBuilding)
                        resident.StartElevatorWaiting(elevatorBuilding);
                }

                // Set Target Building
                if (data.residentTargetBuildingIndexes != null && data.residentTargetBuildingIndexes.Length > i && data.residentTargetBuildingIndexes[i] >= 0)
                {
                    Building targetBuilding = builtFloors[(int)(data.residentTargetBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentTargetBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (targetBuilding)
                        resident.SetTargetBuilding(targetBuilding);
                }
            }
        }
        else
        {
            // Create new residents
            for (int i = 0; i < startResidentsCount; i++)
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
        }

        OnResidentsAdded?.Invoke();
    }

    private IEnumerator LoadCityCoroutine(SaveData data)
    {
        if (bakeNavMeshSurfaceCoroutine != null)
            yield return bakeNavMeshSurfaceCoroutine;

        // Resources
        if (data != null)
        {
            if (data.resourcesAmount != null)
            {
                for (int i = 0; i < data.resourcesAmount.Length; i++)
                {
                    items[i].SetAmount(data.resourcesAmount[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < startResources.Count; i++)
            {
                items[(int)startResources[i].itemdata.itemId].SetAmount(startResources[i].capacity);
            }
        }

        LoadEntities(data);
    }

    private void AddResident(Resident resident)
    {
        residents.Add(resident);
        unemployedResidentsCount++;

        OnResidentAdded?.Invoke(resident);
    }

    private void RemoveResident(Resident resident)
    {
        OnResidentRemoved?.Invoke(resident);
        //residents.Remove(residents[]);
        Destroy(resident);
        unemployedResidentsCount++;
    }

    public void AddWorker()
    {
        employedResidentCount++;
        unemployedResidentsCount--;
    }

    public void RemoveWorker()
    {
        employedResidentCount--;
        unemployedResidentsCount++;
    }

    // Building Places
    public void InitializeFloor(FloorBuilding floor)
    {
        if (builtFloors.Count == floor.GetFloorIndex())
            builtFloors.Add(floor);

        currentRoomsNumberOnFloor.Add(0);

        UpdateEmptyBuildingPlacesCount();
        UpdateCityHeight();
    }

    public void UpdateEmptyBuildingPlacesCount()
    {
        List<int> lastPlacedRoomsFloorIndex = new List<int>();
        for (int i = 0; i < CityManager.roomsCountPerFloor; i++)
            lastPlacedRoomsFloorIndex.Add(0);

        int lastPlacedHallFloorIndex = 0;

        for (int i = 0; i < builtFloors.Count; i++)
        {
            // Set room heights
            bool isRoomPlacedOnFloor = false;
            for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
            {
                if (builtFloors[i].roomBuildingPlaces[j].placedBuilding)
                    isRoomPlacedOnFloor = true;

                if (builtFloors[i].roomBuildingPlaces[j].placedBuilding)
                    lastPlacedRoomsFloorIndex[j] = i;

                for (int k = lastPlacedRoomsFloorIndex[j]; k <= i; k++)
                {
                    builtFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesAbove = i - k;

                    if (k != lastPlacedRoomsFloorIndex[j])
                        builtFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesBelow = k - lastPlacedRoomsFloorIndex[j] - 1;
                }
            }

            // Set hall heights
            if (builtFloors[i].hallBuildingPlace.placedBuilding || isRoomPlacedOnFloor) {
                lastPlacedHallFloorIndex = i;}

            for (int k = lastPlacedHallFloorIndex; k <= i; k++)
            {
                builtFloors[k].hallBuildingPlace.emptyBuildingPlacesAbove = i - k;

                if (k != lastPlacedHallFloorIndex)
                    builtFloors[k].hallBuildingPlace.emptyBuildingPlacesBelow = k - lastPlacedHallFloorIndex - 1;
            }
        }
    }

    private void UpdateCityHeight()
    {
        cityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + CityManager.floorHeight;
    }

    public void ShowBuildingPlacesByType(Building building)
    {
        HideAllBuildigPlaces();

        for (int i = 0; i < builtFloors.Count; i++)
        {
            builtFloors[i].ShowBuildingPlacesByType(building);
        }
    }

    public void HideBuildingPlacesByType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.Room)
        {
            for (int i = 0; i < builtFloors.Count; i++)
            {
                for (int j = 0; j < CityManager.roomsCountPerFloor; j++)
                {
                    if (builtFloors[i].roomBuildingPlaces[j] != null)
                    {
                        builtFloors[i].roomBuildingPlaces[j].HideBuildingPlace();
                    }
                }
            }
        }
        else if (buildingType == BuildingType.Hall)
        {
            for (int i = 0; i < builtFloors.Count; i++)
            {
                if (builtFloors[i].hallBuildingPlace != null)
                {
                    builtFloors[i].hallBuildingPlace.HideBuildingPlace();
                }
            }
        }
        else if (buildingType == BuildingType.FloorFrame)
        {
            for (int i = 0; i < builtFloors.Count; i++)
            {
                if (builtFloors[i].floorBuildingPlace != null)
                {
                    builtFloors[i].floorBuildingPlace.HideBuildingPlace();
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
        for (int i = 0; i < builtFloors.Count; i++)
        {
            builtFloors[i].HideAllBuildingPlaces();
        }
    }

    // Buildings
    public void PlaceBuilding(Building buildingToPlace, BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    {
        bool canPlace = false;
        BuildingPlace currentBuildingPlace = null;

        int buildingHeight = buildingToPlace.buildingData.buildingFloors;

        if (buildingToPlace.buildingData.buildingType == BuildingType.Room)
        {
            int floorIndex = buildingPlace.floorIndex;
            int buildingPlaceIndex = buildingPlace.buildingPlaceIndex;
            
            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;

            buildingPlace.PlaceBuilding(buildingToPlace, 0, true);
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
                currentBuildingPlace = builtFloors[buildingPlace.floorIndex - (buildingHeight + buildingPlace.emptyBuildingPlacesAbove - 1)].hallBuildingPlace;
            }

            if (canPlace)
            {
                int floorIndex = currentBuildingPlace.floorIndex;
                currentBuildingPlace.PlaceBuilding(buildingToPlace, levelIndex, isUnderConstruction);
                for (int i = 0; i < roomsCountPerFloor; i++)
                    builtFloors[floorIndex].roomBuildingPlaces[i].AddPlacedBuilding(buildingToPlace);

                for (int i = floorIndex + 1; i < floorIndex + buildingHeight; i++)
                {
                    builtFloors[i].hallBuildingPlace.AddPlacedBuilding(buildingToPlace);

                    for (int j = 0; j < roomsCountPerFloor; j++)
                        builtFloors[i].roomBuildingPlaces[j].AddPlacedBuilding(buildingToPlace);
                }


            }
        }
        else if (buildingToPlace.buildingData.buildingType == BuildingType.FloorFrame)
        {
            buildingPlace.PlaceBuilding(buildingToPlace, 0, false);
        }
    }

    private void OnBuildingStartConstructing(Building building)
    {
        int levelIndex = building.levelIndex;
        SpendItems(building.buildingLevelsData[levelIndex].ResourcesToBuild);

        UpdateEmptyBuildingPlacesCount();

        if (bakeNavMeshSurfaceCoroutine == null)
            bakeNavMeshSurfaceCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());

        HideAllBuildigPlaces();
    }

    private void OnBuildingFinishConstructing(Building building)
    {
        int levelIndex = building.levelIndex;
        SpendItems(building.buildingLevelsData[levelIndex].ResourcesToBuild);

        FloorBuilding floorBuilding = building as FloorBuilding;
        if (floorBuilding)
        {
            InitializeFloor(floorBuilding);
        }

        ElevatorBuilding elevatorBuilding = building as ElevatorBuilding;

        if (elevatorBuilding)
        {
            if (elevatorGroups.Count > elevatorBuilding.elevatorGroupId)
            {
                elevatorGroups[elevatorBuilding.elevatorGroupId].Add(elevatorBuilding);
            }
            else
            {
                List<ElevatorBuilding> elevatorGroup = new List<ElevatorBuilding>();
                elevatorGroup.Add(elevatorBuilding);
                elevatorGroups.Add(elevatorGroup);
            }
        }

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

                building.StartBuilding(levelIndex + 1);
            }
        }
    }

    public void DemolishBuilding(Building building)
    {
        building.Demolish();
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();

        bakeNavMeshSurfaceCoroutine = null;
    }

    // Get Buildings
    private Building GetBuildingByIndex(int floorIndex, int buildingPlaceIndex)
    {
        Building building = null;

        bool isFloorIndexMoreMin = floorIndex >= 0;
        bool isFloorIndexLessMax = floorIndex < builtFloors.Count;
        bool isBuildingPlaceIndexMoreMin = buildingPlaceIndex >= 0;
        bool isBuildingPlaceIndexLessMax = buildingPlaceIndex < roomsCountPerFloor;

        if (isFloorIndexMoreMin && isFloorIndexLessMax && isBuildingPlaceIndexMoreMin && isBuildingPlaceIndexLessMax)
        {
            building = builtFloors[floorIndex].roomBuildingPlaces[buildingPlaceIndex].placedBuilding;
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
                if (startBuildingPlace.floorIndex < builtFloors.Count - 1)
                {
                    verticalBuilding = builtFloors[startBuildingPlace.floorIndex + 1].roomBuildingPlaces[startBuildingPlace.buildingPlaceIndex].placedBuilding;
                }
            }
            else
            {
                if ((startBuildingPlace.floorIndex > firstBuildCityFloorIndex + 1))
                {
                    verticalBuilding = builtFloors[startBuildingPlace.floorIndex - 1].roomBuildingPlaces[startBuildingPlace.buildingPlaceIndex].placedBuilding;

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
            int id = (int)itemsToSpend[i].resourceData.itemId;
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
            startBuildingPlace = builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCitybuildingPlace];

        allPaths.Add(new List<Building>());

        if (FindTargetBuildingOnFloor(startBuildingPlace, targetBuildingPlace, null, ref allPaths, ref pathIndex))
        {
            for (int i = 0; i < allPaths.Count; i++)
            {
                allPaths2.Add(new BuildingPath());

                allPaths2[i].paths = allPaths[i];

                if (allPaths[i].Count > 0 && allPaths[i][allPaths[i].Count - 1].GetFloorIndex() == targetBuildingPlace.floorIndex && allPaths[i][allPaths[i].Count - 1].GetPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
                {
                    buildingsPath = allPaths[i];
                }
            }

            for (int i = 0; i < buildingsPath.Count; i++)
            {
                Type currentBuildingType = buildingsPath[i].GetType();

                if (currentBuildingType != null && buildingsPath.Count > i + 1)
                {
                    Type nextBuildingType = buildingsPath[i + 1].GetType();

                    if (nextBuildingType != null && currentBuildingType == nextBuildingType)
                    {
                        if (currentBuildingType == typeof(Building))
                        {
                            buildingsPath.RemoveAt(i);
                            i--;
                        }
                        else if (currentBuildingType == typeof(ElevatorBuilding))
                        {
                            if (nextBuildingType != null && buildingsPath.Count > i + 2)
                            {
                                if (buildingsPath[i + 2].GetType() == currentBuildingType)
                                {
                                    buildingsPath.RemoveAt(i + 1);
                                    i--;
                                }
                            }
                        }
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
                    if (AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                        return true;

                    if (AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingPlace, lastBuildingPlace, ref pathIndex))
                        return true;
                }
                else
                {
                    if (startBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && startBuilding.GetPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
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

                                if (leftBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && leftBuilding.GetPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
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

                                if (rightBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && rightBuilding.GetPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
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

                            if (finishBuilding.GetFloorIndex() == targetBuildingPlace.floorIndex && finishBuilding.GetPlaceIndex() == targetBuildingPlace.buildingPlaceIndex)
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

                ElevatorBuilding lastElevatorBuilding = lastBuildingPlace ? lastBuildingPlace.placedBuilding as ElevatorBuilding : null;
                if (lastElevatorBuilding)
                {
                    if (pathIndex > 1)
                    {
                        for (int i = 1; i < pathIndex; i++)
                        {
                            if (startElevatorBuilding != allPaths[i][i - 1])
                                allPaths[pathIndex].Add(allPaths[i][i - 1]);
                        }
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
