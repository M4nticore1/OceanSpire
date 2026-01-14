using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
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
    public const int firstBuildCityBuildingPlace = 1;
    public float cityHeight { get; private set; } = 0;

    [HideInInspector] public List<List<ElevatorBuilding>> elevatorGroups = new List<List<ElevatorBuilding>>();

    // Items
    public List<ItemInstance> startResources = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> items = new Dictionary<int, ItemInstance>();
    public Dictionary<int, ItemInstance> totalStorageCapacity = new Dictionary<int, ItemInstance>();

    [Header("NPC")]
    [HideInInspector] public List<Entity> residents = new List<Entity>();
    private const int startResidentsCount = 2;
    [HideInInspector] public int employedResidentCount = 0;
    [HideInInspector] public int unemployedResidentsCount = 0;
    [SerializeField] private List<Transform> entitySpawnPositions = new List<Transform>();

    [Header("Boats")]
    [SerializeField] private List<Boat> spawnedBoats = new List<Boat>();
    public IReadOnlyList<Boat> SpawnedBoats => spawnedBoats.AsReadOnly();

    public static event Action OnConstructionStartPlaced;
    public static event Action OnConstructionPlaced;
    public static event Action OnConstructionDestroyed;
    public static event Action OnLootAdded;
    public static event Action OnStorageCapacityUpdated;
    public event Action OnResidentsAdded;
    public event Action<Entity> OnResidentAdded;
    public event Action<Entity> OnResidentRemoved;

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

        Boat.OnBoadDestroyed += OnBoatDestroyed;
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

        Boat.OnBoadDestroyed -= OnBoatDestroyed;
    }

    private void Start()
    {
        Load(GameManager.saveData);
    }

    private void Update()
    {

    }

    public void Load(SaveData data)
    {
        gameManager = FindAnyObjectByType<GameManager>();
        towerNavMeshSurface = towerRoot.GetComponent<NavMeshSurface>();

        InitializeItems();
        LoadBuildings(data);
        LoadResources(data);
        CreateEntities(data);
        CreateLoads(data);
        StartCoroutine(LoadCityCoroutine(data));
    }

    private void LoadBuildings(SaveData data)
    {
        if (builtFloors.Count > 0) {
            int builtFloorsCount = data != null ? data.builtFloorsCount : builtFloors.Count;
            for (int i = 0; i < builtFloorsCount; i++) {
                if (i < builtFloors.Count) {
                    builtFloors[i].InitializeBuilding(i > 0 ? builtFloors[i - 1].floorBuildingPlace : null, false, 0, -1);
                }
                else {
                    PlaceBuilding(gameManager.BuildingPrefabsList.buildingPrefabsById[0], builtFloors[i - 1].floorBuildingPlace, 0, false);
                }

                if (data != null) {
                    if (data.placedBuildingIds != null) {
                        for (int j = 0; j < roomsCountPerFloor; j++) {
                            int placeIndex = (i * roomsCountPerFloor) + j;
                            int buildingId = data.placedBuildingIds[placeIndex];

                            if (data.placedBuildingIds[placeIndex] >= 0) {
                                int buildingLevelIndex = data.placedBuildingLevels != null ? data.placedBuildingLevels[placeIndex] : 0;
                                int buildingLInteriorId = data.placedBuildingInteriorIds != null ? data.placedBuildingInteriorIds[placeIndex] : 0;
                                bool buildingIsUnderConstruction = data.placedBuildingsUnderConstruction != null ? data.placedBuildingsUnderConstruction[placeIndex] : false;

                                Building buildingToPlace = gameManager.BuildingPrefabsList.buildingPrefabsById[buildingId];
                                Building building = null;
                                BuildingType buildingType = gameManager.BuildingPrefabsList.buildingPrefabsById[buildingId].BuildingData.BuildingType;

                                if (buildingType == BuildingType.Hall) {
                                    if (!builtFloors[i].hallBuildingPlace.placedBuilding || (builtFloors[i].hallBuildingPlace.placedBuilding && !builtFloors[i].hallBuildingPlace.placedBuilding.isInitialized)) {
                                        BuildingPlace place = builtFloors[i].hallBuildingPlace;
                                        building = PlaceBuilding(buildingToPlace, place, buildingLevelIndex, buildingIsUnderConstruction);
                                    }
                                }
                                else /*(buildingType == BuildingType.Room)*/ {
                                    BuildingPlace place = builtFloors[i].roomBuildingPlaces[j];
                                    building = PlaceBuilding(buildingToPlace, place, buildingLevelIndex, buildingIsUnderConstruction);
                                }

                                if (building) {
                                    // Buildings
                                    ElevatorBuilding elevator = building as ElevatorBuilding;
                                    if (elevator) {
                                        Vector3 platformPosition = elevator.spawnedElevatorCabin.transform.position;
                                        if (data.elevatorPlatformHeights != null && data.elevatorPlatformHeights.Length > placeIndex)
                                        elevator.spawnedElevatorCabin.transform.position = new Vector3(platformPosition.x, data.elevatorPlatformHeights[placeIndex], platformPosition.z);
                                    }

                                    // Building Components
                                    ProductionBuilding productionBuilding = building.GetComponent<ProductionBuilding>();
                                    if (productionBuilding) {
                                        float time = data.buildingProductionTimers != null && data.buildingProductionTimers.Length > placeIndex ? data.buildingProductionTimers[placeIndex] : 0;
                                        productionBuilding.SetProductionTime(time);
                                    }
                                }
                            }
                            else {
                                Building building = builtFloors[i].hallBuildingPlace.placedBuilding;
                                if (building && !building.isInitialized)
                                    DemolishContruction(building);
                            }
                        }
                    }
                    pierBuilding.InitializeBuilding(null, pierBuilding.constructionComponent.isUnderConstruction, pierBuilding.levelIndex);
                }
                else {
                    BuildingPlace hallPlace = builtFloors[i].hallBuildingPlace;
                    Building hall = hallPlace.placedBuilding;
                    if (hall)
                        PlaceBuilding(hall, hallPlace, hall.levelIndex, hall.constructionComponent.isUnderConstruction);

                    for (int j = 0; j < roomsCountPerFloor; j++) {
                        BuildingPlace roomPlace = builtFloors[i].roomBuildingPlaces[j];
                        Building room = roomPlace.placedBuilding;
                        if (room)
                            PlaceBuilding(room, roomPlace, room.levelIndex, room.constructionComponent.isUnderConstruction);
                    }

                    // Pier Building
                    pierBuilding.InitializeBuilding(null, pierBuilding.constructionComponent.isUnderConstruction, pierBuilding.levelIndex);
                }
            }

            if (data != null) {
                for (int i = builtFloors.Count - 1; i > data.builtFloorsCount; i--) {
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

    private void CreateEntities(SaveData data)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (data != null) {
            for (int i = 0; i < data.residentsCount; i++) {
                position = new Vector3(data.residentPositionsX[i], data.residentPositionsY[i], data.residentPositionsZ[i]);
                rotation = Quaternion.identity;
                CreateResident(position, rotation);
            }
        }
        else {
            for (int i = 0; i < startResidentsCount; i++) {
                position = Vector3.zero;
                rotation = Quaternion.identity;

                if (entitySpawnPositions.Count > i && entitySpawnPositions[i]) {
                    position = entitySpawnPositions[i].position;
                    rotation = entitySpawnPositions[i].rotation;
                    CreateResident(position, rotation);
                }
            }
        }

        OnResidentsAdded?.Invoke();
    }

    private void LoadEntities(SaveData data)
    {
        if (data != null) {
            for (int i = 0; i < residents.Count; i++) {
                Entity resident = residents[i];

                // Set Current Building
                if (data.residentCurrentBuildingIndexes != null && data.residentCurrentBuildingIndexes.Length > i && data.residentCurrentBuildingIndexes[i] >= 0) {
                    Building building = builtFloors[(data.residentCurrentBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentCurrentBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (building)
                        resident.EnterBuilding(building);
                }

                // Set Work Building
                if (data.residentWorkBuildingIndexes != null && data.residentWorkBuildingIndexes.Length > i && data.residentWorkBuildingIndexes[i] >= 0) {
                    Building building = builtFloors[(data.residentWorkBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentWorkBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (building)
                        resident.SetWork(building);
                }

                if (data.npcElevatorPassengerStates != null && data.npcElevatorPassengerStates.Length > i && data.npcElevatorPassengerStates[i] >= 0) {
                    ElevatorPassengerState state = (ElevatorPassengerState)data.npcElevatorPassengerStates[i];
                    resident.SetElevatorPassengerState(state);
                }
            }
        }
        else {
            for (int i = 0; i < residents.Count; i++) {
                Entity resident = residents[i];

                resident.navMeshAgent.enabled = true;
            }
        }
    }

    private void CreateLoads(SaveData data)
    {
        if (data != null) {
            if (data.spawnedBoatIds != null) {
                for (int j = 0; j < data.spawnedBoatIds.Length; j++) {
                    int id = data.spawnedBoatIds[j];
                    bool isUnderConstruction = data.spawnedBoatsAreUnderConstruction[j];
                    bool isFloating = data.spawnedBoatsAreFloating[j];
                    bool isReturning = data.spawnedBoatsAreReturning[j];
                    float health = data.spawnedBoatsHealth[j];
                    float positionX = data.spawnedBoatPositionsX[j];
                    float positionZ = data.spawnedBoatPositionsZ[j];
                    float rotationY = data.spawnedBoatRotationsY[j];
                    PlaceBoat(gameManager.BoatPrefabsList.BoatPrefabs[id], isUnderConstruction, j, isFloating, isReturning, health, positionX, positionZ, rotationY);
                }
            }
        }
        else {
            List<Boat> spawnedBoats = SpawnedBoats.ToList();
            for (int j = 0; j < spawnedBoats.Count; j++) {
                if (spawnedBoats[j]) {
                    PierConstruction construction = pierBuilding.constructionComponent.SpawnedConstruction as PierConstruction;
                    spawnedBoats[j].Initialize(false, j);
                    spawnedBoats[j].transform.position = construction.BoatDockPositions[j].position;
                    spawnedBoats[j].transform.rotation = construction.BoatDockPositions[j].rotation;
                }
            }
        }
    }

    private void InitializeItems()
    {
        for (int i = 0; i < gameManager.LootList.Loot.Count; i++)
        {
            ItemData data = gameManager.LootList.Loot[i];
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

    private IEnumerator LoadCityCoroutine(SaveData data)
    {
        if (bakeNavMeshSurfaceCoroutine != null)
            yield return bakeNavMeshSurfaceCoroutine;

        LoadEntities(data);
    }

    private void CreateResident(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Entity resident = Instantiate(gameManager.residentPrefab, spawnPosition, spawnRotation);
        AddResident(resident);

        if (bakeNavMeshSurfaceCoroutine != null) {
            resident.navMeshAgent.enabled = false;
        }
    }

    private void AddResident(Entity resident)
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
        if (builtFloors.Count == floor.floorIndex)
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
            PlaceBoat(boat, true);
    }

    public Building PlaceBuilding(Building building, BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    {
        Building spawnedBuilding = buildingPlace.placedBuilding;
        if (buildingPlace) {
            if (!spawnedBuilding)
                spawnedBuilding = Instantiate(building, buildingPlace.transform);

            buildingPlace.SetPlacedBuilding(spawnedBuilding);
            BuildingType type = spawnedBuilding.BuildingData.BuildingType;
            if (type == BuildingType.Room) {
                currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
            }
            else if (type == BuildingType.Hall)  {
                if (!spawnedBuilding.isInitialized) {
                    if (currentRoomsNumberOnFloor[buildingPlace.floorIndex] == 0) {
                        for (int i = 0; i < roomsCountPerFloor; i++) {
                            builtFloors[buildingPlace.floorIndex].roomBuildingPlaces[i].SetPlacedBuilding(spawnedBuilding);
                            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
                        }
                    }
                }
            }
            spawnedBuilding.InitializeBuilding(buildingPlace, isUnderConstruction, levelIndex);
            OnConstructionPlaced?.Invoke();
        }
       return spawnedBuilding;
    }

    public void PlaceBoat(Boat boat, bool isUnderConstruction = false, int? dockIndex = null, bool isFloating = false, bool isReturningToDock = false, float? health = null, float? positionX = null, float? positionZ = null, float? rotationY = null)
    {
        //pierBuilding.CreateBoat(boat, isUnderConstruction, dockIndex, isFloating, isReturningToDock, health, positionX, positionZ, rotationY);

        PierConstruction pierConstruction = pierBuilding.constructionComponent.SpawnedConstruction as PierConstruction;
        if (dockIndex == null) {
            for (int i = 0; i < spawnedBoats.Count; i++) {
                if (!spawnedBoats[i]) {
                    dockIndex = i;
                    break;
                }
            }
        }

        Vector3 position = Vector3.zero /*pierConstruction.BoatDockPositions[dockIndex.Value].position*/;
        if (positionX != null) position.x = positionX.Value;
        if (positionZ != null) position.z = positionZ.Value;

        Quaternion rotation = Quaternion.identity;
        if (rotationY != null) rotation = Quaternion.Euler(0, rotationY.Value, 0);
        else rotation = pierConstruction.BoatDockPositions[dockIndex.Value].rotation;

        if (spawnedBoats[dockIndex.Value])
            spawnedBoats[dockIndex.Value].Demolish(false);

        Boat spawnedBoat = Instantiate(boat, position, rotation);
        spawnedBoat.Initialize(isUnderConstruction, dockIndex.Value, isFloating, isReturningToDock, health);
        spawnedBoats[dockIndex.Value] = spawnedBoat;
    }

    public void DemolishContruction(Building building)
    {
        building.constructionComponent.StartDemolishing();
    }

    private void OnBuildingStartConstructing(ConstructionComponent construction)
    {
        int levelIndex = construction.ownedBuilding.levelIndex;

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

            if (building.BuildingData.BuildingType != BuildingType.Environment && building.storageComponent)
            {
                int level = building.levelIndex;
                if (level > 1)
                {
                    StorageBuildingLevelData previousLevelData = building.storageComponent.LevelsData[level - 1] as StorageBuildingLevelData;
                    SubtractStorageCapacity(previousLevelData, false);
                }

                StorageBuildingLevelData currentLevelData = building.storageComponent.LevelsData[level] as StorageBuildingLevelData;
                if (currentLevelData)
                    AddStorageCapacity(currentLevelData, true);
                else
                    Debug.LogError(building.BuildingData.BuildingName + $" has no StorageBuildingLevelData by level index {level}");
            }

            //HideAllBuildigPlaces();
        }
    }

    public void TryToUpgradeConstruction(Building building)
    {
        int nextLevelIndex = building.levelIndex + (building.constructionComponent.isRuined ? 0 : 1);

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
        List<ItemInstance> resourceToBuilds = construction.constructionLevelsData[construction.ownedBuilding.levelIndex].ResourcesToBuild;
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

    // Boats
    private void OnBoatDestroyed(Boat boat)
    {
        spawnedBoats[boat.dockIndex] = null;
    }

    public Boat GetBoatByIndex(int index)
    {
        for (int i = 0; i < spawnedBoats.Count; i++) {
            if (spawnedBoats[i])
                return spawnedBoats[i];
        }

        return null;
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
                totalStorageCapacity[id].SubtractAmount(changeValue);
        }

        for (int i = 0; i < storageLevelData.storageItemCategories.Count; i++)
        {
            for (int j = 0; j < gameManager.LootList.Loot.Count; j++)
            {
                if (items[j].ItemData.ItemCategory == storageLevelData.storageItemCategories[i].itemCategory)
                {
                    int changeValue = storageLevelData.storageItemCategories[i].amount;

                    if (isIncreasing)
                        totalStorageCapacity[j].AddAmount(changeValue);
                    else
                        totalStorageCapacity[j].SubtractAmount(changeValue);
                }
            }
        }

        if (isNeededToUpdate)
            OnStorageCapacityUpdated?.Invoke();
    }

    public int AddItem(ItemInstance item)
    {
        int amountToReturn = AddItem_Internal(item.ItemData.ItemId, item.Amount);
        OnLootAdded?.Invoke();
        return amountToReturn;
    }

    public int AddItem(int itemId, int amount)
    {
        int amountToReturn = AddItem_Internal(itemId, amount);
        OnLootAdded?.Invoke();
        return amountToReturn;
    }

    public void AddItems(List<ItemInstance> items)
    {
        foreach (ItemInstance item in items)
            AddItem_Internal(item.ItemData.ItemId, item.Amount);
        OnLootAdded?.Invoke();
    }

    private int AddItem_Internal(int itemId, int amount)
    {
        return items[itemId].AddAmount(amount, totalStorageCapacity[itemId].Amount);

        //for (int i = 0; i < builtFloors.Count; i++)
        //{
        //    if (amount <= 0)
        //        break;

        //    for (int j = 0; j < roomsCountPerFloor; j++)
        //    {
        //        if (amount <= 0)
        //            break;

        //        Building placedBuilding = builtFloors[i].roomBuildingPlaces[j].placedBuilding;
        //        if (placedBuilding)
        //        {
        //            if (placedBuilding.storageComponent)
        //            {
        //                int amountToAdd = placedBuilding.storageComponent.AddItem(itemId, amount);
        //                amount -= amountToAdd;
        //                items[itemId].AddAmount(amountToAdd, totalStorageCapacity[itemId].Amount);
        //            }

        //            if (placedBuilding.BuildingData.BuildingType == BuildingType.Hall)
        //                break;
        //        }
        //    }
        //}

        //return items[itemId].Amount;
    }

    public void SpendItem(int id, int amount)
    {
        items[id].SubtractAmount(amount);
    }

    public void SpendItem(string idName, int amount)
    {
        int index = gameManager.LootList.lootByIdName[idName].ItemId;
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
    public Building TryGetPathToBuilding(BuildingPlace startBuildingPlace, Building targetBuilding, ref List<Building> buildingsPath, bool addStartBuildingToPath = false)
    {
        return TryGetPathToBuilding_Internal(startBuildingPlace, targetBuilding, ref buildingsPath, addStartBuildingToPath);
    }

    public Building TryGetPathToBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath, bool addStartBuildingToPath = false)
    {
        Building targetBuilding = null;
        for (int i = 0; i < builtFloors.Count; i++) {
            Building hall = builtFloors[i].hallBuildingPlace.placedBuilding;
            if (hall && targetBuildingCondition(hall)) {
                targetBuilding = hall;
                break;
            }

            for (int j = 0; j < roomsCountPerFloor; j++) {
                Building room = builtFloors[i].roomBuildingPlaces[j].placedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }  
            }

            if (targetBuilding)
                break;
        }

        return TryGetPathToBuilding_Internal(startBuildingPlace, targetBuilding, ref buildingsPath, addStartBuildingToPath);
    }

    private Building TryGetPathToBuilding_Internal(BuildingPlace startBuildingPlace, Building targetBuilding, ref List<Building> buildingsPath, bool removeStartBuilding = false)
    {
        // Preparing
        buildingsPath.Clear();
        allPaths.Clear();
        allPaths2.Clear();

        int pathIndex = 0;

        allPaths.Add(new List<Building>());

        List<bool> checkedBuildingPlaces = new List<bool>();
        for (int i = 0; i < builtFloors.Count * roomsCountPerFloor; i++)
            checkedBuildingPlaces.Add(false);

        if (startBuildingPlace && startBuildingPlace.floorIndex < firstBuildCityFloorIndex)
            startBuildingPlace = builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCityBuildingPlace];

        // Main
        if (startBuildingPlace || targetBuilding as TowerBuilding) {
            BuildingPlace newStartBuildingPlace = !startBuildingPlace ? builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCityBuildingPlace] : startBuildingPlace;
            Building newTargetBuilding = targetBuilding.GetType() == typeof(Building) ? builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCityBuildingPlace].placedBuilding : targetBuilding;

            BuildingPlace targetBuildingPlace = TryGetTargetBuildingOnFloor(newStartBuildingPlace, newTargetBuilding, null, ref allPaths, ref pathIndex, ref checkedBuildingPlaces);
            if (targetBuildingPlace) {
                for (int i = 0; i < allPaths.Count; i++) {
                    allPaths2.Add(new BuildingPath());

                    allPaths2[i].paths = allPaths[i];

                    if (allPaths[i].Count > 0 && targetBuilding == allPaths[i][allPaths[i].Count - 1] && targetBuilding == allPaths[i][allPaths[i].Count - 1]) {
                        buildingsPath = allPaths[i];
                    }
                }

                for (int i = 0; i < buildingsPath.Count - 1; i++) {
                    Type currentType = buildingsPath[i].GetType();
                    Type nextType = buildingsPath[i + 1].GetType();

                    if (currentType == typeof(ElevatorBuilding)) {
                        if (buildingsPath.Count > i + 2 && buildingsPath[i + 2] && buildingsPath[i].placeIndex == buildingsPath[i + 2].placeIndex) {
                            if (buildingsPath[i + 2].GetType() == currentType) {
                                buildingsPath.RemoveAt(i + 1);
                                i--;
                            }
                        }
                    }
                    else {
                        buildingsPath.RemoveAt(i);
                        i--;
                    }
                }

                //if (removeStartBuilding) {
                //    buildingsPath.RemoveAt(0);
                //}
            }

            if (newTargetBuilding != targetBuilding)
                buildingsPath.Add(targetBuilding);
        }
        else {
            buildingsPath.Add(targetBuilding);
        }

        return targetBuilding;
    }

    private BuildingPlace TryGetTargetBuildingOnFloor(BuildingPlace startBuildingPlace, Building targetBuilding, BuildingPlace lastBuildingPlace, ref List<List<Building>> allPaths, ref int pathIndex, ref List<bool> checkedBuildingPlaces, bool addStartBuildingToPath = false)
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

            if (startBuilding) {
                // Start Building
                int startBuildingPlaceIndex = startBuildingPlace.floorIndex * roomsCountPerFloor + startBuildingPlace.BuildingPlaceIndex;
                if (!checkedBuildingPlaces[startBuildingPlaceIndex]) {
                    checkedBuildingPlaces[startBuildingPlaceIndex] = true;
                    ElevatorBuilding elevatorBuilding = startBuilding as ElevatorBuilding;

                    if (elevatorBuilding) {
                        BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.upConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                        if (upperTargetPlace)
                            return upperTargetPlace;

                        BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.downConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                        if (lowerTargetPlace)
                            return lowerTargetPlace;
                    }
                    else {
                        allPaths[startPathIndex].Add(startBuilding);

                        if (startBuilding == targetBuilding)
                            return startBuilding.buildingPlace;
                    }
                }

                // Side Buildings
                for (int i = 1; i < roomsCountPerFloor / 2; i++) {
                    int index = (startBuildingPlace.BuildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;

                    // Left Buildings
                    if (isNeededToCheckLeftSide) {
                        int leftIndex = (startBuildingPlace.BuildingPlaceIndex + i + roomsCountPerFloor) % roomsCountPerFloor;
                        leftBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, leftIndex);

                        if (leftBuilding) {
                            int buildingPlaceIndex = leftBuilding.floorIndex * roomsCountPerFloor + leftBuilding.placeIndex;
                            if (!checkedBuildingPlaces[buildingPlaceIndex]) {
                                checkedBuildingPlaces[buildingPlaceIndex] = true;
                                ElevatorBuilding elevatorBuilding = leftBuilding as ElevatorBuilding;

                                if (elevatorBuilding) {
                                    BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.upConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (upperTargetPlace)
                                        return upperTargetPlace;

                                    BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.downConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (lowerTargetPlace)
                                        return lowerTargetPlace;
                                }
                                else {
                                    allPaths[startPathIndex].Add(leftBuilding);

                                    if (leftBuilding == targetBuilding)
                                        return leftBuilding.buildingPlace;
                                }
                            }
                        }
                        else {
                            isNeededToCheckLeftSide = false;
                        }
                    }

                    // Right Buildings
                    if (isNeededToCheckRightSide) {
                        int rightIndex = (startBuildingPlace.BuildingPlaceIndex - i + roomsCountPerFloor) % roomsCountPerFloor;
                        rightBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, rightIndex);

                        if (rightBuilding) {
                            int buildingPlaceIndex = rightBuilding.floorIndex * roomsCountPerFloor + rightBuilding.placeIndex;
                            if (!checkedBuildingPlaces[buildingPlaceIndex]) {
                                checkedBuildingPlaces[buildingPlaceIndex] = true;
                                ElevatorBuilding elevatorBuilding = rightBuilding as ElevatorBuilding;

                                if (elevatorBuilding) {
                                    BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.upConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (upperTargetPlace)
                                        return upperTargetPlace;

                                    BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.downConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                    if (lowerTargetPlace)
                                        return lowerTargetPlace;
                                }
                                else  {
                                    allPaths[startPathIndex].Add(rightBuilding);

                                    if (rightBuilding == targetBuilding) {
                                        return rightBuilding.buildingPlace;
                                    }
                                }
                            }
                        }
                        else {
                            isNeededToCheckRightSide = false;
                        }
                    }

                    if (!leftBuilding && !rightBuilding)
                        break;
                }

                // Check Finish Building
                if (isNeededToCheckLeftSide || isNeededToCheckRightSide) {
                    int finishIndex = (startBuildingPlace.BuildingPlaceIndex + (roomsCountPerFloor / 2) + roomsCountPerFloor) % roomsCountPerFloor;
                    finishBuilding = GetBuildingByIndex(startBuildingPlace.floorIndex, finishIndex);

                    if (finishBuilding) {
                        int buildingPlaceIndex = finishBuilding.floorIndex * roomsCountPerFloor + finishBuilding.placeIndex;
                        if (!checkedBuildingPlaces[buildingPlaceIndex]) {
                            checkedBuildingPlaces[buildingPlaceIndex] = true;
                            ElevatorBuilding elevatorBuilding = finishBuilding as ElevatorBuilding;

                            if (elevatorBuilding) {
                                BuildingPlace upperTargetPlace = AddElevatorPath(elevatorBuilding.upConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                if (upperTargetPlace)
                                    return upperTargetPlace;

                                BuildingPlace lowerTargetPlace = AddElevatorPath(elevatorBuilding.downConnectedBuilding as ElevatorBuilding, elevatorBuilding, targetBuilding, lastBuildingPlace, ref pathIndex, ref checkedBuildingPlaces);
                                if (lowerTargetPlace)
                                    return lowerTargetPlace;
                            }
                            else {
                                allPaths[startPathIndex].Add(finishBuilding);

                                if (finishBuilding == targetBuilding)
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

    private BuildingPlace AddElevatorPath(ElevatorBuilding verticalElevatorBuilding, ElevatorBuilding startElevatorBuilding, Building targetBuilding, BuildingPlace lastBuildingPlace, ref int pathIndex, ref List<bool> checkedBuildingPlaces)
    {
        if (verticalElevatorBuilding && verticalElevatorBuilding.buildingPlace != lastBuildingPlace) {
            if (verticalElevatorBuilding.BuildingData.BuildingIdName == startElevatorBuilding.BuildingData.BuildingIdName) {
                pathIndex++;
                allPaths.Add(new List<Building>());
                ElevatorBuilding lastElevatorBuilding = lastBuildingPlace ? lastBuildingPlace.placedBuilding as ElevatorBuilding : null;
                if (lastElevatorBuilding) {
                    if (pathIndex > 1) {
                        for (int i = 1; i < pathIndex; i++) {
                            if (startElevatorBuilding != allPaths[i][i - 1])
                                allPaths[pathIndex].Add(allPaths[i][i - 1]);
                        }
                    }
                }
                allPaths[pathIndex].Add(startElevatorBuilding);
                allPaths[pathIndex].Add(verticalElevatorBuilding);

                return TryGetTargetBuildingOnFloor(verticalElevatorBuilding.buildingPlace, targetBuilding, startElevatorBuilding.buildingPlace, ref allPaths, ref pathIndex, ref checkedBuildingPlaces);
            }
            else
                return null;
        }
        else
            return null;
    }
}
