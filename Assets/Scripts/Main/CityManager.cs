using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[Serializable]
public class CityData
{
    public string cityName = "";
    public int floorsCount = 0;
}

public class CityManager : MonoBehaviour
{
    public static CityManager Instance { get; private set; } = null;

    [SerializeField] private PlayerController playerController = null;

    private Inventory inventory = null;
    public Inventory Inventory => inventory;

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
    public const int firstFloorHeight = 10;

    public const int roomsCountPerFloor = 8;
    public const int roomsCountPerSide = 3;
    public const int roomsWidth = 8;
    public const int floorWidth = 24;
    //public const int firstBuildCityFloorIndex = 1;
    public const int firstBuildCityBuildingPlace = 1;
    public float currentCityHeight { get; private set; } = 0;

    public List<List<ElevatorModule>> elevatorGroups { get; private set; } = new List<List<ElevatorModule>>();

    public Building buildingToPlace { get; private set; }

    [Header("NPC")]
    public List<Human> citizens { get; private set; } = new List<Human>();
    private const int startResidentsCount = 2;

    [Header("Boats")]
    [SerializeField] private BoatIdEnum[] startBoatIds;
    public List<Boat> citizenBoats { get; private set; } = new List<Boat>();

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    public bool isNavMeshBuilt { get; private set; } = false;

    [Header("NPC")]
    [field: SerializeField] public Transform entitySpawnPosition { get; private set; } = null;
    public const float maxSpawnRange = 5f;

    // Other
    public const float autoSaveFrequency = 1;
    public const float triggerLootContainerRadius = 150f;
    public const float demolitionResourceRefundRate = 0.2f;

    IEnumerable<GroundBuilding> EnvironmentBuildings()
    {
        yield return towerGate;
        yield return pierBuilding;
    }

    private void Awake()
    {
        inventory = GetComponent<Inventory>();

        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerController.Init();
    }

    private void OnEnable()
    {
        // Buildings
        EventBus.onBuildingPlacePressed += OnBuildingPlacePressed;
        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
        EventBus.onBuildingInitialized += OnBuildingInitialized;
        EventBus.onBuildingPlaced += OnBuildingPlaced;

        // Modules
        EventBus.onBuildingModuleInited += OnBuildingModuleInited;
        EventBus.onBuildingModuleUpgraded += OnBuildingModuleUpgraded;
        EventBus.onBuildingModuleDemolished += OnBuildingModuleDemolished;

        // Production Module
        EventBus.onProductionModuleClicked += OnProductionModuleClicked;

        // Boats
        Boat.OnBoadDestroyed += OnBoatDestroyed;

        // Inventory
        inventory.onChangedItemAmount += OnItemAmountChanged;
    }

    private void OnDisable()
    {
        // Buildings
        EventBus.onBuildingPlacePressed -= OnBuildingPlacePressed;
        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
        EventBus.onBuildingInitialized -= OnBuildingInitialized;
        EventBus.onBuildingPlaced -= OnBuildingPlaced;

        // Modules
        EventBus.onBuildingModuleInited -= OnBuildingModuleInited;
        EventBus.onBuildingModuleUpgraded -= OnBuildingModuleUpgraded;
        EventBus.onBuildingModuleDemolished -= OnBuildingModuleDemolished;

        // Production Module
        EventBus.onProductionModuleClicked -= OnProductionModuleClicked;

        // Boats
        Boat.OnBoadDestroyed -= OnBoatDestroyed;

        // Inventory
        inventory.onChangedItemAmount -= OnItemAmountChanged;
    }

    private void Start()
    {
        TimerManager.Initialize();

        WorldData saveData = WorldSaveManager.Instance.worldData;

        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
        LoadItems(saveData);
        LoadBuildings(saveData);
        LoadBoats(saveData);
        LoadCreatures(saveData);

        playerController.Load(saveData?.playerData);
    }

    private void Update()
    {
        TimerManager.Tick();
    }

    private void LoadBuildings(WorldData data)
    {
        LoadEnvironmentBuildings(data);
        LoadFloorFrames();
        LoadTowerBuildings(data);
    }

    private void LoadEnvironmentBuildings(WorldData data)
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

    private void LoadTowerBuildings(WorldData saveData)
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

    private void LoadCreatures(WorldData saveData)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        if (saveData != null) {
            foreach (var data in saveData.citizensData) {
                int entityId = data.id;
                Human citizen = CreatureFactory.CreateCreature(entityId, data) as Human;
                AddResident(citizen);
                citizen.SetNavAgentEnabled(false);
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

                HumanEntry data = new HumanEntry { position = finalPosition, rotation = rotation.eulerAngles };
                Human citizen = CreatureFactory.CreateCreature(0, data) as Human;
                AddResident(citizen);
                citizen.SetNavAgentEnabled(false);
            }
        }
    }

    private void LoadBoats(WorldData saveData)
    {
        PierModule pier = PierBuilding.GetComponent<PierModule>();

        if (saveData != null) {

        }
        else {
            for (int i = 0; i < startBoatIds.Length; i++) {
                Boat prefab = BoatsList.Instance.boats[(int)startBoatIds[i]];
                BoatDockPoint spawnTransform = pier.PierConstruction.BoatDocks[i];
                Vector3 spawnPosition = spawnTransform.DockTransform.position;
                Vector3 spawnRotation = spawnTransform.DockTransform.rotation.eulerAngles;
                BoatEntry data = new BoatEntry { position = spawnPosition, rotation = spawnRotation, health = prefab.BoatData.MaxHealth };
                Boat boat = BoatFactory.CreateBoat((int)startBoatIds[i], data);
                citizenBoats.Add(boat);
            }
        }
    }

    private void LoadItems(WorldData saveData)
    {
        if (saveData != null) {

        }
        else {
            foreach (ItemData data in ItemsList.Instance.Items) {
                int id = data.ItemId;
                inventory.AddItem(id);
            }
        }
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        if (bakeNavMeshCoroutine != null) yield break;

        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();
        bakeNavMeshCoroutine = null;
        EventBus.InvokNavMeshBaked();
    }

    private void AddResident(Human resident)
    {
        citizens.Add(resident);
        EventBus.InvokeCitizenAdded(resident);
    }

    private void RemoveResident(Human resident)
    {
        Destroy(resident);
        EventBus.InvokeResidentRemoved(resident);
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
        currentCityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + floorHeight;
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
        EventBus.InvokeOnBuildingStartPlacing(widget.buildingPrefab);
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

    private void OnBuildingPlaced(Building building)
    {
        FloorFrameModule floorBuilding = building.GetComponent<FloorFrameModule>();
        ElevatorModule elevatorBuilding = building.GetComponent<ElevatorModule>();

        if (elevatorBuilding) {
            if (elevatorGroups.Count <= elevatorBuilding.elevatorGroupId) {
                List<ElevatorModule> elevatorGroup = new List<ElevatorModule>();
                elevatorGroups.Add(elevatorGroup);
            }
            elevatorGroups[elevatorBuilding.elevatorGroupId].Add(elevatorBuilding);
        }
    }

    // Modules
    private void OnBuildingModuleInited(BuildingModule module)
    {
        StorageBuildingModule storage = module as StorageBuildingModule;
        if (storage) {
            OnStorageModuleInited(storage);
        }
    }

    private void OnBuildingModuleUpgraded(BuildingModule module)
    {
        StorageBuildingModule storage = module as StorageBuildingModule;
        if (storage) {
            OnStorageModuleUpgraded(storage);
        }
    }

    private void OnBuildingModuleDemolished(BuildingModule module)
    {
        StorageBuildingModule storage = module as StorageBuildingModule;
        if (storage) {
            OnStorageModuleDemolished(storage);
        }
    }

    private void OnStorageModuleInited(StorageBuildingModule module)
    {
        StorageModuleLevelData levelData = module.LevelData as StorageModuleLevelData;
        foreach (ItemInstance item in levelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;

            inventory.AddItemMaxAmount(id, amount);
        }

        EventBus.InvokeStorageCapacityChanged();
    }

    private void OnStorageModuleUpgraded(StorageBuildingModule module)
    {
        StorageModuleLevelData currentLevelData = module.LevelData as StorageModuleLevelData;
        StorageModuleLevelData lastLevelData = module.LevelData as StorageModuleLevelData;
        foreach (ItemInstance item in currentLevelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount - currentLevelData.storageItems[id].Amount;

            inventory.AddItemMaxAmount(id, amount);
        }

        EventBus.InvokeStorageCapacityChanged();
    }

    private void OnStorageModuleDemolished(StorageBuildingModule module)
    {
        StorageModuleLevelData levelData = module.LevelData as StorageModuleLevelData;
        foreach (ItemInstance item in levelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;

            inventory.RemoveItemMaxAmount(id, amount);
        }

        EventBus.InvokeStorageCapacityChanged();
    }

    private void OnBuildingDemolished(Building building)
    {
        // Return the part of resources
        ItemInstance[] resourceToBuilds = building.ConstructionLevelsData[building.LevelIndex].ResourcesToBuild;
        for (int i = 0; i < resourceToBuilds.Length; i++) {
            int id = resourceToBuilds[i].ItemData.ItemId;
            int amount = (int)math.ceil(resourceToBuilds[i].Amount * demolitionResourceRefundRate);
            inventory.AddItemAmount(id, amount);
        }
    }

    // Inventory
    private void OnItemAmountChanged(ItemInstance item)
    {
        EventBus.InvokeMainStorageAmountChanged(item);
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
        int maxAmount = inventory.items[itemId].maxAmount;
        int remainedAmount = maxAmount - inventory.items[itemId].item.Amount;

        ItemInstance item = module.TakeProducedItem(remainedAmount);
        Inventory.AddItemAmount(item.ItemData.ItemId, item.Amount);
    }

    // Boats
    private void OnBoatDestroyed(Boat boat)
    {

    }

    public Boat GetBoatByIndex(int index)
    {
        for (int i = 0; i < citizenBoats.Count; i++) {
            if (citizenBoats[i])
                return citizenBoats[i];
        }

        return null;
    }

    private IEnumerator AutosaveCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(autoSaveFrequency);
            WorldSaveSystem.SaveData(playerController);
        }
    }
}
