using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Android.Gradle.Manifest;
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

public class CityData
{
    public int floorsCount = 0;
}

public class CityManager : MonoBehaviour
{
    public static CityManager Instance { get; private set; } = null;

    [SerializeField] private PlayerController playerController = null;

    // Buildings
    [Header("Buildings")]
    [SerializeField] private NavMeshSurface towerNavMeshSurface = null;
    [SerializeField] private List<FloorFrameModule> builtFloors = new List<FloorFrameModule>();
    public List<FloorFrameModule> BuiltFloors => builtFloors;

    [Header("Environment Buildings")]
    [SerializeField] private GroundBuilding towerGate = null;
    public GroundBuilding TowerGate => towerGate;
    [SerializeField] private GroundBuilding pierBuilding = null;
    public GroundBuilding PierBuilding => pierBuilding;

    public List<int> currentRoomsNumberOnFloor { get; private set; } = new List<int>();

    public const int floorHeight = 5;
    public const int firstFloorHeight = 5;

    public const int roomsCountPerFloor = 8;
    public const int roomsCountPerSide = 3;
    public const int roomsWidth = 8;
    public const int floorWidth = 24;
    //public const int firstBuildCityFloorIndex = 1;
    public const int firstBuildCityBuildingPlace = 1;
    public float cityHeight { get; private set; } = 0;

    public List<List<ElevatorBuildingModule>> elevatorGroups { get; private set; } = new List<List<ElevatorBuildingModule>>();

    public Building buildingToPlace { get; private set; }

    // Items
    public List<ItemInstance> startResources = new List<ItemInstance>();
    public ItemInstance[] items;
    public int[] maxItemsAmount;

    [Header("NPC")]
    public List<Creature> residents { get; private set; } = new List<Creature>();
    private const int startResidentsCount = 2;
    public int employedResidentCount { get; private set; } = 0;
    public int unemployedResidentsCount { get; private set; } = 0;

    [Header("Boats")]
    public List<Boat> spawnedBoats { get; private set; } = new List<Boat>();

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    private bool isNavMeshBuilt = false;

    [Header("NPC")]
    [field: SerializeField] public Transform entitySpawnPosition { get; private set; } = null;
    public const float maxSpawnRange = 5f;

    // Other
    public const float autoSaveFrequency = 1;
    public const float triggerLootContainerRadius = 150f;
    public const float demolitionResourceRefundRate = 0.2f;

    public const float collectLootFlickingMultiplier = 0.35f;

    IEnumerable<GroundBuilding> EnvironmentBuildings()
    {
        yield return towerGate;
        yield return pierBuilding;
    }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerController.Initialize();
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitializeAsync();
    }

    private void OnEnable()
    {
        // Construction
        EventBus.onBuildingPlacePressed += OnBuildingPlacePressed;
        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
        EventBus.onBuildingInitialized += OnBuildingInitialized;
        EventBus.onBuildingPlaced += OnConstructionBuilt;

        // Production Module
        EventBus.onProductionModuleClicked += OnProductionModuleClicked;

        Creature.OnWorkerAdd += AddWorker;
        Creature.OnWorkerRemove += RemoveWorker;

        Boat.OnBoadDestroyed += OnBoatDestroyed;
    }

    private void OnDisable()
    {
        EventBus.onBuildingPlacePressed -= OnBuildingPlacePressed;
        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
        EventBus.onBuildingInitialized -= OnBuildingInitialized;
        EventBus.onBuildingPlaced -= OnConstructionBuilt;

        Creature.OnWorkerAdd -= AddWorker;
        Creature.OnWorkerRemove -= RemoveWorker;

        Boat.OnBoadDestroyed -= OnBoatDestroyed;
    }

    private void Start()
    {
        TimerManager.Initialize();

        SaveData saveData = SaveManager.Instance.saveData;

        LoadBuildings(saveData);
        LoadItems(saveData);
        CreateCreatures(saveData);
        //CreateBoats(saveData);
        StartCoroutine(LoadCityAsync(saveData));

        playerController.Load(saveData);

        //StartCoroutine(AutosaveCoroutine());
    }

    private void Update()
    {
        playerController.Tick();

        TimerManager.Tick();
    }

    private void LoadBuildings(SaveData data)
    {
        LoadEnvironmentBuildings(data);
        LoadFloorFrames();
        LoadTowerBuildings(data);
    }

    private void LoadEnvironmentBuildings(SaveData data)
    {
        GroundBuildingEntry[] groundBuildingData;
        if (data != null)
            groundBuildingData = data.groundBuildingsData;
        else
            groundBuildingData = new GroundBuildingEntry[EnvironmentBuildings().Count()];

        int i = 0;
        foreach (var building in EnvironmentBuildings()) {
            if (i >= groundBuildingData.Length) break;
            GroundBuildingEntry groundBuildingEntry = groundBuildingData[i];
            i++;
            if (data != null && groundBuildingEntry.id != building.BuildingData.BuildingId) continue;

            building.Init(groundBuildingEntry);
        }
    }

    private void LoadFloorFrames()
    {
        int floorsCount = builtFloors.Count;
        for (int i = 0; i < floorsCount; i++) {
            var data = new TowerBuildingEntry { floorIndex = i };
            FloorFrameModule floor = builtFloors[i];
            floor.OwnedBuilding.Init(data);
        }
    }

    private void LoadTowerBuildings(SaveData saveData)
    {
        if (saveData != null) {
            TowerBuildingEntry[] towerBuildingsData = saveData.towerBuildingsData;
            foreach (var data in towerBuildingsData) {
                if (data == null) {
                    Debug.LogError($"entry was not found in towerBuildingsData");
                    continue;
                }

                if (data.placeIndex == 0) {
                    TowerBuilding placedBuilding = builtFloors[data.floorIndex].hallBuildingPlace.placedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id)
                        placedBuilding.Demolish();

                    if (data.id >= 0)
                        BuildingFactory.CreateBuilding(data.id, data);
                }
                else {
                    TowerBuilding placedBuilding = builtFloors[data.floorIndex].roomBuildingPlaces[data.placeIndex].placedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id)
                        placedBuilding.Demolish();

                    if (data.id >= 0)
                        BuildingFactory.CreateBuilding(data.id, data);
                }
            }
        }
        else {
            int floorsCount = builtFloors.Count;
            for (int i = 0; i < floorsCount; i++) {
                FloorFrameModule floor = builtFloors[i];

                // Hall
                var hallData = new TowerBuildingEntry { floorIndex = i, placeIndex = 0 };
                floor.hallBuildingPlace.placedBuilding?.Init(hallData);

                // Rooms
                for (int j = 0; j < roomsCountPerFloor; j++) {
                    TowerBuilding room = floor.roomBuildingPlaces[j].placedBuilding;
                    if (!room) continue;

                    var roomData = new TowerBuildingEntry { floorIndex = i, placeIndex = j };
                    room.Init(roomData);
                }
            }
        }
    }

    private void CreateCreatures(SaveData data)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        if (data != null) {
            for (int i = 0; i < data.residentsCount; i++) {
                position = new Vector3(data.residentPositionsX[i], data.residentPositionsY[i], data.residentPositionsZ[i]);
                rotation = Quaternion.identity;
                Creature resident = CreateResident(position, rotation);
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
    }

    private void LoadCreatures(SaveData data)
    {
        for (int i = 0; i < residents.Count; i++) {
            Creature resident = residents[i];

            // Enable Nav Mesh
            resident.navMeshAgent.enabled = true;

            if (data != null) {

                // Set Current Building
                if (data.residentCurrentBuildingIndexes != null && data.residentCurrentBuildingIndexes.Length > i && data.residentCurrentBuildingIndexes[i] >= 0) {
                    Building building = builtFloors[(data.residentCurrentBuildingIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentCurrentBuildingIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (building)
                        resident.EnterBuilding(building);
                }

                // Set Work Building
                if (data.residentTowerBuildingWorkIndexes != null && data.residentTowerBuildingWorkIndexes.Length > i && data.residentTowerBuildingWorkIndexes[i] >= 0) {
                    Building building = builtFloors[(data.residentTowerBuildingWorkIndexes[i] / roomsCountPerFloor)].roomBuildingPlaces[data.residentTowerBuildingWorkIndexes[i] % roomsCountPerFloor].placedBuilding;
                    if (building)
                        resident.SetWork(building);
                }

                if (data.npcElevatorPassengerStates != null && data.npcElevatorPassengerStates.Length > i && data.npcElevatorPassengerStates[i] >= 0) {
                    ElevatorPassengerState state = (ElevatorPassengerState)data.npcElevatorPassengerStates[i];
                    resident.SetElevatorPassengerState(state);
                }
            }
        }
    }

    //private void CreateBoats(SaveData data)
    //{
    //    if (data != null) {
    //        if (data.spawnedBoatIds != null) {
    //            for (int j = 0; j < data.spawnedBoatIds.Length; j++) {
    //                int id = data.spawnedBoatIds[j];
    //                bool isUnderConstruction = data.spawnedBoatsAreUnderConstruction[j];
    //                bool isFloating = data.spawnedBoatsAreFloating[j];
    //                bool isReturning = data.spawnedBoatsAreReturning[j];
    //                float health = data.spawnedBoatsHealth[j];
    //                float positionX = data.spawnedBoatPositionsX[j];
    //                float positionZ = data.spawnedBoatPositionsZ[j];
    //                float rotationY = data.spawnedBoatRotationsY[j];
    //                PlaceBoat(BoatsList.Instance.boats[id], isUnderConstruction, j, isFloating, isReturning, health, positionX, positionZ, rotationY);
    //            }
    //        }
    //    }
    //    else {
    //        for (int j = 0; j < spawnedBoats.Count; j++) {
    //            if (spawnedBoats[j]) {
    //                PierConstruction construction = pierBuilding.ConstructionComponent.SpawnedConstruction as PierConstruction;
    //                spawnedBoats[j].Init(false, j);
    //                spawnedBoats[j].transform.position = construction.BoatDockPositions[j].position;
    //                spawnedBoats[j].transform.rotation = construction.BoatDockPositions[j].rotation;
    //            }
    //        }
    //    }
    //}

    private void LoadItems(SaveData data)
    {
        int length = ItemsList.Instance.Items.Length;
        maxItemsAmount = new int[length];
        items = new ItemInstance[length];
        for (int i = 0; i < length; i++) {
            ItemData itemData = ItemsList.Instance.Items[i];
            int id = itemData.ItemId;
            items[id] = new ItemInstance(itemData);
        }

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

        isNavMeshBuilt = true;

        LoadCreatures(data);
    }

    private Creature CreateResident(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Creature resident = Instantiate(CreaturesList.Instance.resident, spawnPosition, spawnRotation);
        resident.Initialize();
        AddResident(resident);
        if (!isNavMeshBuilt)
            resident.navMeshAgent.enabled = false;
        return resident;
    }

    private void AddResident(Creature resident)
    {
        residents.Add(resident);
        unemployedResidentsCount++;
        EventBus.InvokeResidentAdded(resident);
    }

    private void RemoveResident(Creature resident)
    {
        Destroy(resident);
        unemployedResidentsCount++;
        EventBus.InvokeResidentRemoved(resident);
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
    private void OnBuildingInitialized(Building building)
    {
        var floorFrame = building.GetComponent<FloorFrameModule>();
        if (floorFrame) {
            if (builtFloors.Count == (floorFrame.OwnedBuilding as TowerBuilding).floorIndex)
                builtFloors.Add(floorFrame);

            currentRoomsNumberOnFloor.Add(0);

            UpdateEmptyBuildingPlacesCount();
            UpdateCityHeight();
        }
    }

    private void UpdateEmptyBuildingPlacesCount()
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
    // Buildings
    private void OnBuildingPlacePressed(BuildingPlace place)
    {
        if (buildingToPlace as TowerBuilding) {
            TowerBuildingEntry towerData = new TowerBuildingEntry();
            towerData.floorIndex = place.floorIndex;
            towerData.placeIndex = place.PlaceIndex;
            BuildingFactory.CreateBuilding(buildingToPlace as TowerBuilding, towerData);
        }
        else {
            Debug.LogError("buildingToPlace is not TowerBuilding");
        }
    }

    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        EventBus.InvokeOnBuildingStartPlacing(buildingToPlace);
    }

    //public Building PlaceBuilding(TowerBuilding buildingToPlace, BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    //{
    //    if (!buildingToPlace) {
    //        Debug.LogError("building is NULL");
    //        return null;
    //    }
    //    if (!buildingPlace) {
    //        Debug.LogError("buildingPlace is NULL");
    //        return null;
    //    }

    //    TowerBuilding spawnedBuilding = buildingPlace.placedBuilding;
    //    if (spawnedBuilding && spawnedBuilding.isInitialized) {
    //        buildingPlace.SetPlacedBuilding(spawnedBuilding);
    //        return spawnedBuilding;
    //    }

    //    // Spawn
    //    if (!spawnedBuilding) {
    //        spawnedBuilding = Instantiate(buildingToPlace, buildingPlace.transform);
    //    }

    //    // Initialize
    //    if (!spawnedBuilding.isInitialized) {
    //        spawnedBuilding.Init(buildingPlace, isUnderConstruction, levelIndex);
    //    }

    //    // Set Building to Place
    //    BuildingType type = spawnedBuilding.BuildingData.BuildingType;
    //    if (type == BuildingType.Room) {
    //        buildingPlace.SetPlacedBuilding(spawnedBuilding);
    //        currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
    //    }
    //    else if (type == BuildingType.Hall) {
    //        builtFloors[buildingPlace.floorIndex].hallBuildingPlace.SetPlacedBuilding(spawnedBuilding);
    //        for (int i = 0; i < roomsCountPerFloor; i++) {
    //            builtFloors[buildingPlace.floorIndex].roomBuildingPlaces[i].SetPlacedBuilding(spawnedBuilding);
    //            currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;
    //        }
    //    }

    //    UpdateEmptyBuildingPlacesCount();
    //    HideAllBuildigPlaces();
    //    BakeNavMeshSurface();

    //    return spawnedBuilding;
    //}

    //public void PlaceBoat(Boat boat, bool isUnderConstruction = false, int? dockIndex = null, bool isFloating = false, bool isReturningToDock = false, float? health = null, float? positionX = null, float? positionZ = null, float? rotationY = null)
    //{
    //    //pierBuilding.CreateBoat(boat, isUnderConstruction, dockIndex, isFloating, isReturningToDock, health, positionX, positionZ, rotationY);

    //    PierConstruction pierConstruction = pierBuilding.ConstructionComponent.SpawnedConstruction as PierConstruction;
    //    if (dockIndex == null) {
    //        for (int i = 0; i < spawnedBoats.Count; i++) {
    //            if (!spawnedBoats[i]) {
    //                dockIndex = i;
    //                break;
    //            }
    //        }
    //    }

    //    Vector3 position = Vector3.zero /*pierConstruction.BoatDockPositions[dockIndex.Value].position*/;
    //    if (positionX != null) position.x = positionX.Value;
    //    if (positionZ != null) position.z = positionZ.Value;

    //    Quaternion rotation = Quaternion.identity;
    //    if (rotationY != null) rotation = Quaternion.Euler(0, rotationY.Value, 0);
    //    else rotation = pierConstruction.BoatDockPositions[dockIndex.Value].rotation;

    //    if (spawnedBoats[dockIndex.Value])
    //        spawnedBoats[dockIndex.Value].Demolish(false);

    //    Boat spawnedBoat = Boat.Instantiate(boat, position, rotation);
    //    spawnedBoat.Init(isUnderConstruction, dockIndex.Value, isFloating, isReturningToDock, health);
    //    spawnedBoats[dockIndex.Value] = spawnedBoat;
    //}

    //private void OnBuildingStartConstructing(ConstructionComponent construction)
    //{
    //    int levelIndex = construction.ownedBuilding.LevelIndex;

    //    Building building = construction.GetComponent<Building>();
    //    if (building) {
    //        OnBuildingFinishConstructing(construction);
    //        //building.FinishConstructing();
    //    }
    //}

    private void OnConstructionBuilt(Building building)
    {
        FloorFrameModule floorBuilding = building.GetComponent<FloorFrameModule>();
        ElevatorBuildingModule elevatorBuilding = building.GetComponent<ElevatorBuildingModule>();

        if (elevatorBuilding) {
            if (elevatorGroups.Count <= elevatorBuilding.elevatorGroupId) {
                List<ElevatorBuildingModule> elevatorGroup = new List<ElevatorBuildingModule>();
                elevatorGroups.Add(elevatorGroup);
            }
            elevatorGroups[elevatorBuilding.elevatorGroupId].Add(elevatorBuilding);
        }

        if (building.BuildingData.BuildingType != BuildingType.Environment && building.GetComponent<StorageBuildingModule>()) {
            StorageBuildingModule storage = building.GetComponent<StorageBuildingModule>();

            int level = building.LevelIndex;
            if (level > 1) {
                StorageModuleLevelData previousLevelData = storage.LevelsData[level - 1] as StorageModuleLevelData;
                SubtractStorageCapacity(previousLevelData);
            }

            StorageModuleLevelData currentLevelData = storage.LevelsData[level] as StorageModuleLevelData;
            if (currentLevelData)
                AddStorageCapacity(currentLevelData);
            else
                Debug.LogError(building.BuildingData.BuildingName + $" has no StorageBuildingLevelData by level index {level}");

            EventBus.InvokeStorageCapacityChanged();
        }
    }

    //public void TryToUpgradeConstruction(Building building)
    //{
    //    int nextLevelIndex = building.LevelIndex + (building.IsRuined ? 0 : 1);

    //    if (building.ConstructionLevelsData.Count() > nextLevelIndex) {
    //        bool isResourcesToUpgradeEnough = true;

    //        int index = 0;
    //        int amount = 0;
    //        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

    //        for (int i = 0; i < resourcesToUpgrade.Length; i++) {
    //            index = resourcesToUpgrade[i].ItemData.ItemId;
    //            amount = resourcesToUpgrade[i].Amount;

    //            if (items[index].Amount < amount) {
    //                isResourcesToUpgradeEnough = false;
    //                break;
    //            }
    //        }

    //        if (isResourcesToUpgradeEnough) {
    //            for (int i = 0; i < resourcesToUpgrade.Length; i++) {
    //                //itemIndex = GameManager.GetItemIndexById(GameManager.itemsData, resourcesToUpgrade[i].ItemData.ItemId);
    //                amount = resourcesToUpgrade[i].Amount;
    //                SpendItem(resourcesToUpgrade[i].ItemData.ItemId, amount);
    //            }

    //            building.StartUpgrading();
    //        }
    //    }
    //}

    private void OnBuildingDemolished(Building building)
    {
        // Return the part of resources
        ItemInstance[] resourceToBuilds = building.ConstructionLevelsData[building.LevelIndex].ResourcesToBuild;
        for (int i = 0; i < resourceToBuilds.Length; i++) {
            int id = resourceToBuilds[i].ItemData.ItemId;
            int amount = (int)math.ceil(resourceToBuilds[i].Amount * demolitionResourceRefundRate);
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

    // Production Module
    private void OnProductionModuleClicked(ProductionBuildingModule module)
    {
        int itemId = module.produceItem.produceItem.ItemData.ItemId;
        int maxAmount = maxItemsAmount[itemId];
        int remainedAmount = maxItemsAmount[itemId] - items[itemId].Amount;

        ItemInstance itemToTake = module.TakeProducedItem(remainedAmount);
        Debug.Log(itemToTake.Amount);
        AddItem(itemToTake);
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
    public void AddStorageCapacity(StorageModuleLevelData storageLevelData)
    {
        ChangeStorageCapacity(storageLevelData, true);
    }

    public void SubtractStorageCapacity(StorageModuleLevelData storageLevelData)
    {
        ChangeStorageCapacity(storageLevelData, false);
    }

    private void ChangeStorageCapacity(StorageModuleLevelData storageLevelData, bool isIncreasing)
    {
        for (int i = 0; i < storageLevelData.storageItems.Length; i++) {
            int id = storageLevelData.storageItems[i].ItemData.ItemId;
            int changeValue = storageLevelData.storageItems[i].Amount;

            if (isIncreasing)
                maxItemsAmount[id] += changeValue;
            else
                maxItemsAmount[id] -= changeValue;
        }

        for (int i = 0; i < storageLevelData.storageItemCategories.Length; i++) {
            for (int j = 0; j < ItemsList.Instance.Items.Length; j++) {
                if (items[j].ItemData.ItemCategory == storageLevelData.storageItemCategories[i].itemCategory) {
                    int changeValue = storageLevelData.storageItemCategories[i].amount;

                    if (isIncreasing)
                        maxItemsAmount[j] += changeValue;
                    else
                        maxItemsAmount[j] -= changeValue;
                }
            }
        }
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
        ItemInstance item = items[itemId];
        int maxAmount = maxItemsAmount[itemId];
        item.AddAmount(amount, maxAmount);
        EventBus.InvokeItemAdded(item);
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

    private IEnumerator AutosaveCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(autoSaveFrequency);
            SaveSystem.SaveData(playerController);
        }
    }
}
