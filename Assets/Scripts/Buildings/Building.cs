using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<Human> enteredEntities { get; private set; } = new List<Human>();
    public List<Human> workers { get; private set; } = new List<Human>();
    public List<Human> currentWorkers { get; private set; } = new List<Human>();

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
    public event System.Action onEntityEnterBuilding;
    public event System.Action onEntityExitBuilding;

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
        spawnedConstruction?.Init(this);

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
    public virtual void EnterBuilding(Human entity)
    {
        enteredEntities.Add(entity);
        onEntityEnterBuilding?.Invoke();
    }

    public virtual void ExitBuilding(Human entity)
    {
        enteredEntities.Remove(entity);
        onEntityExitBuilding?.Invoke();
    }

    public void AddWorker(Human interactor)
    {
        workers.Add(interactor);
    }

    public void RemoveWorker(Human interactor)
    {
        workers.Remove(interactor);

        if (currentWorkers.Count == 0)
            StopWorking();
    }

    public  void AddCurrentWorker(Human interactor)
    {
        currentWorkers.Add(interactor);

        if (currentWorkers.Count == 1)
            StartWorking();
    }

    public void RemoveCurrentWorker(Human interactor)
    {
        currentWorkers.Remove(interactor);

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