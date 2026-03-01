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
    private BuildingStrategy strategy = null;

    private bool isWorking = false;
    public int LevelIndex => levelComponent ? levelComponent.LevelIndex : GetComponent<LevelComponent>().LevelIndex;

    //[HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<EntityCityNavigator> enteredEntities { get; private set; } = new List<EntityCityNavigator>();
    public List<EntityInteractor> workers { get; private set; } = new List<EntityInteractor>();
    public List<EntityInteractor> currentWorkers { get; private set; } = new List<EntityInteractor>();

    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> ConstructionLevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => ConstructionLevelsData.Count > LevelIndex ? ConstructionLevelsData[LevelIndex] : null;
    public BuildingLevelData NextLevelData => ConstructionLevelsData.Count > LevelIndex + 1 ? ConstructionLevelsData[LevelIndex] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public event System.Action onBuildingInited;
    public event System.Action onBuildingStartWorking;
    public event System.Action onBuildingStopWorking;
    public event System.Action<EntityCityNavigator> onEntityEnterBuilding;
    public event System.Action<EntityCityNavigator> onEntityExitBuilding;

    protected virtual void Awake()
    {
        AssignComponents();
    }

    protected virtual void OnEnable()
    {
        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    // Constructing
    public void Init(BuildingEntry data)
    {
        AssignComponents();
        CreateStrategy();

        OnInit(data);
        BuildConstruction();
        spawnedConstruction?.Init(this);

        onBuildingInited?.Invoke();
        EventBus.InvokeBuildingInitialized(this);
    }

    protected abstract void OnInit(BuildingEntry saveData);

    protected abstract BuildingConstruction GetConstruction();

    private void AssignComponents()
    {
        if (!levelComponent)
            levelComponent = GetComponent<LevelComponent>();
        if (!selectComponent)
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
    public void EnterBuilding(EntityCityNavigator navigator)
    {
        enteredEntities.Add(navigator);
        onEntityEnterBuilding?.Invoke(navigator);
        strategy.OnEntityEnter(navigator);
    }

    public void ExitBuilding(EntityCityNavigator navigator)
    {
        enteredEntities.Remove(navigator);
        onEntityExitBuilding?.Invoke(navigator);
        strategy.OnEntityExit(navigator);
    }

    public void AddWorker(EntityInteractor interactor)
    {
        workers.Add(interactor);
        strategy.OnSetInteractBuilding(interactor);
    }

    public void RemoveWorker(EntityInteractor interactor)
    {
        workers.Remove(interactor);

        strategy.OnRemoveInteractBuilding(interactor);
    }

    public void AddCurrentWorker(EntityInteractor interactor)
    {
        currentWorkers.Add(interactor);

        if (currentWorkers.Count == 1)
            StartWorking();

        strategy.OnStartInteracting(interactor);
    }

    public void RemoveCurrentWorker(EntityInteractor interactor)
    {
        currentWorkers.Remove(interactor);

        if (currentWorkers.Count == 0)
            StopWorking();

        strategy.OnStopInteracting(interactor);
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
            BuildingActionWaypoint[] waypoints = actions[index].waypoints;

            if (waypoints.Length > 0) {
                Transform waypointTransform = actions[index].waypoints[0].transform;

                if (waypointTransform) {
                    return waypointTransform;
                }
                else {
                    Debug.LogError("waypointTransform is not valid.");
                    return transform;
                }
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

    private void CreateStrategy()
    {
        switch (buildingData.BuildingStrategy) {
            case BuildingStrategyEnum.WorkBuilding:
                strategy = new WorkBuildingStrategy(this);
                break;
            case BuildingStrategyEnum.Pier:
                strategy = new PierBuildingStrategy(this);
                break;
        }
    }

    // Events
    private void OnSelected()
    {
        EventBus.InvokeSelectedBuilding(this);
    }

    private void OnDeselected()
    {
        EventBus.InvokeDeselectedBuilding(this);
    }
}