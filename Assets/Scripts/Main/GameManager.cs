using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;

    [SerializeField] private PlayerController playerController = null;

    [Header("Content")]
    private const string listsFolder = "Lists";
    public ItemsList lootList { get; private set; } = null;
    public buildingsList buildingsList { get; private set; } = null;
    public boatsList boatsList { get; private set; } = null;
    public LootContainersList lootContainersList { get; private set; } = null;
    public CreaturesList creaturesList { get; private set; } = null;

    // Buildings
    [Header("Buildings")]
    [SerializeField] private NavMeshSurface towerNavMeshSurface = null;
    [field: SerializeField] public List<FloorBuilding> builtFloors { get; private set; } = new List<FloorBuilding>();
    [field: SerializeField] public PierBuilding pierBuilding { get; private set; } = null;

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

    public List<List<ElevatorBuilding>> elevatorGroups { get; private set; } = new List<List<ElevatorBuilding>>();

    public Building buildingToPlace { get; private set; }

    // Items
    public List<ItemInstance> startResources = new List<ItemInstance>();
    public ItemInstance[] items;
    public int[] totalStorageCapacity;

    [Header("NPC")]
    public List<Creature> residents { get; private set; } = new List<Creature>();
    private const int startResidentsCount = 2;
    public int employedResidentCount { get; private set; } = 0;
    public int unemployedResidentsCount { get; private set; } = 0;

    [Header("Boats")]
    public List<Boat> spawnedBoats { get; private set; } = new List<Boat>();

    public static event Action OnConstructionStartPlaced;
    public static event Action OnConstructionPlaced;
    public static event Action OnConstructionDestroyed;
    public static event Action<ItemInstance> OnLootAdded;
    public static event Action OnStorageCapacityUpdated;
    public event Action OnResidentsAdded;
    public event Action<Creature> OnResidentAdded;
    public event Action<Creature> OnResidentRemoved;

    public List<List<Building>> allPaths = new List<List<Building>>();
    public List<BuildingPath> allPaths2 = new List<BuildingPath>();

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;

    [Header("NPC")]
    [field: SerializeField] public Transform entitySpawnPosition { get; private set; } = null;
    public const float maxSpawnRange = 5f;

    // Wind
    public Vector2 windDirection { get; private set; } = Vector2.zero;
    public float windRotation { get; private set; } = 0;
    private Vector2 newWindDirection = Vector2.zero;

    public const float windSpeed = 15.0f;
    private const float windChangingSpeed = 0.05f;
    private float windDirectionChangeRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    // Other
    public const float autoSaveFrequency = 1;
    public const float triggerLootContainerRadius = 150f;
    public const float demolitionResourceRefundRate = 0.2f;

    public const float collectLootFlickingMultiplier = 0.35f;

    public static SaveData saveData = null;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        buildingsList = Resources.Load<buildingsList>($"{listsFolder}/buildingsList");
        creaturesList = Resources.Load<CreaturesList>($"{listsFolder}/CreaturesList");
        boatsList = Resources.Load<boatsList>($"{listsFolder}/BoatsList");
        lootList = Resources.Load<ItemsList>($"{listsFolder}/LootList");
        lootContainersList = Resources.Load<LootContainersList>($"{listsFolder}/LootContainersList");

        playerController.Initialize();
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitializeAsync();
    }

    private void OnEnable()
    {
        EventBus.Instance.onConstructionPlacePressed += OnConstructionPlacePressed;
        EventBus.Instance.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
        PlayerUIManager.OnBuildStopPlacing += HideAllBuildigPlaces;

        ConstructionComponent.onAnyConstructionStartConstructing += OnBuildingStartConstructing;
        ConstructionComponent.onAnyConstructionFinishConstructing += OnBuildingFinishConstructing;
        ConstructionComponent.onAnyConstructionDemolished += OnConstructionDemolished;

        Creature.OnWorkerAdd += AddWorker;
        Creature.OnWorkerRemove += RemoveWorker;

        Boat.OnBoadDestroyed += OnBoatDestroyed;
    }

    private void OnDisable()
    {
        EventBus.Instance.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
        PlayerUIManager.OnBuildStopPlacing -= HideAllBuildigPlaces;

        ConstructionComponent.onAnyConstructionStartConstructing -= OnBuildingStartConstructing;
        ConstructionComponent.onAnyConstructionFinishConstructing -= OnBuildingFinishConstructing;
        ConstructionComponent.onAnyConstructionDemolished -= OnConstructionDemolished;

        Creature.OnWorkerAdd -= AddWorker;
        Creature.OnWorkerRemove -= RemoveWorker;

        Boat.OnBoadDestroyed -= OnBoatDestroyed;
    }

    private void Start()
    {
        StartCoroutine(AutosaveCoroutine());
        TimerManager.Initialize();

        ChangeWind();
        windDirection = newWindDirection;

        new LootManager();
        string worldName = SaveManager.Instance.saveWorldName;
        saveData = SaveSystem.GetSaveDataByWorldName(worldName);

        InitializeItems();
        LoadBuildings(saveData);
        LoadResources(saveData);
        CreateEntities(saveData);
        CreateLoads(saveData);
        StartCoroutine(LoadCityAsync(saveData));

        playerController.Load(saveData);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private void Update()
    {
        playerController.Tick();

        ChangingWind();
        TimerManager.Tick();
    }

    public void Load(SaveData data)
    {
        InitializeItems();
        LoadBuildings(data);
        LoadResources(data);
        CreateEntities(data);
        CreateLoads(data);
        LoadCityAsync(data);
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
                    PlaceBuilding(buildingsList.buildings[0], builtFloors[i - 1].floorBuildingPlace, 0, false);
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

                                Building buildingToPlace = buildingsList.buildings[buildingId];
                                Building building = null;
                                BuildingType buildingType = buildingsList.buildings[buildingId].BuildingData.BuildingType;

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
                    pierBuilding.InitializeBuilding(null, pierBuilding.constructionComponent.isUnderConstruction, pierBuilding.LevelIndex);
                }
                else {
                    BuildingPlace hallPlace = builtFloors[i].hallBuildingPlace;
                    Building hall = hallPlace.placedBuilding;
                    if (hall)
                        PlaceBuilding(hall, hallPlace, hall.LevelIndex, hall.constructionComponent.isUnderConstruction);

                    for (int j = 0; j < roomsCountPerFloor; j++) {
                        BuildingPlace roomPlace = builtFloors[i].roomBuildingPlaces[j];
                        Building room = roomPlace.placedBuilding;
                        if (room)
                            PlaceBuilding(room, roomPlace, room.LevelIndex, room.constructionComponent.isUnderConstruction);
                    }

                    // Pier Building
                    pierBuilding.InitializeBuilding(null, pierBuilding.constructionComponent.isUnderConstruction, pierBuilding.LevelIndex);
                }
            }

            if (data != null) {
                for (int i = builtFloors.Count - 1; i > data.builtFloorsCount; i--) {
                    builtFloors[i].constructionComponent.StartDemolishing();
                }
            }

            cityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + floorHeight;
        }
        else
            Debug.LogError("The count of builtFloors is 0");
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
            position = entitySpawnPosition.position;
            rotation = entitySpawnPosition.rotation;
            for (int i = 0; i < startResidentsCount; i++) {
                float x = UnityEngine.Random.Range(position.x - maxSpawnRange, position.x + maxSpawnRange);
                float y = position.y;
                float z = UnityEngine.Random.Range(position.z - maxSpawnRange, position.z + maxSpawnRange);
                Vector3 finalPosition = new Vector3(x, y, z);
                CreateResident(finalPosition, rotation);
            }
        }

        OnResidentsAdded?.Invoke();
    }

    private void LoadEntities(SaveData data)
    {
        if (data != null) {
            for (int i = 0; i < residents.Count; i++) {
                Creature resident = residents[i];

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
                Creature resident = residents[i];

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
                    PlaceBoat(this.boatsList.boats[id], isUnderConstruction, j, isFloating, isReturning, health, positionX, positionZ, rotationY);
                }
            }
        }
        else {
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
        int length = lootList.Items.Length;
        totalStorageCapacity = new int[length];
        items = new ItemInstance[length];
        for (int i = 0; i < length; i++) {
            ItemData data = lootList.Items[i];
            int id = data.ItemId;
            items[id] = new ItemInstance(data);
        }
    }

    private void LoadResources(SaveData data)
    {
        if (data != null) {
            if (data.resourcesAmount != null) {
                for (int i = 0; i < data.resourcesAmount.Length; i++) {
                    AddItem(i, data.resourcesAmount[i]);
                }
            }
        }
        else {
            for (int i = 0; i < startResources.Count; i++) {
                AddItem(startResources[i].ItemData.ItemId, startResources[i].Amount);
            }
        }
    }

    private IEnumerator LoadCityAsync(SaveData data)
    {
        while (bakeNavMeshCoroutine != null) {
            Debug.Log("bakeNavMeshCoroutine");
            yield return null;
        }

        LoadEntities(data);
    }

    private void CreateResident(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Creature resident = Instantiate(creaturesList.resident, spawnPosition, spawnRotation);
        resident.Initialize();
        AddResident(resident);
        resident.navMeshAgent.enabled = false;
    }

    private void AddResident(Creature resident)
    {
        residents.Add(resident);
        unemployedResidentsCount++;

        OnResidentAdded?.Invoke(resident);
    }

    private void RemoveResident(Creature resident)
    {
        OnResidentRemoved?.Invoke(resident);
        //residents.Remove(residents[]);
        Creature.Destroy(resident);
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
        for (int i = 0; i < roomsCountPerFloor; i++)
            lastPlacedRoomsFloorIndex.Add(0);

        int lastPlacedHallFloorIndex = 0;

        for (int i = 0; i < builtFloors.Count; i++) {
            // Set room heights
            bool isRoomPlacedOnFloor = false;
            for (int j = 0; j < roomsCountPerFloor; j++) {
                if (builtFloors[i].roomBuildingPlaces[j].placedBuilding)
                    isRoomPlacedOnFloor = true;

                if (builtFloors[i].roomBuildingPlaces[j].placedBuilding)
                    lastPlacedRoomsFloorIndex[j] = i;

                for (int k = lastPlacedRoomsFloorIndex[j]; k <= i; k++) {
                    builtFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesAbove = i - k;

                    if (k != lastPlacedRoomsFloorIndex[j])
                        builtFloors[k].roomBuildingPlaces[j].emptyBuildingPlacesBelow = k - lastPlacedRoomsFloorIndex[j] - 1;
                }
            }

            // Set hall heights
            if (builtFloors[i].hallBuildingPlace.placedBuilding || isRoomPlacedOnFloor) {
                lastPlacedHallFloorIndex = i;
            }

            for (int k = lastPlacedHallFloorIndex; k <= i; k++) {
                builtFloors[k].hallBuildingPlace.emptyBuildingPlacesAbove = i - k;

                if (k != lastPlacedHallFloorIndex)
                    builtFloors[k].hallBuildingPlace.emptyBuildingPlacesBelow = k - lastPlacedHallFloorIndex - 1;
            }
        }
    }

    private void UpdateCityHeight()
    {
        cityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + floorHeight;
    }

    public void ShowBuildingPlacesByType(Building building)
    {
        HideAllBuildigPlaces();

        for (int i = 0; i < builtFloors.Count; i++) {
            builtFloors[i].ShowBuildingPlacesByType(building);
        }
    }

    public void HideBuildingPlacesByType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.Room) {
            for (int i = 0; i < builtFloors.Count; i++) {
                for (int j = 0; j < roomsCountPerFloor; j++) {
                    if (builtFloors[i].roomBuildingPlaces[j] != null) {
                        builtFloors[i].roomBuildingPlaces[j].HideBuildingPlace();
                    }
                }
            }
        }
        else if (buildingType == BuildingType.Hall) {
            for (int i = 0; i < builtFloors.Count; i++) {
                if (builtFloors[i].hallBuildingPlace != null) {
                    builtFloors[i].hallBuildingPlace.HideBuildingPlace();
                }
            }
        }
        else if (buildingType == BuildingType.FloorFrame) {
            for (int i = 0; i < builtFloors.Count; i++) {
                if (builtFloors[i].floorBuildingPlace != null) {
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
        for (int i = 0; i < builtFloors.Count; i++) {
            builtFloors[i].HideAllBuildingPlaces();
        }
    }

    // Buildings
    private void OnConstructionPlacePressed(BuildingPlace place)
    {
        PlaceBuilding(buildingToPlace, place, 0, true);
    }

    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        ConstructionComponent construction = widget.constructionComponent;
        Building building = construction.GetComponent<Building>();
        Boat boat = construction.GetComponent<Boat>();
        if (building) {
            ShowBuildingPlacesByType(building);
            buildingToPlace = building;
        }
        else if (boat)
            PlaceBoat(boat, true);
    }

    public Building PlaceBuilding(Building building, BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    {
        Building spawnedBuilding = buildingPlace.placedBuilding;
        if (buildingPlace) {
            if (!spawnedBuilding)
                spawnedBuilding = Instantiate(building, buildingPlace.transform);

            buildingPlace.SetPlacedBuilding((TowerBuilding)spawnedBuilding);
            BuildingType type = spawnedBuilding.BuildingData.BuildingType;
            if (type == BuildingType.Room) {
                currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
            }
            else if (type == BuildingType.Hall) {
                if (!spawnedBuilding.isInitialized) {
                    if (currentRoomsNumberOnFloor[buildingPlace.floorIndex] == 0) {
                        for (int i = 0; i < roomsCountPerFloor; i++) {
                            builtFloors[buildingPlace.floorIndex].roomBuildingPlaces[i].SetPlacedBuilding((TowerBuilding)spawnedBuilding);
                            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
                        }
                    }
                }
            }
            spawnedBuilding.InitializeBuilding(buildingPlace, isUnderConstruction, levelIndex);

            UpdateEmptyBuildingPlacesCount();
            HideAllBuildigPlaces();

            OnConstructionPlaced?.Invoke();
        }

        BakeNavMeshSurface();

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

        Boat spawnedBoat = Boat.Instantiate(boat, position, rotation);
        spawnedBoat.Initialize(isUnderConstruction, dockIndex.Value, isFloating, isReturningToDock, health);
        spawnedBoats[dockIndex.Value] = spawnedBoat;
    }

    public void DemolishContruction(Building building)
    {
        building.constructionComponent.StartDemolishing();
    }

    private void OnBuildingStartConstructing(ConstructionComponent construction)
    {
        int levelIndex = construction.ownedBuilding.LevelIndex;

        Building building = construction.GetComponent<Building>();
        if (building) {
            OnBuildingFinishConstructing(construction);
            //building.FinishConstructing();
        }
    }

    private void OnBuildingFinishConstructing(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();
        if (building) {
            building.FinishConstructing();

            FloorBuilding floorBuilding = building as FloorBuilding;
            ElevatorBuilding elevatorBuilding = building as ElevatorBuilding;
            if (floorBuilding) {
                InitializeFloor(floorBuilding);
            }
            else if (elevatorBuilding) {
                if (elevatorGroups.Count <= elevatorBuilding.elevatorGroupId) {
                    List<ElevatorBuilding> elevatorGroup = new List<ElevatorBuilding>();
                    elevatorGroups.Add(elevatorGroup);
                }
                elevatorGroups[elevatorBuilding.elevatorGroupId].Add(elevatorBuilding);
            }

            if (building.BuildingData.BuildingType != BuildingType.Environment && building.storageComponent) {
                int level = building.LevelIndex;
                if (level > 1) {
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
        int nextLevelIndex = building.LevelIndex + (building.constructionComponent.isRuined ? 0 : 1);

        if (building.ConstructionLevelsData.Count() > nextLevelIndex) {
            bool isResourcesToUpgradeEnough = true;

            int index = 0;
            int amount = 0;
            ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

            for (int i = 0; i < resourcesToUpgrade.Length; i++) {
                index = resourcesToUpgrade[i].ItemData.ItemId;
                amount = resourcesToUpgrade[i].Amount;

                if (items[index].Amount < amount) {
                    isResourcesToUpgradeEnough = false;
                    break;
                }
            }

            if (isResourcesToUpgradeEnough) {
                for (int i = 0; i < resourcesToUpgrade.Length; i++) {
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
        ItemInstance[] resourceToBuilds = construction.constructionLevelsData[construction.ownedBuilding.LevelIndex].ResourcesToBuild;
        for (int i = 0; i < resourceToBuilds.Length; i++) {
            int id = resourceToBuilds[i].ItemData.ItemId;
            int amount = (int)math.ceil(resourceToBuilds[i].Amount * GameManager.demolitionResourceRefundRate);
            AddItem(id, amount);
        }
    }

    private void BakeNavMeshSurface()
    {
        if (bakeNavMeshCoroutine != null)
            StopCoroutine(bakeNavMeshCoroutine);
        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();
        bakeNavMeshCoroutine = null;
    }

    // Get Buildings
    public TowerBuilding GetBuildingByIndex(int floorIndex, int buildingPlaceIndex)
    {
        TowerBuilding building = null;

        bool isFloorIndexMoreMin = floorIndex >= 0;
        bool isFloorIndexLessMax = floorIndex < builtFloors.Count;
        bool isBuildingPlaceIndexMoreMin = buildingPlaceIndex >= 0;
        bool isBuildingPlaceIndexLessMax = buildingPlaceIndex < roomsCountPerFloor;

        if (isFloorIndexMoreMin && isFloorIndexLessMax && isBuildingPlaceIndexMoreMin && isBuildingPlaceIndexLessMax) {
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
        for (int i = 0; i < storageLevelData.storageItems.Length; i++) {
            int id = storageLevelData.storageItems[i].ItemData.ItemId;
            int changeValue = storageLevelData.storageItems[i].Amount;

            if (isIncreasing)
                totalStorageCapacity[id] += changeValue;
            else
                totalStorageCapacity[id] -= changeValue;
        }

        for (int i = 0; i < storageLevelData.storageItemCategories.Length; i++) {
            for (int j = 0; j < lootList.Items.Length; j++) {
                if (items[j].ItemData.ItemCategory == storageLevelData.storageItemCategories[i].itemCategory) {
                    int changeValue = storageLevelData.storageItemCategories[i].amount;

                    if (isIncreasing)
                        totalStorageCapacity[j] += changeValue;
                    else
                        totalStorageCapacity[j] -= changeValue;
                }
            }
        }

        if (isNeededToUpdate)
            OnStorageCapacityUpdated?.Invoke();
    }

    public int AddItem(ItemInstance item)
    {
        int amountToReturn = AddItem_Internal(item.ItemData.ItemId, item.Amount);
        return amountToReturn;
    }

    public int AddItem(int itemId, int amount)
    {
        int amountToReturn = AddItem_Internal(itemId, amount);
        return amountToReturn;
    }

    public void AddItems(List<ItemInstance> items)
    {
        foreach (ItemInstance item in items)
            AddItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    private int AddItem_Internal(int itemId, int amount)
    {
        ItemInstance item = items[itemId];
        item.AddAmount(amount, totalStorageCapacity[itemId]);
        OnLootAdded?.Invoke(item);
        return item.Amount;
    }

    public void SpendItem(int id, int amount)
    {
        items[id].SubtractAmount(amount);
    }

    public void SpendItems(List<ItemInstance> itemsToSpend)
    {
        for (int i = 0; i < itemsToSpend.Count; i++) {
            int id = (int)itemsToSpend[i].ItemData.ItemId;
            int amount = itemsToSpend[i].Amount;

            SpendItem(id, amount);
        }
    }

    // Path finding
    public bool TryGetPathToBuilding(BuildingPlace startBuildingPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        return TryGetPathToBuilding_Internal(startBuildingPlace, targetBuilding, ref buildingsPath);
    }

    public bool TryGetPathToBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
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

        return TryGetPathToBuilding_Internal(startBuildingPlace, targetBuilding, ref buildingsPath);
    }

    private bool TryGetPathToBuilding_Internal(BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
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

        if (!startPlace || startPlace.floorIndex < firstBuildCityFloorIndex)
            startPlace = builtFloors[firstBuildCityFloorIndex].roomBuildingPlaces[firstBuildCityBuildingPlace];

        // Main
        HashSet<Building> visitedBuildings = new HashSet<Building>();
        bool found = FindPath(startPlace, targetBuilding, allPaths, ref pathIndex, visitedBuildings);

        for (int i = 0; i < allPaths.Count; i++) {
            allPaths2.Add(new BuildingPath());
            for (int j = 0; j < allPaths[i].Count; j++) {
                allPaths2[i].paths.Add(allPaths[i][j]);
            }
        }

        if (found) {
            buildingsPath = allPaths[allPaths.Count - 1].ToList();
        }

        return found;
    }

    private bool FindPath(BuildingPlace startPlace, Building targetBuilding, List<List<Building>> buildingPaths, ref int pathIndex, HashSet<Building> visitedBuildings, int enterPathIndex = 0, int pathLength = 0)
    {
        Building startBuilding = startPlace.placedBuilding;
        if (!startBuilding) {
            Debug.LogError("startBuilding == NULL");
            return false;
        }

        if (!visitedBuildings.Add(startBuilding)) return false;

        // Connect path with parent path
        if (pathIndex > 0 && buildingPaths[pathIndex].Count == 0) {
            for (int i = 0; i < pathLength; i++) {
                buildingPaths[pathIndex].Add(buildingPaths[enterPathIndex][i]);
            }
        }

        // Add this building as new
        buildingPaths[pathIndex].Add(startBuilding);
        if (startBuilding == targetBuilding)
            return true;

        TowerBuilding towerBuilding = startBuilding as TowerBuilding;
        if (!towerBuilding) {
            Debug.LogError("startTowerBuilding is NULL");
            return false;
        }

        ElevatorBuilding startElevator = towerBuilding as ElevatorBuilding;
        int enterIndex = pathIndex;
        int currentPathLength = buildingPaths[enterPathIndex].Count;

        // Get new paths
        int buildingsCount = 0;
        foreach (TowerBuilding direction in startElevator ? startElevator.NeighborBuildings(NeighborMask.All) : towerBuilding.NeighborBuildings(NeighborMask.Horizontal)) {
            if (!direction) continue;
            if (visitedBuildings.Contains(direction)) continue;

            if (buildingsCount > 0) {
                pathIndex++;
                buildingPaths.Add(new List<Building>());
            }

            if (FindPath(direction.buildingPlace, targetBuilding, buildingPaths, ref pathIndex, visitedBuildings, enterIndex, currentPathLength))
                return true;
            buildingsCount++;
        }
        return false;
    }

    private void ChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeRate)
        {
            ChangeWind();
        }

        windDirection = math.lerp(windDirection, newWindDirection, windChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float xAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        float yAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        newWindDirection = new Vector2(xAxis, yAxis).normalized;

        windDirectionChangeTime = Time.time;
    }

    private IEnumerator AutosaveCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(autoSaveFrequency);
            SaveSystem.SaveData(playerController);
        }
    }
}
