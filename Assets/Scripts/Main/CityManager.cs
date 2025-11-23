using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

enum Direction
{
    Forward,
    Back
}

[Serializable]
public class BuildingPath
{
    public List<Building> paths = new List<Building>();
}

public class CityManager : MonoBehaviour
{
    private GameManager gameManager;

    // Buildings
    [Header("Buildings")]
    public Transform towerRoot = null;
    private NavMeshSurface towerNavMeshSurface = null;
    public List<FloorBuilding> builtFloors = new List<FloorBuilding>();
    [SerializeField] private PierBuilding pierBuilding = null;
    public PierBuilding PierBuilding => pierBuilding;

    public List<int> currentRoomsNumberOnFloor { get; private set; } = new List<int>();

    public const int floorHeight = 5;
    public const int firstFloorHeight = 5;

    public const int roomsCountPerFloor = 8;
    public const int roomsCountPerSide = 3;
    public const int roomsWidth = 8;
    public const int floorWidth = 24;
    public const int firstBuildCityFloorIndex = 1;
    public const int firstBuildCitybuildingPlace = 1;
    public float cityHeight { get; private set; } = 0;

    [HideInInspector] public List<List<ElevatorBuilding>> elevatorGroups = new List<List<ElevatorBuilding>>();

    // Items
    public List<ItemInstance> startResources = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> items = new Dictionary<int, ItemInstance>();
    public Dictionary<int, ItemInstance> totalStorageCapacity = new Dictionary<int, ItemInstance>();

    [Header("NPC")]
    [HideInInspector] public List<Resident> residents = new List<Resident>();
    private const int startResidentsCount = 1;
    [HideInInspector] public int employedResidentCount = 0;
    [HideInInspector] public int unemployedResidentsCount = 0;
    [SerializeField] private List<Transform> entitySpawnPositions = new List<Transform>();

    public static event Action<ItemData> OnItemAdded;
    public static event Action OnStorageCapacityUpdated;
    public event Action OnResidentsAdded;
    public event Action<Resident> OnResidentAdded;
    public event Action<Resident> OnResidentRemoved;

    public List<List<Building>> allPaths = new List<List<Building>>();
    public List<BuildingPath> allPaths2 = new List<BuildingPath>();

    public static Coroutine bakeNavMeshSurfaceCoroutine;

    private void OnEnable()
    {
        BuildingWidget.OnStartPlacingConstruction += OnStartPlacingConstruction;
        UIManager.OnBuildStopPlacing += HideAllBuildigPlaces;

        ConstructionComponent.onAnyConstructionStartConstructing += OnBuildingStartConstructing;
        ConstructionComponent.onAnyConstructionFinishConstructing += OnBuildingFinishConstructing;
        ConstructionComponent.onAnyConstructionDemolished += OnConstructionDemolished;

        Resident.OnWorkerAdd += AddWorker;
        Resident.OnWorkerRemove += RemoveWorker;
    }

    private void OnDisable()
    {
        BuildingWidget.OnStartPlacingConstruction += OnStartPlacingConstruction;
        UIManager.OnBuildStopPlacing += HideAllBuildigPlaces;

        ConstructionComponent.onAnyConstructionStartConstructing -= OnBuildingStartConstructing;
        ConstructionComponent.onAnyConstructionFinishConstructing -= OnBuildingFinishConstructing;
        ConstructionComponent.onAnyConstructionDemolished += OnConstructionDemolished;

        Resident.OnWorkerAdd -= AddWorker;
        Resident.OnWorkerRemove -= RemoveWorker;
    }

    public void Load(SaveData data)
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();

        InitializeItems();
        LoadBuildings(data);
        LoadResources(data);
        //LoadEntities(data);
        StartCoroutine(LoadCityCoroutine(data));
    }

    private void LoadBuildings(SaveData data)
    {
        if (builtFloors.Count > 0)
        {
            int builtFloorsCount = data != null ? data.builtFloorsCount : builtFloors.Count;
            for (int i = 0; i < builtFloorsCount; i++)
            {
                if (i < builtFloors.Count)
                {
                    builtFloors[i].InitializeBuilding(i > 0 ? builtFloors[i - 1].floorBuildingPlace : null, 0, false, -1);
                }
                else
                {
                    PlaceBuilding(gameManager.buildingPrefabs[1], builtFloors[i - 1].floorBuildingPlace, 0, false);
                }

                if (data != null)
                {
                    if (data.placedBuildingIds != null)
                    {
                        for (int j = 0; j < roomsCountPerFloor; j++)
                        {
                            int placeIndex = (i * roomsCountPerFloor) + j;
                            int buildingId = data.placedBuildingIds[placeIndex];

                            if (data.placedBuildingIds[placeIndex] >= 0)
                            {
                                int buildingLevelIndex = data.placedBuildingLevels != null ? data.placedBuildingLevels[placeIndex] : 0;
                                int buildingLInteriorId = data.placedBuildingInteriorIds != null ? data.placedBuildingInteriorIds[placeIndex] : 0;
                                bool buildingIsUnderConstruction = data.placedBuildingsUnderConstruction != null ? data.placedBuildingsUnderConstruction[placeIndex] : false;

                                Building building = gameManager.buildingPrefabs[buildingId];
                                BuildingType buildingType = gameManager.buildingPrefabs[buildingId].BuildingData.BuildingType;

                                if (buildingType == BuildingType.Hall)
                                {
                                    if (!builtFloors[i].hallBuildingPlace.placedBuilding || (builtFloors[i].hallBuildingPlace.placedBuilding && !builtFloors[i].hallBuildingPlace.placedBuilding.isInitialized))
                                    {
                                        BuildingPlace place = builtFloors[i].hallBuildingPlace;
                                        PlaceBuilding(building, place, buildingLevelIndex, buildingIsUnderConstruction);
                                    }
                                }
                                else if (buildingType == BuildingType.Room)
                                {
                                    BuildingPlace place = builtFloors[i].roomBuildingPlaces[j];
                                    PlaceBuilding(building, place, buildingLevelIndex, buildingIsUnderConstruction);
                                }
                            }
                            else
                            {
                                Building building = builtFloors[i].hallBuildingPlace.placedBuilding;
                                if (building && !building.isInitialized)
                                    DemolishContruction(building);
                            }
                        }
                    }

                    pierBuilding.InitializeBuilding(null, pierBuilding.levelComponent.LevelIndex, pierBuilding.constructionComponent.isUnderConstruction);

                    if (data.spawnedBoatIds != null)
                    {
                        for (int j = 0; j < data.spawnedBoatIds.Length; j++)
                        {
                            int id = data.spawnedBoatIds[j];
                            bool isUnderConstruction = data.spawnedBoatsAreUnderConstruction[j];
                            bool isMoving = data.spawnedBoatsAreMoving[j];
                            float health = data.spawnedBoatsHealth[j];
                            float positionX = data.spawnedBoatPositionsX[j];
                            float positionZ = data.spawnedBoatPositionsZ[j];
                            float rotationY = data.spawnedBoatRotationsY[j];

                            PlaceBoat(gameManager.boatPrefabs[id], pierBuilding, isUnderConstruction, j, isMoving, positionX, positionZ, rotationY);
                        }
                    }
                }
                else
                {
                    BuildingPlace hallPlace = builtFloors[i].hallBuildingPlace;
                    Building hall = hallPlace.placedBuilding;
                    if (hall)
                        PlaceBuilding(hall, hallPlace, hall.levelComponent.LevelIndex, hall.constructionComponent.isUnderConstruction);

                    for (int j = 0; j < roomsCountPerFloor; j++)
                    {
                        BuildingPlace roomPlace = builtFloors[i].roomBuildingPlaces[j];
                        Building room = roomPlace.placedBuilding;
                        if (room)
                            PlaceBuilding(room, roomPlace, room.levelComponent.LevelIndex, room.constructionComponent.isUnderConstruction);
                    }

                    // Pier Building
                    pierBuilding.InitializeBuilding(null, pierBuilding.levelComponent.LevelIndex, pierBuilding.constructionComponent.isUnderConstruction);
                    List<Boat> spawnedBoats = pierBuilding.SpawnedBoats.ToList();
                    for (int j = 0; j < spawnedBoats.Count; j++)
                    {
                        if (spawnedBoats[j])
                        {
                            PierConstruction construction = pierBuilding.constructionComponent.spawnedConstruction as PierConstruction;
                            spawnedBoats[j].Initialize(pierBuilding, j);
                            spawnedBoats[j].transform.position = construction.BoatDockPositions[j].position;
                            spawnedBoats[j].transform.rotation = construction.BoatDockPositions[j].rotation;
                        }
                    }
                }
            }

            if (data != null)
            {
                for (int i = builtFloors.Count - 1; i > data.builtFloorsCount; i--)
                {
                    builtFloors[i].constructionComponent.StartDemolishing();
                }
            }

            cityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + CityManager.floorHeight;
        }
        else
            Debug.LogError("The count of builtFloors is 0");

        UpdateEmptyBuildingPlacesCount();

        if (bakeNavMeshSurfaceCoroutine == null)
            bakeNavMeshSurfaceCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private void InitializeItems()
    {
        for (int i = 0; i < ItemDatabase.items.Count; i++)
        {
            ItemData data = ItemDatabase.itemsById[i];
            int id = data.ItemId;
            items.Add(id, new ItemInstance(data));
            totalStorageCapacity.Add(id, new ItemInstance(data));
        }
    }

    private void LoadResources(SaveData data)
    {
        if (data != null)
        {
            if (data.resourcesAmount != null)
            {
                for (int i = 0; i < data.resourcesAmount.Length; i++)
                {
                    AddItem(i, data.resourcesAmount[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < startResources.Count; i++)
            {
                AddItem(startResources[i].ItemData.ItemId, startResources[i].Amount);
            }
        }
    }

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
                        resident.StartElevatorRiding();

                    resident.transform.position = spawnPosition;
                }

                // Set Walking Elevator
                if (data.residentsWalkingToElevator != null && data.residentsWalkingToElevator.Length > i && data.residentsWalkingToElevator[i])
                {
                    ElevatorBuilding elevatorBuilding = resident.currentBuilding as ElevatorBuilding;
                    if (elevatorBuilding)
                        resident.StartElevatorWalking();
                }

                // Set Waiting Elevator
                if (data.residentsWaitingForElevator != null && data.residentsWaitingForElevator.Length > i && data.residentsWaitingForElevator[i])
                {
                    ElevatorBuilding elevatorBuilding = resident.currentBuilding as ElevatorBuilding;
                    if (elevatorBuilding)
                        resident.StartElevatorWaiting();
                }

                // Set Target Building
                if (data.residentTargetBuildingIndexes != null && data.residentTargetBuildingIndexes.Length > i && data.residentTargetBuildingIndexes[i] >= 0)
                {
                    Building targetBuilding = builtFloors[(data.residentTargetBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentTargetBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (targetBuilding)
                        resident.SetTargetBuilding(targetBuilding.buildingPlace, b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex());
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
    private void OnStartPlacingConstruction(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();
        Boat boat = construction.GetComponent<Boat>();
        if (building)
            ShowBuildingPlacesByType(building);
        else if (boat)
            PlaceBoat(boat, pierBuilding, true);
    }

    public void PlaceBuilding(Building building, BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    {
        int buildingHeight = building.BuildingData.BuildingFloors;

        if (buildingPlace)
        {
            Building spawnedBuilding = buildingPlace.placedBuilding;
            if (!spawnedBuilding)
                spawnedBuilding = Instantiate(building, buildingPlace.transform);

            buildingPlace.SetPlacedBuilding(spawnedBuilding);
            BuildingType type = spawnedBuilding.BuildingData.BuildingType;
            if (type == BuildingType.Room)
            {
                currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
            }
            else if (type == BuildingType.Hall)
            {
                if (!spawnedBuilding.isInitialized)
                {
                    if (currentRoomsNumberOnFloor[buildingPlace.floorIndex] == 0)
                    {
                        for (int i = 0; i < roomsCountPerFloor; i++)
                        {
                            builtFloors[buildingPlace.floorIndex].roomBuildingPlaces[i].SetPlacedBuilding(spawnedBuilding);
                            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
                        }
                    }
                }
            }

            spawnedBuilding.InitializeBuilding(buildingPlace, levelIndex, isUnderConstruction);
        }

        //bool canPlace = false;
        //BuildingPlace currentBuildingPlace = null;

        //int buildingHeight = buildingToPlace.BuildingData.BuildingFloors;

        //if (buildingToPlace.BuildingData.BuildingType == BuildingType.Room)
        //{
        //    int floorIndex = buildingPlace.floorIndex;
        //    int buildingPlaceIndex = buildingPlace.buildingPlaceIndex;

        //    currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;

        //    buildingPlace.PlaceBuilding(buildingToPlace, 0, true, -1);
        //}
        //else if (buildingToPlace.BuildingData.BuildingType == BuildingType.Hall)
        //{
        //    if (buildingPlace.emptyBuildingPlacesAbove >= buildingHeight - 1)
        //    {
        //        canPlace = true;
        //        currentBuildingPlace = buildingPlace;
        //    }
        //    else if (buildingPlace.emptyBuildingPlacesBelow >= buildingHeight - 1)
        //    {
        //        canPlace = true;
        //        currentBuildingPlace = builtFloors[buildingPlace.floorIndex - (buildingHeight + buildingPlace.emptyBuildingPlacesAbove - 1)].hallBuildingPlace;
        //    }

        //    if (canPlace)
        //    {
        //        int floorIndex = currentBuildingPlace.floorIndex;
        //        currentBuildingPlace.PlaceBuilding(buildingToPlace, levelIndex, isUnderConstruction, -1);
        //        for (int i = 0; i < roomsCountPerFloor; i++)
        //            builtFloors[floorIndex].roomBuildingPlaces[i].AddPlacedBuilding(buildingToPlace);

        //        for (int i = floorIndex + 1; i < floorIndex + buildingHeight; i++)
        //        {
        //            builtFloors[i].hallBuildingPlace.AddPlacedBuilding(buildingToPlace);

        //            for (int j = 0; j < roomsCountPerFloor; j++)
        //                builtFloors[i].roomBuildingPlaces[j].AddPlacedBuilding(buildingToPlace);
        //        }
        //    }
        //}
        //else if (buildingToPlace.BuildingData.BuildingType == BuildingType.FloorFrame)
        //{
        //    buildingPlace.PlaceBuilding(buildingToPlace, 0, false, -1);
        //}
    }

    public void PlaceBoat(Boat boat, PierBuilding pierBuilding, bool isUnderConstruction = false, int? dockIndex = null, bool isMoving = false, float? health = null, float? positionX = null, float? positionZ = null, float? rotationY = null)
    {
        pierBuilding.CreateBoat(boat, isUnderConstruction, dockIndex, isMoving, health, positionX, positionZ, rotationY);
    }

    public void DemolishContruction(Building building)
    {
        building.constructionComponent.StartDemolishing();
    }

    private void OnBuildingStartConstructing(ConstructionComponent construction)
    {
        int levelIndex = construction.levelComponent ? construction.levelComponent.LevelIndex : 0;

        Building building = construction.GetComponent<Building>();
        if (building)
        {
            OnBuildingFinishConstructing(construction);
            //building.FinishConstructing();
        }

        UpdateEmptyBuildingPlacesCount();

        if (bakeNavMeshSurfaceCoroutine == null)
            bakeNavMeshSurfaceCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());

        HideAllBuildigPlaces();
    }

    private void OnBuildingFinishConstructing(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();
        if (building)
        {
            building.FinishConstructing();

            FloorBuilding floorBuilding = building as FloorBuilding;
            ElevatorBuilding elevatorBuilding = building as ElevatorBuilding;
            if (floorBuilding)
            {
                InitializeFloor(floorBuilding);
            }
            else if (elevatorBuilding)
            {
                if (elevatorGroups.Count <= elevatorBuilding.elevatorGroupId)
                {
                    List<ElevatorBuilding> elevatorGroup = new List<ElevatorBuilding>();
                    elevatorGroups.Add(elevatorGroup);
                }
                elevatorGroups[elevatorBuilding.elevatorGroupId].Add(elevatorBuilding);
            }

            if (building.storageComponent)
            {
                int level = building.levelComponent.LevelIndex;
                if (level > 1)
                {
                    StorageBuildingLevelData previousLevelData = building.storageComponent.levelsData[level - 1] as StorageBuildingLevelData;
                    SubtractStorageCapacity(previousLevelData, false);
                }

                StorageBuildingLevelData currentLevelData = building.storageComponent.levelsData[level] as StorageBuildingLevelData;
                if (currentLevelData)
                    AddStorageCapacity(currentLevelData, false);
                else
                    Debug.LogError(building.BuildingData.BuildingName + $" has no StorageBuildingLevelData by level index {level}");
            }

            //HideAllBuildigPlaces();
        }
    }

    public void TryToUpgradeBuilding(Building building)
    {
        int nextLevelIndex = building.levelComponent.LevelIndex + (building.constructionComponent.isRuined ? 0 : 1);

        if (building.ConstructionLevelsData.Count() > nextLevelIndex)
        {
            bool isResourcesToUpgradeEnough = true;

            int index = 0;
            int amount = 0;
            List<ItemInstance> resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

            for (int i = 0; i < resourcesToUpgrade.Count; i++)
            {
                index = resourcesToUpgrade[i].ItemData.ItemId;
                amount = resourcesToUpgrade[i].Amount;

                if (items[index].Amount < amount)
                {
                    isResourcesToUpgradeEnough = false;
                    break;
                }
            }

            if (isResourcesToUpgradeEnough)
            {
                for (int i = 0; i < resourcesToUpgrade.Count; i++)
                {
                    //itemIndex = GameManager.GetItemIndexById(GameManager.itemsData, resourcesToUpgrade[i].ItemData.ItemId);
                    amount = resourcesToUpgrade[i].Amount;
                    SpendItem(resourcesToUpgrade[i].ItemData.ItemId, amount);
                }

                building.constructionComponent.StartUpgrading();
            }
        }
    }

    private void OnConstructionDemolished(ConstructionComponent construction)
    {
        // Return the part of resources
        List<ItemInstance> resourceToBuilds = construction.constructionLevelsData[construction.levelComponent.LevelIndex].ResourcesToBuild;
        for (int i = 0; i < resourceToBuilds.Count; i++)
        {
            int id = resourceToBuilds[i].ItemData.ItemId;
            int amount = (int)math.ceil(resourceToBuilds[i].Amount * GameManager.demolitionResourceRefundRate);
            AddItem(id, amount);
        }
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();

        bakeNavMeshSurfaceCoroutine = null;
    }

    // Get Buildings
    public Building GetBuildingByIndex(int floorIndex, int buildingPlaceIndex)
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

        return building;
    }

    public static int GetFloorIndexByHeight(float height)
    {
        int floorIndex = (int)((height - firstFloorHeight) / floorHeight);
        if (floorIndex < 0) floorIndex = 0;
        return floorIndex;
    }

    // Resources
    public void AddStorageCapacity(StorageBuildingLevelData storageLevelData, bool isNeededToUpdate)
    {
        ChangeStorageCapacity(storageLevelData, true, isNeededToUpdate);
    }

    public void SubtractStorageCapacity(StorageBuildingLevelData storageLevelData, bool isNeededToUpdate)
    {
        ChangeStorageCapacity(storageLevelData, false, isNeededToUpdate);
    }

    private void ChangeStorageCapacity(StorageBuildingLevelData storageLevelData, bool isIncreasing, bool isNeededToUpdate)
    {
        for (int i = 0; i < storageLevelData.storageItems.Count; i++)
        {
            int id = storageLevelData.storageItems[i].ItemData.ItemId;
            int changeValue = storageLevelData.storageItems[i].Amount;

            if (isIncreasing)
                totalStorageCapacity[id].AddAmount(changeValue);
            else
                totalStorageCapacity[id].AddAmount(changeValue);
        }

        for (int i = 0; i < storageLevelData.storageItemCategories.Count; i++)
        {
            for (int j = 0; j < ItemDatabase.items.Count; j++)
            {
                if (items[j].ItemData.itemCategory == storageLevelData.storageItemCategories[i].itemCategory)
                {
                    int changeValue = storageLevelData.storageItemCategories[i].amount;

                    if (isIncreasing)
                        totalStorageCapacity[j].AddAmount(changeValue);
                    else
                        totalStorageCapacity[j].AddAmount(changeValue);
                }
            }
        }

        if (isNeededToUpdate)
            OnStorageCapacityUpdated?.Invoke();
    }

    public int AddItem(ItemInstance item)
    {
        return AddItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    public int AddItem(int itemId, int amount)
    {
        return AddItem_Internal(itemId, amount);
    }

    public void AddItems(List<ItemInstance> items)
    {
        foreach (ItemInstance item in items)
            AddItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    private int AddItem_Internal(int itemId, int amount)
    {
        for (int i = 0; i < builtFloors.Count; i++)
        {
            if (amount <= 0)
                break;

            for (int j = 0; j < roomsCountPerFloor; j++)
            {
                if (amount <= 0)
                    break;

                Building placedBuilding = builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                if (placedBuilding)
                {
                    if (placedBuilding.storageComponent)
                    {
                        int amountToAdd = placedBuilding.storageComponent.AddItem(itemId, amount);
                        amount -= amountToAdd;
                        items[itemId].AddAmount(amountToAdd, totalStorageCapacity[itemId].Amount);
                    }

                    if (placedBuilding.BuildingData.BuildingType == BuildingType.Hall)
                        break;
                }
            }
        }

        OnItemAdded?.Invoke(ItemDatabase.items[itemId]);
        return items[itemId].Amount;
    }

    public void SpendItem(int id, int amount)
    {
        items[id].SubtractAmount(amount);
    }

    public void SpendItem(string idName, int amount)
    {
        int index = ItemDatabase.itemsByIdName[idName].ItemId;
        items[index].SubtractAmount(amount);
    }

    public void SpendItems(List<ItemInstance> itemsToSpend)
    {
        for (int i = 0; i < itemsToSpend.Count; i++)
        {
            int id = (int)itemsToSpend[i].ItemData.ItemId;
            int amount = itemsToSpend[i].Amount;

            SpendItem(id, amount);
        }
    }

    // Path finding
    public Building FindPathToBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
    {
        buildingsPath.Clear();
        allPaths.Clear();
        allPaths2.Clear();

        int pathIndex = 0;

        if (!startBuildingPlace || startBuildingPlace.floorIndex < firstBuildCityFloorIndex)
            startBuildingPlace = builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCitybuildingPlace];

        allPaths.Add(new List<Building>());

        List<bool> checkedBuildingPlaces = new List<bool>();
        for (int i = 0; i < builtFloors.Count * roomsCountPerFloor; i++)
            checkedBuildingPlaces.Add(false);

        BuildingPlace targetBuildingPlace = FindTargetBuildingOnFloor(startBuildingPlace, targetBuildingCondition, null, ref allPaths, ref pathIndex, ref checkedBuildingPlaces);
        if (targetBuildingPlace)
        {
            for (int i = 0; i < allPaths.Count; i++)
            {
                allPaths2.Add(new BuildingPath());

                allPaths2[i].paths = allPaths[i];

                if (allPaths[i].Count > 0 && targetBuildingCondition(allPaths[i][allPaths[i].Count - 1]) && targetBuildingCondition(allPaths[i][allPaths[i].Count - 1]))
                {
                    buildingsPath = allPaths[i];
                }
            }

            for (int i = 0; i < buildingsPath.Count - 1; i++)
            {
                Type currentType = buildingsPath[i].GetType();
                Type nextType = buildingsPath[i + 1].GetType();

                if (currentType == typeof(ElevatorBuilding))
                {
                    if (buildingsPath.Count > i + 2 && buildingsPath[i + 2] && buildingsPath[i].GetPlaceIndex() == buildingsPath[i + 2].GetPlaceIndex())
                    {
                        if (buildingsPath[i + 2].GetType() == currentType)
                        {
                            buildingsPath.RemoveAt(i + 1);
                            i--;
                        }
                    }
                }
                else
                {
                    buildingsPath.RemoveAt(i);
                    i--;
                }
            }

            return targetBuildingPlace.placedBuilding;
        }
        else
        {
            Debug.Log("Path wasn't found");
            return null;
        }
    }

    private BuildingPlace FindTargetBuildingOnFloor(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex, ref List<bool> checkedBuildingPlaces)
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
                // Start Building
                int startBuildingPlaceIndex = startBuildingPlace.floorIndex * roomsCountPerFloor + startBuildingPlace.BuildingPlaceIndex;
                if (!checkedBuildingPlaces[startBuildingPlaceIndex])
                {
                    checkedBuildingPlaces[startBuildingPlaceIndex] = true;
                    ElevatorBuilding elevatorBuilding = startBuilding as ElevatorBuilding;

                    if (elevatorBuilding)
                    {
                        BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                        if (upperTargetPlace)
                            return upperTargetPlace;

                        BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                        if (lowerTargetPlace)
                            return lowerTargetPlace;
                    }
                    else
                    {
                        allPaths[startPathIndex].Add(startBuilding);

                        if (targetBuildingCondition(startBuilding))
                            return startBuilding.buildingPlace;
                    }
                }

                // Side Buildings
                for (int i = 1; i < roomsCountPerFloor / 2; i++)
                {
                    int index = (startBuildingPlace.BuildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;

                    // Left Buildings
                    if (isNeededToCheckLeftSide)
                    {
                        int leftIndex = (startBuildingPlace.BuildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;
                        leftBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, leftIndex);

                        if (leftBuilding)
                        {
                            int buildingPlaceIndex = leftBuilding.GetFloorIndex() * roomsCountPerFloor + leftBuilding.GetPlaceIndex();
                            if (!checkedBuildingPlaces[buildingPlaceIndex])
                            {
                                checkedBuildingPlaces[buildingPlaceIndex] = true;
                                ElevatorBuilding elevatorBuilding = leftBuilding as ElevatorBuilding;

                                if (elevatorBuilding)
                                {
                                    BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (upperTargetPlace)
                                        return upperTargetPlace;

                                    BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (lowerTargetPlace)
                                        return lowerTargetPlace;
                                }
                                else
                                {
                                    allPaths[startPathIndex].Add(leftBuilding);

                                    if (targetBuildingCondition(leftBuilding))
                                        return leftBuilding.buildingPlace;
                                }
                            }
                        }
                        else
                        {
                            isNeededToCheckLeftSide = false;
                        }
                    }

                    // Right Buildings
                    if (isNeededToCheckRightSide)
                    {
                        int rightIndex = (startBuildingPlace.BuildingPlaceIndex - i + roomsCountPerFloor) % roomsCountPerFloor;
                        rightBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, rightIndex);

                        if (rightBuilding)
                        {
                            int buildingPlaceIndex = rightBuilding.GetFloorIndex() * roomsCountPerFloor + rightBuilding.GetPlaceIndex();
                            if (!checkedBuildingPlaces[buildingPlaceIndex])
                            {
                                checkedBuildingPlaces[buildingPlaceIndex] = true;
                                ElevatorBuilding elevatorBuilding = rightBuilding as ElevatorBuilding;

                                if (elevatorBuilding)
                                {
                                    BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (upperTargetPlace)
                                        return upperTargetPlace;

                                    BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (lowerTargetPlace)
                                        return lowerTargetPlace;
                                }
                                else
                                {
                                    allPaths[startPathIndex].Add(rightBuilding);

                                    if (targetBuildingCondition(rightBuilding))
                                    {
                                        return rightBuilding.buildingPlace;
                                    }
                                }
                            }
                        }
                        else
                        {
                            isNeededToCheckRightSide = false;
                        }
                    }

                    if (!leftBuilding && !rightBuilding)
                        break;
                }

                // Check Finish Building
                if (isNeededToCheckLeftSide || isNeededToCheckRightSide)
                {
                    int finishIndex = (startBuildingPlace.BuildingPlaceIndex + (roomsCountPerFloor / 2) + roomsCountPerFloor) % roomsCountPerFloor;
                    finishBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, finishIndex);

                    if (finishBuilding)
                    {
                        int buildingPlaceIndex = finishBuilding.GetFloorIndex() * roomsCountPerFloor + finishBuilding.GetPlaceIndex();
                        if (!checkedBuildingPlaces[buildingPlaceIndex])
                        {
                            checkedBuildingPlaces[buildingPlaceIndex] = true;
                            ElevatorBuilding elevatorBuilding = finishBuilding as ElevatorBuilding;

                            if (elevatorBuilding)
                            {
                                BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.aboveConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                if (upperTargetPlace)
                                    return upperTargetPlace;

                                BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.belowConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuildingCondition, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                if (lowerTargetPlace)
                                    return lowerTargetPlace;
                            }
                            else
                            {
                                allPaths[startPathIndex].Add(finishBuilding);

                                if (targetBuildingCondition(finishBuilding))
                                    return finishBuilding.buildingPlace;
                            }
                        }
                    }
                }
            }
            //else
                //return null;
        }

        return null;
    }

    private BuildingPlace AddElevatorPath(ElevatorBuilding verticalElevatorBuilding, ElevatorBuilding startElevatorBuilding, Func<Building, bool> targetBuildingCondition, BuildingPlace lastBuildingPlace, ref int pathIndex, ref List<bool> checkedBuildingPlaces)
    {
        if (verticalElevatorBuilding && verticalElevatorBuilding.buildingPlace != lastBuildingPlace)
        {
            if (verticalElevatorBuilding.BuildingData.BuildingIdName == startElevatorBuilding.BuildingData.BuildingIdName)
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

                return FindTargetBuildingOnFloor(verticalElevatorBuilding.buildingPlace, targetBuildingCondition, startElevatorBuilding.buildingPlace, ref allPaths, ref pathIndex, ref checkedBuildingPlaces);
            }
            else
                return null;
        }
        else
            return null;
    }
}
