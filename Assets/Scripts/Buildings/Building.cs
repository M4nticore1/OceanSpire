using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour, ILevelable, ISelectable
{
    public ConstructionComponent constructionComponent { get; protected set; } = null;
    //public SelectComponent selectComponent { get; protected set; } = null;
    public StorageBuildingComponent storageComponent { get; protected set; } = null;
    public ProductionBuilding productionComponent { get; protected set; } = null;

    public bool isInitialized { get; protected set; } = false;
    private int levelIndex = 0;
    public int LevelIndex { get { return levelIndex; } set { levelIndex = value; } }
    private bool isSelected = false;
    public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
    private bool isWorking = false;

    //[HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<Creature> enteredEntities { get; private set; } = new List<Creature>();
    public List<Creature> workers { get; private set; } = new List<Creature>();
    public List<Creature> currentWorkers { get; private set; } = new List<Creature>();

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected List<ConstructionLevelData> buildingLevelsData = new List<ConstructionLevelData>();
    public List<ConstructionLevelData> ConstructionLevelsData => buildingLevelsData;
    public ConstructionLevelData currentLevelData => ConstructionLevelsData.Count > LevelIndex ? ConstructionLevelsData[LevelIndex] : null;

    public BuildingPlace buildingPlace { get; protected set; } = null;

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
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuilding>();
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

    // Constructing
    public virtual void InitializeBuilding(BuildingPlace buildingPlace, bool isUnderConstruction, int levelIndex, int interiorIndex = -1)
    {
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuilding>();

        this.buildingPlace = buildingPlace;
        this.LevelIndex = levelIndex;

        if (storageComponent)
            storageComponent.Initialize();
        if (productionComponent)
            productionComponent.Initialize();

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
        BuildConstruction(LevelIndex);
    }

    public virtual void FinishConstructing()
    {
        if (BuildingData.BuildingIdName == "floor_frame") return;

        BuildConstruction(LevelIndex);
        onBuildingFinishConstructing?.Invoke();
    }

    protected void Demolish()
    {

    }

    public void SetLevel(int level)
    {
        LevelIndex = level;
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
    public virtual void EnterBuilding(Creature entity)
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

    public virtual void ExitBuilding(Creature entity)
    {
        enteredEntities.Remove(entity);
        onExitBuilding?.Invoke();
    }

    public void AddWorker(Creature worker)
    {
        workers.Add(worker);
    }

    public void RemoveWorker(Creature worker)
    {
        workers.Remove(worker);
    }

    public  void AddCurrentWorker(Creature worker)
    {
        currentWorkers.Add(worker);
        worker.SetWorkerIndex(currentWorkers.Count - 1);
        onResidentStartWorking?.Invoke();
        StartWorking();
    }

    public void RemoveCurrentWorker(Creature worker)
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

    public void Select()
    {
        IsSelected = true;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
    }

    public void Deselect()
    {
        IsSelected = false;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Default");
        }
    }
}