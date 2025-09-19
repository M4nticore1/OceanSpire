using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;
using static BuildingData;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public float worldTime = 0.0f;

    // Buildings
    //[HideInInspector] public int buildedFloorsCount = 0;

    [Header("Buildings")]
    public List<Building> buildingPrefabs = new List<Building>();
    //public List<Floor> spawnedFloors = new List<Floor>();

    //private List<List<BuildingPlace>> spawnedRoomPlaces = new List<List<BuildingPlace>>();
    //[SerializeField] private List<BuildingPlace> spawnedHallPlaces = new List<BuildingPlace>();
    //private List<List<BuildingPlace>> spawnedElevatorPlaces = new List<List<BuildingPlace>>();
    //[SerializeField] private List<BuildingPlace> spawnedFloorPlaces = new List<BuildingPlace>();

    //[HideInInspector] public List<Building> allBuildings = new List<Building>();
    //[HideInInspector] public List<List<RoomBuilding>> allRooms = new List<List<RoomBuilding>>();
    //[HideInInspector] public List<List<ElevatorBuilding>> allElevators = new List<List<ElevatorBuilding>>();
    //[HideInInspector] public List<StorageBuildingComponent> spawnedStorageBuildings = new List<StorageBuildingComponent>();

    //[HideInInspector] public List<int> currentRoomsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentHallsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentElevatorsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentFloorFrameNumberOnFloor = new List<int>();

    //private const float firstFloorHeight = 5.0f;
    //[HideInInspector] public const int roomsCountPerFloor = 8;
    //[HideInInspector] public const int elevatorsCountPerFloor = 12;
    //[HideInInspector] public float cityHeight = 0;

    public const float demolitionResourceRefundRate = 0.2f;

    // Items
    [Header("Items")]
    public List<ItemData> itemsData = new List<ItemData>();
    //[HideInInspector] public List<ItemInstance> items = new List<ItemInstance>();

    // Loot Containers
    private const float lootContainerSpawnFrequency = 0.1f;
    private float currentTimeToSpawnLootContainer = 0.0f;

    [Header("Loot Containers")]
    [SerializeField] private List<LootContainer> lootContainerPrefabs = new List<LootContainer>();
    private List<float> spawnedLootContainersTime = new List<float>();

    public const float lootContainersSpawnDistance = 250.0f;
    private const float lootContainersSpawnMaxOffsetYaw = 60.0f;

    private float checkLootContainersPositionTime = 0.0f;
    private const float checkLootContainersPositionRate = 2.0f;

    [Header("NPC")]
    public Resident residentPrefab = null;

    // Wind
    [HideInInspector] public Vector2 windDirection = Vector2.zero;
    private Vector2 newWindDirection = Vector2.zero;

    [HideInInspector] public float windSpeed = 0.0f;
    private float newWindSpeed = 0.0f;
    private const float windMinSpeed = 4.0f;
    private const float windMaxSppeed = 10.0f;

    private float windSpeedChangingSpeed = 0.0f;
    private const float windMinSpeedChangingSpeed = 0.05f;
    private const float windMaxSpeedChangingSpeed = 0.1f;

    private float windDirectionChangeRate = 0.0f;
    private const float windDirectionChanceMinRate = 120.0f;
    private const float windDirectionChanceMaxRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    private float windDirectionChangeSpeed = 0.0f;
    private const float windDirectionMinChangeSpeed = 0.04f;
    private const float windDirectionMaxChangeSpeed = 0.05f;

    // Delegates
    //public delegate void StorageCapacityChangedHandler();
    //public static event System.Action OnStorageCapacityChanged;

    private void Awake()
    {
        SetLootContainersSpawnTime();

        windSpeed = windMinSpeed;
        ChangeWind();

        //InitializeItems();

        //for (int i = 0; i < spawnedFloors.Count; i++)
        //{
        //    spawnedFloors[i].InitializeBuildingPlaces();

        //    buildedFloorsCount++;
        //    currentRoomsNumberOnFloor.Add(0);
        //    currentHallsNumberOnFloor.Add(0);
        //    currentElevatorsNumberOnFloor.Add(0);
        //    currentFloorFrameNumberOnFloor.Add(0);

        //    spawnedRoomPlaces.Add(spawnedFloors[i].roomsBuildingPlaces);
        //    spawnedHallPlaces.Add(spawnedFloors[i].hallBuildingPlace);
        //    spawnedElevatorPlaces.Add(spawnedFloors[i].elevatorsBuildingPlaces);

        //    List<RoomBuilding> rooms = new List <RoomBuilding>();
        //    for (int j = 0; j < roomsCountPerFloor; j++)
        //        rooms.Add(null);
        //    allRooms.Add(rooms);

        //    cityHeight = spawnedFloors[buildedFloorsCount - 1].transform.position.y + floorHeight;
        //}

        //HideAllBuildigPlaces();

        for (int i = 0; i < lootContainerPrefabs.Count; i++)
            spawnedLootContainersTime.Add(0.0f);
    }

    private void Start()
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

    private void Update()
    {
        worldTime += Time.deltaTime;

        SpawningLootContainers();
        ChangingWind();
    }

    //// Building Places
    //public void AddFloor(Floor newFloor)
    //{
    //    buildedFloorsCount++;
    //    spawnedFloors.Add(newFloor);

    //    currentRoomsNumberOnFloor.Add(0);
    //    currentHallsNumberOnFloor.Add(0);
    //    currentElevatorsNumberOnFloor.Add(0);
    //    currentFloorFrameNumberOnFloor.Add(0);

    //    spawnedRoomPlaces.Add(newFloor.roomsBuildingPlaces);
    //    spawnedHallPlaces.Add(newFloor.hallBuildingPlace);
    //    spawnedElevatorPlaces.Add(newFloor.elevatorsBuildingPlaces);
    //    spawnedFloorPlaces.Add(newFloor.floorBuildingPlace);

    //    List<RoomBuilding> rooms = new List<RoomBuilding>();
    //    for (int j = 0; j < roomsCountPerFloor; j++)
    //        rooms.Add(null);
    //    allRooms.Add(rooms);

    //    cityHeight = spawnedFloors[buildedFloorsCount - 1 - (spawnedFloorPlaces.Count() - 1)].transform.position.y + floorHeight;
    //}

    //public void ShowBuildingPlacesByType(BuildingType buildingType)
    //{
    //    HideAllBuildigPlaces();

    //    for (int i = 0; i < spawnedFloors.Count; i++)
    //    {
    //        spawnedFloors[i].ShowBuildingPlacesByType(buildingType);
    //    }
    //}

    //public void HideBuildingPlacesByType(BuildingType buildingType)
    //{
    //    if (buildingType == BuildingType.Room)
    //    {
    //        for (int i = 0; i < buildedFloorsCount; i++)
    //        {
    //            for (int j = 0; j < spawnedFloors[i].roomsBuildingPlaces.Count; j++)
    //            {
    //                if (spawnedRoomPlaces[i][j] != null)
    //                {
    //                    spawnedRoomPlaces[i][j].HideBuildingPlace();
    //                }
    //            }
    //        }
    //    }
    //    else if (buildingType == BuildingType.Hall)
    //    {
    //        for (int i = 0; i < buildedFloorsCount; i++)
    //        {
    //            if (spawnedHallPlaces[i] != null)
    //            {
    //                spawnedHallPlaces[i].HideBuildingPlace();
    //            }
    //        }
    //    }
    //    else if (buildingType == BuildingType.Elevator)
    //    {
    //        for (int i = 0; i < buildedFloorsCount; i++)
    //        {
    //            for (int j = 0; j < spawnedFloors[i].elevatorsBuildingPlaces.Count; j++)
    //            {
    //                if (spawnedFloors[i].elevatorsBuildingPlaces[j])
    //                {
    //                    spawnedFloors[i].elevatorsBuildingPlaces[j].HideBuildingPlace();
    //                }
    //            }
    //        }
    //    }
    //}

    //private void HideAllBuildigPlaces()
    //{
    //    for (int i = 0; i < spawnedFloors.Count; i++)
    //    {
    //        spawnedFloors[i].HideAllBuildingPlaces();
    //    }
    //}

    //// BUildings
    //public void PlaceBuilding(Building buildingToPlace, BuildingPlace buildingPlace)
    //{
    //    if (buildingToPlace.buildingData.buildingType == BuildingType.Room)
    //    {
    //        int floorIndex = buildingPlace.floorIndex;
    //        int buildingIndex = buildingPlace.buildingPlaceIndex;

    //        allRooms[floorIndex][buildingIndex] = buildingToPlace as RoomBuilding;
    //        currentRoomsNumberOnFloor[buildingPlace.floorIndex]++;

    //        buildingPlace.PlaceBuilding(buildingToPlace);
    //    }
    //    else if (buildingToPlace.buildingData.buildingType == BuildingType.Hall)
    //    {
    //        buildingPlace.PlaceBuilding(buildingToPlace);

    //        for (int i = 0; i < roomsCountPerFloor; i++)
    //        {
    //            spawnedRoomPlaces[buildingPlace.floorIndex][i].AddPlacedBuilding(buildingToPlace);
    //        }

    //        currentHallsNumberOnFloor[buildingPlace.floorIndex]++;
    //    }
    //    else if (buildingToPlace.buildingData.buildingType == BuildingType.Elevator)
    //    {
    //        buildingPlace.PlaceBuilding(buildingToPlace);

    //        currentElevatorsNumberOnFloor[buildingPlace.floorIndex]++;
    //    }
    //    else if (buildingToPlace.buildingData.buildingType == BuildingType.FloorFrame)
    //    {
    //        buildingPlace.PlaceBuilding(buildingToPlace);

    //        currentFloorFrameNumberOnFloor[buildingPlace.floorIndex]++;
    //    }

    //    StorageBuildingComponent storageBuilding = buildingToPlace.GetComponent<StorageBuildingComponent>();

    //    if (storageBuilding)
    //    {
    //        spawnedStorageBuildings.Add(storageBuilding);
    //    }

    //    // Spend Build Resources
    //    //List<ResourceToBuild> resourcesToBuild = buildingToPlace.buildingLevelsData[0].ResourcesToBuild;

    //    //for (int i = 0; i < resourcesToBuild.Count(); i++)
    //    //{
    //    //    string itemIdName = resourcesToBuild[i].resourceData.itemIdName;
    //    //    int itemAmount = resourcesToBuild[i].amount;

    //    //    SpendItemByIdName(itemIdName, itemAmount);
    //    //}

    //    HideAllBuildigPlaces();
    //}

    //public void TryToUpgradeBuilding(Building building)
    //{
    //    int levelIndex = building.levelIndex + (building.isRuined ? 0 : 1);

    //    if (building.buildingLevelsData.Count() > levelIndex)
    //    {
    //        bool isResourcesToUpgradeEnough = true;

    //        int itemIndex = 0;
    //        int itemAmount = 0;
    //        List<ResourceToBuild> resourcesToUpgrade = building.buildingLevelsData[levelIndex].ResourcesToBuild;

    //        for (int i = 0; i < resourcesToUpgrade.Count; i++)
    //        {
    //            itemIndex = GetItemIndexByIdName(resourcesToUpgrade[i].resourceData.itemIdName);
    //            itemAmount = resourcesToUpgrade[i].amount;

    //            if (items[itemIndex].amount < itemAmount)
    //            {
    //                isResourcesToUpgradeEnough = false;
    //                break;
    //            }
    //        }

    //        if (isResourcesToUpgradeEnough)
    //        {
    //            for (int i = 0; i < resourcesToUpgrade.Count; i++)
    //            {
    //                itemIndex = GetItemIndexByIdName(resourcesToUpgrade[i].resourceData.itemIdName);
    //                itemAmount = resourcesToUpgrade[i].amount;
    //                SpendItemByIndex(itemIndex, itemAmount);
    //            }

    //            building.Upgrade();
    //        }
    //    }
    //}

    //public void DemolishBuilding(Building building)
    //{
    //    building.Demolish();
    //}

    //// Resources
    //public void AddStorageCapacity(StorageBuildingLevelData storageLevelData)
    //{
    //    ChangeStorageCapacity(storageLevelData, true);
    //}

    //public void SubtractStorageCapacity(StorageBuildingLevelData storageLevelData)
    //{
    //    ChangeStorageCapacity(storageLevelData, false);
    //}

    //private void ChangeStorageCapacity(StorageBuildingLevelData storageLevelData, bool isIncreasing)
    //{
    //    for (int i = 0; i < storageLevelData.storageItems.Count(); i++)
    //    {
    //        int index = GetItemIndexByIdName(storageLevelData.storageItems[i].itemdata.itemIdName);
    //        int changeValue = storageLevelData.storageItems[i].capacity;

    //        if (isIncreasing)
    //            items[index].AddMaxAmount(changeValue);
    //        else
    //            items[index].SubtractMaxAmount(changeValue);
    //    }

    //    for (int i = 0; i < storageLevelData.storageItemCategories.Count(); i++)
    //    {
    //        for (int j = 0; j < items.Count(); j++)
    //        {
    //            if (items[j].itemData.itemCategory == storageLevelData.storageItemCategories[i].itemCategory)
    //            {
    //                int changeValue = storageLevelData.storageItemCategories[i].capacity;

    //                if (isIncreasing)
    //                    items[j].AddMaxAmount(changeValue);
    //                else
    //                    items[j].SubtractMaxAmount(changeValue);
    //            }
    //        }
    //    }

    //    OnStorageCapacityChanged?.Invoke();
    //}

    //private void InitializeItems()
    //{
    //    //itemsMaxAmount.Add(ItemType.Population, 8);
    //    //itemsMaxAmount.Add(ItemType.Food, 100);
    //    //itemsMaxAmount.Add(ItemType.Electricity, 100);
    //    //itemsMaxAmount.Add(ItemType.Building, 1000);
    //    //itemsMaxAmount.Add(ItemType.Crafting, 10);
    //    //itemsMaxAmount.Add(ItemType.Weapon, 0);

    //    for (int i = 0; i < itemsData.Count; i++)
    //    {
    //        items.Add(new ItemInstance(itemsData[i], 0, 0));
    //    }
    //}

    //public void AddItemByIndex(int index, int amount)
    //{
    //    items[index].AddAmount(amount);
    //}

    //public void SpendItemByIndex(int index, int amount)
    //{
    //    items[index].SubtractAmount(amount);
    //}

    //public void SpendItems(List<ItemInstance> itemsToSpend)
    //{
    //    for (int i = 0; i < itemsToSpend.Count; i++)
    //    {
    //        int index = GetItemIndexByIdName(itemsToSpend[i].itemData.itemIdName);
    //        int amountToSpend = itemsToSpend[i].amount;

    //        SpendItemByIndex(index, amountToSpend);
    //    }
    //}

    //public int GetItemIndexByIdName(string idName)
    //{
    //    int id = 0;

    //    for (int i = 0; i < items.Count(); i++)
    //    {
    //        if (items[i].itemData.itemIdName == idName)
    //        {
    //            id = i;
    //            break;
    //        }
    //    }

    //    return id;
    //}

    //public int GetItemIndexById(int id)
    //{
    //    int currentId = 0;

    //    for (int i = 0; i < items.Count(); i++)
    //    {
    //        if (items[i].itemData.itemId == id)
    //        {
    //            currentId = i;
    //            break;
    //        }
    //    }

    //    return currentId;
    //}

    private void SpawningLootContainers()
    {
        currentTimeToSpawnLootContainer += Time.deltaTime;

        if (currentTimeToSpawnLootContainer >= lootContainerSpawnFrequency)
        {
            for (int i = 0; i < lootContainerPrefabs.Count(); i++)
            {
                spawnedLootContainersTime[i] += currentTimeToSpawnLootContainer;

                if (spawnedLootContainersTime[i] >= lootContainerPrefabs[i].spawnTime)
                {
                    float spawnPositionOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnMaxOffsetYaw / 2, lootContainersSpawnMaxOffsetYaw / 2);
                    Quaternion rotation = Quaternion.Euler(0, spawnPositionOffsetYaw, 0);
                    Vector3 direction = rotation * new Vector3(windDirection.x, 0, windDirection.y);
                    Vector2 newWindDirection = new Vector2(direction.x, direction.z).normalized;

                    //float angle = Mathf.Atan2(windDirection.y, windDirection.x) * Mathf.Rad2Deg;
                    float angle = UnityEngine.Random.Range(0, 360);
                    Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

                    Vector2 rangePosition = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
                    Vector3 spawnPosition = new Vector3(-newWindDirection.x, 0, -newWindDirection.y) * lootContainersSpawnDistance + new Vector3(rangePosition.x, 0, rangePosition.y);

                    Instantiate(lootContainerPrefabs[i], spawnPosition, spawnRotation);

                    lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
                    spawnedLootContainersTime[i] = 0;
                }
            }

            currentTimeToSpawnLootContainer = 0;
        }
    }

    private void SetLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count(); i++)
        {
            lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
        }
    }

    private void ChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeRate)
        {
            ChangeWind();
        }

        windDirection = math.lerp(windDirection, newWindDirection, windDirectionChangeSpeed * Time.deltaTime);
        windSpeed = math.lerp(windSpeed, newWindSpeed, windSpeedChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float xAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        float yAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        newWindDirection = new Vector2(xAxis, yAxis).normalized;

        windDirectionChangeRate = UnityEngine.Random.Range(windDirectionChanceMinRate, windDirectionChanceMaxRate);
        windDirectionChangeSpeed = UnityEngine.Random.Range(windDirectionMinChangeSpeed, windDirectionMaxChangeSpeed);
        newWindSpeed = UnityEngine.Random.Range(windMinSpeed, windMaxSppeed);
        windSpeedChangingSpeed = UnityEngine.Random.Range(windMinSpeedChangingSpeed, windMaxSpeedChangingSpeed);

        windDirectionChangeTime = 0;
    }

    private void CheckLootContainersPosition()
    {
        checkLootContainersPositionTime += Time.deltaTime;
    }

    public int GetItemIndexByIdName(string idName)
    {
        int id = 0;

        for (int i = 0; i < itemsData.Count(); i++)
        {
            if (itemsData[i].itemIdName == idName)
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public int GetItemIndexById(int id)
    {
        int currentId = 0;

        for (int i = 0; i < itemsData.Count(); i++)
        {
            if (itemsData[i].itemId == id)
            {
                currentId = i;
                break;
            }
        }

        return currentId;
    }
}
