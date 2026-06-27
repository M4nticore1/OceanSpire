using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour, IUpgradable, ILocalizable
{
    [Header("Data")]
    [SerializeField] protected BuildingDefinition buildingData;
    public BuildingDefinition BuildingData => buildingData;

    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public IReadOnlyList<BuildingLevelData> LevelsData => buildingLevelsData;

    public BuildingLevelData LevelData => LevelsData.Count > levelComponent.Level - 1 ? LevelsData[levelComponent.Level - 1] : null;
    public BuildingLevelData NextLevelData => LevelsData.Count > levelComponent.Level ? LevelsData[levelComponent.Level] : null;

    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    [Header("Main")]
    [SerializeField] protected ConstructionComponent constructionComponent;
    public ConstructionComponent ConstructionComponent => constructionComponent;

    private WorkComponent workComponent;
    public WorkComponent WorkComponent => workComponent ? workComponent : GetComponent<WorkComponent>();

    private RaidComponent raidComponent;
    public RaidComponent RaidComponent => raidComponent ? raidComponent : GetComponent<RaidComponent>();

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

    public BuildingConstruction SpawnedConstruction;

    public const float DemolishionResourcesRefundPercent = 0.2f;

    public bool IsInited { get; private set; } = false;

    public event Action OnInited;
    public event Action OnWorkStarted;
    public event Action OnWorkStopped;

    public event Action<CreatureCityNavigator> onEnterBuilding;
    public event Action<CreatureCityNavigator> onExitBuilding;

    public event Action OnUpgradeStarted;
    public event Action OnUpgradeCompleted;

    public event Action OnLevelChanged;
    public event Action OnDemolished;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingInited;
    public static event Action<Building> OnBuildingDemolished;

    public static event Action<Building> OnBuildingUpgradeStarted;
    public static event Action<Building> OnBuildingUpgradeCompleted;

    public static event Action<Building> OnBuildingConstructionStarted;
    public static event Action<Building> OnBuildingConstructionCompleted;

    public static event Action<Building> OnBuildingLevelChanged;

    public static event Action<Building> OnBuildingSelected;
    public static event Action<Building> OnBuildingDeselected;

    private void Awake()
    {
        SelectComponent = GetComponent<SelectComponent>();
        workComponent = GetComponent<WorkComponent>();
        raidComponent = GetComponent<RaidComponent>();
    }

    protected virtual void OnEnable()
    {
        levelComponent.OnLevelChanged += HandleLevelChanged;

        constructionComponent.OnConstructionStarted += HandleConstructionStarted;
        constructionComponent.OnConstructionCompleted += HandleConstructionCompleted;

        upgradeComponent.OnUpgradeStarted += HandleUpgradeStarted;
        upgradeComponent.OnUpgradeCompleted += HandleUpgradeCompleted;

        WorkComponent.OnWorkerAdded += OnWorkerAdded;
        WorkComponent.OnWorkerRemoved += OnWorkerRemoved;
        WorkComponent.OnCurrentWorkerAdded += OnCurrentWorkerAdded;
        WorkComponent.OnCurrentWorkerRemoved += OnCurrentWorkerRemoved;

        RaidComponent.OnRaiderAdded += OnRaiderAdded;
        RaidComponent.OnRaiderRemoved += OnRaiderRemoved;

        SelectComponent.OnSelected += OnSelected;
        SelectComponent.OnDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        levelComponent.OnLevelChanged -= HandleLevelChanged;

        constructionComponent.OnConstructionStarted -= HandleConstructionStarted;
        constructionComponent.OnConstructionCompleted -= HandleConstructionCompleted;

        upgradeComponent.OnUpgradeStarted -= HandleUpgradeStarted;
        upgradeComponent.OnUpgradeCompleted -= HandleUpgradeCompleted;

        WorkComponent.OnWorkerAdded -= OnWorkerAdded;
        WorkComponent.OnWorkerRemoved -= OnWorkerRemoved;
        WorkComponent.OnCurrentWorkerAdded -= OnCurrentWorkerAdded;
        WorkComponent.OnCurrentWorkerRemoved -= OnCurrentWorkerRemoved;

        RaidComponent.OnRaiderAdded -= OnRaiderAdded;
        RaidComponent.OnRaiderRemoved -= OnRaiderRemoved;

        SelectComponent.OnSelected -= OnSelected;
        SelectComponent.OnDeselected -= OnDeselected;
    }

    // Constructing
    public void Init(BuildingData buildingData)
    {
        OnInit(buildingData);
        UpdateConstruction();

        GetComponent<CraftingModule>()?.Init(buildingData.Crafting);

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
        instanceId.SetGuid(buildingData.InstanceId);
        UpdateStrategy();
        constructionComponent.Init(buildingData.Construction);
        upgradeComponent.Init(buildingData.Upgrade);
        levelComponent.Init(buildingData.Level);
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

    public int GetUpgradeTime()
    {
        return NextLevelData.UpgradeTime;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var currentConstructionTime = currentTime - constructionComponent.ConstructionStartTime;
        var ConstructionTime = constructionComponent.ConstructionFinishTime - constructionComponent.ConstructionStartTime;

        return new Dictionary<string, string>()
        {
            { "name", LocalizationManager.Instance.GetText(buildingData.NameLocalizationItem) },
            { "level", levelComponent.Level.ToString() },
            { "constructionTime", TimeFormatter.SecondsToMinuteTime((int)currentConstructionTime).ToString() + "/" + TimeFormatter.SecondsToMinuteTime((int)ConstructionTime).ToString() },
        };
    }

    // Construction
    protected virtual void OnConstructionStart()
    {
        if (!SpawnedConstruction) return;

        SpawnedConstruction.UpdateInteractTransforms();
    }

    protected virtual void OnConstructionComplete()
    {
        if (!SpawnedConstruction) return;

        SpawnedConstruction.UpdateInteractTransforms();
    }

    protected virtual void OnLevelChange()
    {

    }

    protected void UpdateConstruction()
    {
        var constructionToSpawn = GetConstructionToSpawn();
        if (!constructionToSpawn) return;

        if (constructionToSpawn == SpawnedConstruction) return;

        if (SpawnedConstruction) {
            Destroy(SpawnedConstruction.gameObject);
            SpawnedConstruction = null;
        }

        var data = new BuildingConstructionData()
        {
            BuildingInstanceId = instanceId.GetGuid()
        };

        SpawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, transform, data);
    }

    // Work
    private void OnWorkerAdded(Citizen citizen)
    {
        //UpdateWorkerInteractionTransforms();

        strategy.OnInteractBuildingSet(citizen.InteractComponent);
    }

    private void OnWorkerRemoved(Citizen citizen)
    {
        //UpdateWorkerInteractionTransforms();

        strategy.OnInteractBuildingRemove(citizen.InteractComponent);
    }

    private void OnCurrentWorkerAdded(Citizen citizen)
    {
        if (WorkComponent.CurrentWorkers.Count == 1)
            StartWorking();

        strategy.OnStartedInteracting(citizen.InteractComponent);
    }

    private void OnCurrentWorkerRemoved(Citizen citizen)
    {
        if (WorkComponent.CurrentWorkers.Count == 0)
            StopWorking();

        strategy.OnStoppedInteracting(citizen.InteractComponent);
    }

    // Raid
    private void OnRaiderAdded(Raider raider)
    {
        //UpdateRaiderInteractionTransforms();
    }

    private void OnRaiderRemoved(Raider raider)
    {
        //UpdateRaiderInteractionTransforms();
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

    private void HandleConstructionStarted()
    {
        OnBuildingConstructionStarted?.Invoke(this);
    }

    private void HandleConstructionCompleted()
    {
        OnBuildingConstructionCompleted?.Invoke(this);
    }

    private void HandleUpgradeStarted()
    {
        UpdateConstruction();
        OnConstructionComplete();

        if (SelectComponent.IsSelected) {
            SelectComponent.Select();
        }

        OnUpgradeStarted?.Invoke();
        OnBuildingUpgradeStarted?.Invoke(this);
    }

    private void HandleUpgradeCompleted()
    {
        UpdateConstruction();
        OnConstructionComplete();

        if (SelectComponent.IsSelected) {
            SelectComponent.Select();
        }

        OnUpgradeCompleted?.Invoke();
        OnBuildingUpgradeCompleted?.Invoke(this);
    }

    private void HandleLevelChanged()
    {
        OnLevelChange();

        OnLevelChanged?.Invoke();
        OnBuildingLevelChanged?.Invoke(this);
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

    //private BuildingAction GetNextAction()
    //{
    //    if (!SpawnedConstruction) {
    //        Debug.LogError("SpawnedConstruction is not valid ", this);
    //        return null;
    //    }

    //    var actions = SpawnedConstruction.BuildingInteractions;
    //    if (actions.Length == 0)
    //        return null;

    //    var interactorsCount = interactTransformsDict.Values.Count;

    //    var actionIndex = interactorsCount % actions.Length;
    //    actionIndex = Mathf.Clamp(actionIndex, 0, actionIndex);

    //    var action = actions[actionIndex];

    //    if (action.waypoints == null || action.waypoints.Length == 0)
    //        return null;

    //    return action.waypoints[0];
    //}
}