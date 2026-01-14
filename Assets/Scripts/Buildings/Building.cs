using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[AddComponentMenu("Buildings/Building")]
public class Building : MonoBehaviour
{
    protected GameManager gameManager { get; private set; } = null;
    public CityManager cityManager { get; private set; } = null;
    //public LevelComponent levelComponent { get; private set; } = null;
    public ConstructionComponent constructionComponent { get; private set; } = null;
    public SelectComponent selectComponent { get; private set; } = null;
    public StorageBuildingComponent storageComponent { get; private set; } = null;
    public ProductionBuilding productionComponent { get; private set; } = null;


    public bool isInitialized { get; private set; } = false;
    public int levelIndex = 0;
    private bool isWorking = false;
    protected bool isSelected { get; private set; } = false;

    //[HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<Entity> enteredEntities { get; private set; } = new List<Entity>();
    public List<Entity> workers { get; private set; } = new List<Entity>();
    public List<Entity> currentWorkers { get; private set; } = new List<Entity>();

    protected BuildingPosition buildingPosition = BuildingPosition.Straight;
    public int floorIndex => buildingPlace ? buildingPlace.floorIndex : 0;
    public int placeIndex => buildingPlace ? buildingPlace.BuildingPlaceIndex : 0;

    public Building leftConnectedBuilding { get; private set; } = null;
    public Building rightConnectedBuilding { get; private set; } = null;
    public Building upConnectedBuilding { get; private set; } = null;
    public Building downConnectedBuilding { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected List<ConstructionLevelData> buildingLevelsData = new List<ConstructionLevelData>();
    public List<ConstructionLevelData> ConstructionLevelsData => buildingLevelsData;
    public ConstructionLevelData currentLevelData => ConstructionLevelsData.Count > levelIndex ? ConstructionLevelsData[levelIndex] : null;

    public BuildingPlace buildingPlace { get; private set; } = null;

    //public static event System.Action<Building> onAnyBuildingFinishConstructing;
    public event System.Action onBuildingFinishConstructing;
    public event System.Action onBuildingStartWorking;
    public event System.Action onBuildingStopWorking;
    public event System.Action onEnterBuilding;
    public event System.Action onExitBuilding;
    public event System.Action onResidentStartWorking;
    public event System.Action onResidentStopWorking;

    protected virtual void Awake()
    {
        GetComponents();
    }

    protected virtual void OnEnable()
    {
        //constructionComponent.onBuildingStartConstructing += StartConstructing;
        //constructionComponent.onBuildingFinishConstructing += FinishConstructing;
        //constructionComponent.onConstructionDemolished += Demolish;
    }

    protected virtual void OnDisable()
    {
        //constructionComponent.onBuildingStartConstructing -= StartConstructing;
        //constructionComponent.onBuildingFinishConstructing -= FinishConstructing;
        //constructionComponent.onConstructionDemolished -= Demolish;
    }

    protected virtual void Start()
    {
        //Place();
    }

    protected void Update()
    {

    }

    private void GetComponents()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        selectComponent = GetComponent<SelectComponent>();
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuilding>();
    }

    // Constructing
    public virtual void InitializeBuilding(BuildingPlace buildingPlace, bool isUnderConstruction, int levelIndex, int interiorIndex = -1)
    {
        if (isInitialized) return;

        GetComponents();

        this.buildingPlace = buildingPlace;
        this.levelIndex = levelIndex;

        if (BuildingData.BuildingType == BuildingType.Room || BuildingData.BuildingType == BuildingType.Hall)
        {
            if (storageComponent)
                storageComponent.Initialize();

            leftConnectedBuilding = GetConnectedBuilding(Side.Left);
            rightConnectedBuilding = GetConnectedBuilding(Side.Right);
            upConnectedBuilding = GetConnectedBuilding(Side.Up);
            downConnectedBuilding = GetConnectedBuilding(Side.Down);

            if (placeIndex % 2 == 0)
                buildingPosition = BuildingPosition.Corner;
            else
                buildingPosition = BuildingPosition.Straight;
        }

        constructionComponent.InitializeConstruction(isUnderConstruction, levelIndex);

        isInitialized = true;
    }

    protected virtual void Place(/*BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex*/)
    {

    }

    protected IEnumerator PlaceCoroutine(bool isUnderConstruction, int levelIndex)
    {
        yield return new WaitForEndOfFrame();
        constructionComponent.InitializeConstruction(isUnderConstruction, levelIndex);
    }

    protected void StartConstructing()
    {
        BuildConstruction(levelIndex);
    }

    public virtual void FinishConstructing()
    {
        if (BuildingData.BuildingIdName == "floor_frame") return;

        BuildConstruction(levelIndex);
        onBuildingFinishConstructing?.Invoke();
    }

    protected void Demolish()
    {

    }

    // Working
    private void StartWorking()
    {
        if (isWorking) return;
        isWorking = true;
        onBuildingStartWorking?.Invoke();
    }

    private void StopWorking()
    {
        if (!isWorking) return;
        isWorking = false;
        onBuildingStopWorking?.Invoke();
    }

    // Residents Management
    public virtual void EnterBuilding(Entity entity)
    {
        enteredEntities.Add(entity);
        Resident resident = entity as Resident;

        if (resident) {
            if (resident.isWorking) { // If constructing building

            }
            else if (resident.workBuilding == this) { // If resident is worker
                StartWorking();
            }
        }

        onEnterBuilding?.Invoke();
    }

    public virtual void ExitBuilding(Entity entity)
    {
        enteredEntities.Remove(entity);
        onExitBuilding?.Invoke();
    }

    public void AddWorker(Entity worker)
    {
        workers.Add(worker);
    }

    public void RemoveWorker(Entity worker)
    {
        workers.Remove(worker);
    }

    public void AddCurrentWorker(Entity worker)
    {
        currentWorkers.Add(worker);
        worker.SetWorkerIndex(currentWorkers.Count - 1);
        onResidentStartWorking?.Invoke();
        StartWorking();
    }

    public void RemoveCurrentWorker(Entity worker)
    {
        Debug.Log("RemoveCurrentWorker");
        currentWorkers.RemoveAt(worker.workerIndex);
        for (int i = 0; i < currentWorkers.Count; i++)
            currentWorkers[i].SetWorkerIndex(i);
        onResidentStopWorking?.Invoke();

        if (currentWorkers.Count == 0)
            StopWorking();
    }

    //protected virtual void UpdateBuildingConstruction(int levelIndex)
    //{
    //    BuildConstruction(levelIndex);
    //}

    public virtual void BuildConstruction(int levelIndex)
    {
        constructionComponent.BuildConstruction(buildingLevelsData[levelIndex].ConstructionStraight);
    }

    protected Building GetConnectedBuilding(Side side)
    {
        int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
        int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
        int verticalIndex = floorIndex + verticalIndexOffset;

        if (verticalIndex < cityManager.builtFloors.Count && verticalIndex >= 0)
        {
            Building building = cityManager.builtFloors[verticalIndex].roomBuildingPlaces[sideIndex].placedBuilding;
            if (building && building.buildingData.BuildingIdName == BuildingData.BuildingIdName && building.levelIndex == levelIndex)
                return building;
        }

        return null;
    }

    public Transform GetInteractionTransform()
    {
        int index = workers.Count > 0 ? ((workers.Count - 1) % currentLevelData.maxResidentsCount) : 0;
        BuildingAction[] actions = constructionComponent.SpawnedConstruction.BuildingInteractions;
        if (actions.Length > index) {
            Transform[] waypoints = actions[index].waypoints;
            if (waypoints.Length > 0) {
                return actions[index].waypoints[0];
            }
            else {
                Debug.LogError("waypoints.Length == 0");
                return transform;
            }
        }
        else {
            Debug.LogError("actions.Length <= index");
            return transform;
        }
    }

    public bool ConnectedWith(Building target)
    {
        if (!target) {
            Debug.Log("buildingToCheck == NULL");
            return false;
        }

        Building start = this;
        Building current = start;
        var visited = new HashSet<Building>();
        visited.Add(current);
        if (buildingData.ConnectionType == ConnectionType.Horizontal) {
            Building[] directions = { leftConnectedBuilding, rightConnectedBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.buildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == leftConnectedBuilding) ? current.leftConnectedBuilding : current.rightConnectedBuilding;
                }
            }
        }
        else if (buildingData.ConnectionType == ConnectionType.Vertical) {
            Building[] directions = { upConnectedBuilding, downConnectedBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.buildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == upConnectedBuilding) ? current.upConnectedBuilding : current.downConnectedBuilding;
                }
            }
        }
        return false;
    }
}
