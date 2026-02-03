using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public abstract class BuildingEntry
{
    public int id;
    public int level;
}

public abstract class Building : MonoBehaviour
{
    protected LevelComponent levelComponent = null;
    protected SelectComponent selectComponent = null;

    public bool isInitialized { get; private set; } = false;
    private bool isWorking = false;
    public int LevelIndex => levelComponent ? levelComponent.LevelIndex : GetComponent<LevelComponent>().LevelIndex;

    //[HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<Creature> enteredEntities { get; private set; } = new List<Creature>();
    public List<Creature> workers { get; private set; } = new List<Creature>();
    public List<Creature> currentWorkers { get; private set; } = new List<Creature>();

    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> ConstructionLevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => ConstructionLevelsData.Count > LevelIndex ? ConstructionLevelsData[LevelIndex] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public event System.Action onBuildingInited;
    public event System.Action onBuildingStartWorking;
    public event System.Action onBuildingStopWorking;
    public event System.Action onEnterBuilding;
    public event System.Action onExitBuilding;
    public event System.Action onResidentStartWorking;
    public event System.Action onResidentStopWorking;

    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected abstract void Start();

    // Constructing
    public void Init(BuildingEntry data)
    {
        if (isInitialized) return;

        GetComponents();
        OnInit(data);
        BuildConstruction();

        isInitialized = true;
        onBuildingInited?.Invoke();
        EventBus.InvokeBuildingInitialized(this);
    }

    protected abstract void OnInit(BuildingEntry saveData);

    protected abstract BuildingConstruction GetConstruction();

    private void GetComponents()
    {
        levelComponent = GetComponent<LevelComponent>();
        selectComponent = GetComponent<SelectComponent>();
    }

    public void Demolish()
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

    private void BuildConstruction()
    {
        BuildingConstruction constructionPrefab = GetConstruction();
        if (constructionPrefab)
            spawnedConstruction = Instantiate(constructionPrefab, transform);
    }

    public Transform GetInteractionTransform()
    {
        int index = workers.Count > 0 ? ((workers.Count - 1) % LevelData.maxResidentsCount) : 0;
        BuildingAction[] actions = spawnedConstruction.BuildingInteractions;
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
}