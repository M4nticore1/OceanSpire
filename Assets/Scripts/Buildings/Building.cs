using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingEntry
{
    public int id;
    public int level;
}

public abstract class Building : MonoBehaviour
{
    protected LevelComponent levelComponent;
    private BuildingStrategy strategy;

    private bool isWorking = false;
    public int LevelIndex => levelComponent ? levelComponent.LevelIndex : GetComponent<LevelComponent>().LevelIndex;

    public List<EntityCityNavigator> enteredEntities { get; private set; } = new List<EntityCityNavigator>();
    public List<EntityInteractor> workers { get; private set; } = new List<EntityInteractor>();
    public List<EntityInteractor> currentWorkers { get; private set; } = new List<EntityInteractor>();

    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> LevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => LevelsData.Count > LevelIndex ? LevelsData[LevelIndex] : null;
    public BuildingLevelData NextLevelData => LevelsData.Count > LevelIndex + 1 ? LevelsData[LevelIndex] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public const float DemolishionResourcesRefundPercent = 0.2f;

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
        EventBus.onClickedContextDemolishButton += OnDemolishClicked;
        EventBus.onClickedContextUpgradeButton += OnUpgradeClicked;
    }

    protected virtual void OnDisable()
    {
        EventBus.onClickedContextDemolishButton -= OnDemolishClicked;
        EventBus.onClickedContextUpgradeButton -= OnUpgradeClicked;
    }

    // Constructing
    public void Init(BuildingEntry data)
    {
        AsssignStrategy();
        OnInit(data);
        UpdateConstruction();

        onBuildingInited?.Invoke();
        EventBus.InvokeBuildingInited(this);

        InvokeBuildingInited();
    }

    public void Demolish()
    {
        Destroy(gameObject);
        EventBus.InvokeBuildingDemolished(this);

        InvokeBuildingDemolished();
    }

    protected abstract void OnInit(BuildingEntry saveData);

    protected abstract BuildingConstruction GetConstructionToSpawn();

    protected virtual void InvokeBuildingInited()
    {
        foreach (var module in GetComponents<IOwnedBuildingListener>()) {
            module.HandleOwnedBuildingInited();
        }
    }

    protected virtual void InvokeBuildingDemolished()
    {
        foreach (var module in GetComponents<IOwnedBuildingListener>()) {
            module.HandleOwnedBuildingDemolished();
        }
    }

    private void AssignComponents()
    {
        levelComponent = GetComponent<LevelComponent>();
    }

    // Residents Management
    public void EnterBuilding(EntityCityNavigator navigator)
    {
        enteredEntities.Add(navigator);
        onEntityEnterBuilding?.Invoke(navigator);
        strategy.OnEntityEnter(navigator);
        InvokeEnterBuilding(navigator);
    }

    public void ExitBuilding(EntityCityNavigator navigator)
    {
        enteredEntities.Remove(navigator);
        onEntityExitBuilding?.Invoke(navigator);
        strategy.OnEntityExit(navigator);
        InvokeExitBuilding(navigator);
    }

    // Workers
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
        InvokeCurrentWorkerAdded(interactor);
    }

    public void RemoveCurrentWorker(EntityInteractor interactor)
    {
        currentWorkers.Remove(interactor);

        if (currentWorkers.Count == 0)
            StopWorking();

        strategy.OnStopInteracting(interactor);
        InvokeCurrentWorkerRemoved(interactor);
    }

    // Interaction
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
                    Debug.LogWarning("waypointTransform is not valid.");
                    return transform;
                }
            }
            else {
                Debug.LogWarning("waypoints.Length == 0");
                return transform;
            }
        }
        else {
            Debug.LogWarning("actions.Length <= index");
            return transform;
        }
    }

    // Events
    private void InvokeEnterBuilding(EntityCityNavigator navigator)
    {
        foreach (var listener in GetComponentsInChildren<IEnterExitListener>()) {
            listener.OnEnterBuilding(navigator);
        }
    }

    private void InvokeExitBuilding(EntityCityNavigator navigator)
    {
        foreach (var listener in GetComponentsInChildren<IEnterExitListener>()) {
            listener.OnExitBuilding(navigator);
        }
    }

    private void InvokeCurrentWorkerAdded(EntityInteractor interactor)
    {
        foreach (var listener in GetComponentsInChildren<ICurrentWorkersListener>()) {
            listener.OnCurrentWorkerAdded(interactor);
        }
    }

    private void InvokeCurrentWorkerRemoved(EntityInteractor interactor)
    {
        foreach (var listener in GetComponentsInChildren<ICurrentWorkersListener>()) {
            listener.OnCurrentWorkerRemoved(interactor);
        }
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

    // Construction
    protected void UpdateConstruction()
    {
        if (spawnedConstruction) {
            Destroy(spawnedConstruction.gameObject);
        }

        BuildingConstruction constructionToSpawn = GetConstructionToSpawn();
        if (!constructionToSpawn) return;

        spawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, this);
        OnChangedConstruction();
    }

    protected virtual void OnChangedConstruction()
    {
        //foreach (BuildingModule module in GetComponents<BuildingModule>()) {
        //    module.HandleChangedConstruction();
        //}
    }

    private void AsssignStrategy()
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

    // Modules
    public BuildingModule[] GetModules()
    {
        BuildingModule[] modules;
        modules = GetComponents<BuildingModule>();

        return modules;
    }

    // Cost
    public ItemInstance[] GetResourcesToBuild()
    {
        return LevelData.ResourcesToBuild;
    }

    public ItemInstance[] GetResourcesToRefund()
    {
        int count = LevelData.ResourcesToBuild.Length;
        var resources = new ItemInstance[count];

        for (int i = 0; i < count; i++) {
            var resource = LevelData.ResourcesToBuild[i];
            var data = resource.ItemData;
            int amount = (int)(resource.Amount * DemolishionResourcesRefundPercent);
            var instance = new ItemInstance(data, amount);
            resources[i] = instance;
        }

        return resources;
    }

    // Events
    private void OnDemolishClicked()
    {
        
    }

    private void OnUpgradeClicked()
    {

    }
}