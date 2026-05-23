using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Building : MonoBehaviour, IUpgradable, ILocalizable
{
    [SerializeField] protected ConstructionComponent constructionComponent;
    public ConstructionComponent ConstructionComponent => constructionComponent;

    public WorkComponent WorkComponent { get; private set; }
    public RaidComponent RaidComponent { get; private set; }

    [SerializeField] protected LevelComponent levelComponent;
    public LevelComponent LevelComponent => levelComponent;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    public SelectComponent SelectComponent { get; private set; }

    [Header("Audio")]
    [SerializeField] protected AudioSource workAudioSource;

    private BuildingStrategy strategy;

    public bool isWorking { get; private set; } = false;
    public bool IsDemolished { get; private set; } = false;

    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingDefinition buildingData = null;
    public BuildingDefinition BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> LevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => LevelsData.Count > levelComponent.Level - 1 ? LevelsData[levelComponent.Level - 1] : null;
    public BuildingLevelData NextLevelData => LevelsData.Count > levelComponent.Level ? LevelsData[levelComponent.Level] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public const float DemolishionResourcesRefundPercent = 0.2f;

    public event Action onInited;
    public event Action onWorkStarted;
    public event Action onWorkStopped;

    public event Action<CreatureCityNavigator> onEnterBuilding;
    public event Action<CreatureCityNavigator> onExitBuilding;

    public event Action<InteractComponent> onCurrentWorkerAdded;
    public event Action<InteractComponent> onCurrentWorkerRemoved;

    public event Action onConstructionStarted;
    public event Action onConstructionFinished;
    public event Action onDemolished;

    public event Action onClicked;

    public static event Action<Building> onBuildingInited;
    public static event Action<Building> onBuildingDemolished;

    public static event Action<Building> onBuildingConstructionStarted;
    public static event Action<Building> onBuildingConstructionFinished;

    public static event Action<Building> onBuildingSelected;
    public static event Action<Building> onBuildingDeselected;

    private void Awake()
    {
        SelectComponent = GetComponent<SelectComponent>();
        WorkComponent = GetComponent<WorkComponent>();
        RaidComponent = GetComponent<RaidComponent>();
    }

    protected virtual void OnEnable()
    {
        constructionComponent.OnConstructionStarted += OnConstructionStarted;
        constructionComponent.OnConstructionCompleted += OnConstructionFinished;

        WorkComponent.onWorkerAdded += OnWorkerAdded;
        WorkComponent.onWorkerRemoved += OnWorkerRemoved;
        WorkComponent.onWorkerEntered += OnCurrentWorkerAdded;
        WorkComponent.onWorkerExited += OnCurrentWorkerRemoved;

        SelectComponent.onSelected += OnSelected;
        SelectComponent.onDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        constructionComponent.OnConstructionStarted -= OnConstructionStarted;
        constructionComponent.OnConstructionCompleted -= OnConstructionFinished;

        WorkComponent.onWorkerAdded -= OnWorkerAdded;
        WorkComponent.onWorkerRemoved -= OnWorkerRemoved;
        WorkComponent.onWorkerEntered -= OnCurrentWorkerAdded;
        WorkComponent.onWorkerExited -= OnCurrentWorkerRemoved;

        SelectComponent.onSelected -= OnSelected;
        SelectComponent.onDeselected -= OnDeselected;
    }

    // Constructing
    public void Init(BuildingData data)
    {
        instanceId.Register(data.InstanceId);

        UpdateStrategy();
        constructionComponent.Init(data.Construction);

        OnInit(data);
        UpdateConstruction();

        onInited?.Invoke();
        onBuildingInited?.Invoke(this);
    }

    public void Demolish()
    {
        IsDemolished = true;
        OnDemolish();

        onDemolished?.Invoke();
        onBuildingDemolished?.Invoke(this);

        Destroy(gameObject);
    }

    protected abstract void OnInit(BuildingData saveData);

    protected abstract void OnDemolish();

    protected abstract BuildingConstruction GetConstructionToSpawn();

    // Residents Management
    public void EnterBuilding(CreatureCityNavigator navigator)
    {
        strategy.OnEntityEnter(navigator);
        onEnterBuilding?.Invoke(navigator);
    }

    public void ExitBuilding(CreatureCityNavigator navigator)
    {
        strategy.OnEntityExit(navigator);
        onExitBuilding?.Invoke(navigator);
    }

    // Click
    public void OnConstructionClicked()
    {
        SelectComponent.Click();
        onClicked?.Invoke();
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
            var data = resource.Definition;
            int amount = (int)(resource.Amount * DemolishionResourcesRefundPercent);

            var item = new ItemInstance(data);
            item.SetAmount(amount);

            resources[i] = item;
        }

        return resources;
    }

    // Interaction
    public Transform GetInteractionTransform()
    {
        int index = WorkComponent.Workers.Count > 0 ? ((WorkComponent.Workers.Count - 1) % LevelData.MaxHumansCount) : 0;
        BuildingAction[] actions = spawnedConstruction.BuildingInteractions;

        if (actions.Length > index) {
            BuildingActionWaypoint[] waypoints = actions[index].waypoints;

            if (waypoints.Length > 0) {
                Transform waypointTransform = actions[index].waypoints[0].transform;

                if (waypointTransform) {
                    return waypointTransform;
                }
                else {
                    Debug.Log("waypointTransform is not valid.");
                    return transform;
                }
            }
            else {
                Debug.Log("waypoints.Length == 0");
                return transform;
            }
        }
        else {
            Debug.Log("actions.Length <= index");
            return transform;
        }
    }

    public float GetUpgradeTime()
    {
        return LevelData.UpgradeTime;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "level", levelComponent.Level.ToString() },
            { "constructionTime", TimeFormatter.SecondsToMinuteTime((int)constructionComponent.CurrentConstructionTime).ToString() + "/" + TimeFormatter.SecondsToMinuteTime((int)constructionComponent.ConstructionTime).ToString() },
        };
    }

    // Construction
    protected virtual void OnConstructionStart()
    {

    }

    protected virtual void OnConstructionFinish()
    {

    }

    protected void UpdateConstruction()
    {
        if (spawnedConstruction) {
            Destroy(spawnedConstruction.gameObject);
        }

        BuildingConstruction constructionToSpawn = GetConstructionToSpawn();
        if (!constructionToSpawn) return;

        BuildingConstructionData data = new BuildingConstructionData()
        {
            BuildingInstanceId = instanceId.Id
        };

        spawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, transform, data);
    }

    private void OnWorkerAdded(InteractComponent interactor)
    {
        strategy.OnSetInteractBuilding(interactor);
    }

    private void OnWorkerRemoved(InteractComponent interactor)
    {
        strategy.OnRemoveInteractBuilding(interactor);
    }

    private void OnCurrentWorkerAdded(InteractComponent interactor)
    {
        if (WorkComponent.EnteredWorkers.Count == 1)
            StartWorking();

        strategy.OnStartedInteracting(interactor);
        onCurrentWorkerAdded?.Invoke(interactor);
    }

    private void OnCurrentWorkerRemoved(InteractComponent interactor)
    {
        if (WorkComponent.EnteredWorkers.Count == 0)
            StopWorking();

        strategy.OnStoppedInteracting(interactor);
        onCurrentWorkerRemoved?.Invoke(interactor);
    }

    // Working
    private void StartWorking()
    {
        if (isWorking) {
            Debug.Log("Building is already working");
            return;
        }

        PlayWorkSound();

        isWorking = true;
        onWorkStarted?.Invoke();
    }

    private void StopWorking()
    {
        if (!isWorking) {
            Debug.Log("Building is already not working");
            return;
        }

        StopWorkSound();

        isWorking = false;
        onWorkStopped?.Invoke();
    }

    private void UpdateStrategy()
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

    // Construction
    private void OnConstructionStarted()
    {
        OnConstructionStart();
        UpdateConstruction();

        onBuildingConstructionStarted?.Invoke(this);
        onConstructionStarted?.Invoke();
    }

    private void OnConstructionFinished()
    {
        OnConstructionFinish();
        UpdateConstruction();

        if (SelectComponent.IsSelected) {
            SelectComponent.Select();
        }

        onBuildingConstructionFinished?.Invoke(this);
        onConstructionFinished?.Invoke();
    }

    // Audio
    private void PlayWorkSound()
    {
        if (!workAudioSource) return;

        workAudioSource.Play();
    }

    private void StopWorkSound()
    {
        if (!workAudioSource) return;

        workAudioSource.Stop();
    }

    // Select
    private void OnSelected()
    {
        onBuildingSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        onBuildingDeselected?.Invoke(this);
    }
}