using System;
using System.Collections;
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

    [SerializeField] private UpgradeComponent upgradeComponent;
    public UpgradeComponent UpgradeComponent => upgradeComponent;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    public SelectComponent SelectComponent { get; private set; }

    [Header("Audio")]
    [SerializeField] protected AudioSource workAudioSource;

    private BuildingStrategy strategy;

    public bool isWorking { get; private set; } = false;
    public bool IsDemolished { get; private set; } = false;

    public BuildingConstruction SpawnedConstruction { get; private set; }

    [Header("Data")]
    [SerializeField] protected BuildingDefinition buildingData;
    public BuildingDefinition BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> LevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => LevelsData.Count > levelComponent.Level - 1 ? LevelsData[levelComponent.Level - 1] : null;
    public BuildingLevelData NextLevelData => LevelsData.Count > levelComponent.Level ? LevelsData[levelComponent.Level] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public const float DemolishionResourcesRefundPercent = 0.2f;

    public bool IsInited { get; private set; } = false;

    public event Action OnInited;
    public event Action OnWorkStarted;
    public event Action OnWorkStopped;

    public event Action<CreatureCityNavigator> onEnterBuilding;
    public event Action<CreatureCityNavigator> onExitBuilding;

    public event Action<InteractComponent> onCurrentWorkerAdded;
    public event Action<InteractComponent> onCurrentWorkerRemoved;

    public event Action onConstructionStarted;
    public event Action onConstructionFinished;

    public event Action OnDemolished;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingInited;
    public static event Action<Building> OnBuildingDemolished;

    public static event Action<Building> OnBuildingConstructionStarted;
    public static event Action<Building> OnBuildingConstructionFinished;

    public static event Action<Building> OnBuildingSelected;
    public static event Action<Building> OnBuildingDeselected;

    private void Awake()
    {
        SelectComponent = GetComponent<SelectComponent>();
        WorkComponent = GetComponent<WorkComponent>();
        RaidComponent = GetComponent<RaidComponent>();
    }

    protected virtual void OnEnable()
    {
        constructionComponent.OnConstructionStarted += OnConstructionStarted;
        levelComponent.OnLevelChanged += OnLevelChanged;

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
        levelComponent.OnLevelChanged -= OnLevelChanged;

        WorkComponent.onWorkerAdded -= OnWorkerAdded;
        WorkComponent.onWorkerRemoved -= OnWorkerRemoved;
        WorkComponent.onWorkerEntered -= OnCurrentWorkerAdded;
        WorkComponent.onWorkerExited -= OnCurrentWorkerRemoved;

        SelectComponent.onSelected -= OnSelected;
        SelectComponent.onDeselected -= OnDeselected;
    }

    // Constructing
    public void Init(BuildingData buildingData)
    {
        OnInit(buildingData);

        IsInited = true;
        OnInited?.Invoke();
        OnBuildingInited?.Invoke(this);
    }

    public void Demolish()
    {
        IsDemolished = true;
        OnDemolish();

        OnDemolished?.Invoke();
        OnBuildingDemolished?.Invoke(this);

        Destroy(gameObject);
    }

    protected virtual void OnInit(BuildingData buildingData)
    {
        instanceId.Register(buildingData.InstanceId);
        UpdateStrategy();
        levelComponent.Init(buildingData.Level);
        upgradeComponent.Init(buildingData.Upgrade);
        constructionComponent.Init(buildingData.Construction);

        GetComponent<CraftingModule>()?.Init(buildingData.Crafting);
    }

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
        OnClicked?.Invoke();
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
        BuildingAction[] actions = SpawnedConstruction.BuildingInteractions;

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
        return NextLevelData.UpgradeTime;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "name", LocalizationManager.Instance.GetText(buildingData.NameLocalizationItem) },
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
        if (!InstanceId.IsRegistered) return;

        var constructionToSpawn = GetConstructionToSpawn();
        if (!constructionToSpawn) return;

        if (constructionToSpawn == SpawnedConstruction) return;

        if (SpawnedConstruction) {
            Destroy(SpawnedConstruction.gameObject);
        }

        var data = new BuildingConstructionData()
        {
            BuildingInstanceId = instanceId.GetId()
        };

        SpawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, transform, data);
    }

    // Work
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
        OnWorkStarted?.Invoke();
    }

    private void StopWorking()
    {
        if (!isWorking) {
            Debug.Log("Building is already not working");
            return;
        }

        StopWorkSound();

        isWorking = false;
        OnWorkStopped?.Invoke();
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

        OnBuildingConstructionStarted?.Invoke(this);
        onConstructionStarted?.Invoke();
    }

    private void OnLevelChanged()
    {
        OnConstructionFinish();
        UpdateConstruction();

        if (SelectComponent.IsSelected) {
            SelectComponent.Select();
        }

        OnBuildingConstructionFinished?.Invoke(this);
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
        OnBuildingSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        OnBuildingDeselected?.Invoke(this);
    }
}