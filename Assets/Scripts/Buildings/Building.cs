using System;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<CreatureCityNavigator, Transform> interactTransformsDict = new();
    public IReadOnlyDictionary<CreatureCityNavigator, Transform> InteractTransformsDict => interactTransformsDict;

    private List<Transform> interactTransformsList = new();
    public IReadOnlyList<Transform> InteractTransformsList => interactTransformsList;

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

    public event Action<BuildingInteractComponent> onCurrentWorkerAdded;
    public event Action<BuildingInteractComponent> onCurrentWorkerRemoved;

    public event Action OnUpgradeStarted;
    public event Action OnUpgradeCompleted;

    public event Action OnLevelChanged;
    public event Action OnDemolished;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingInited;
    public static event Action<Building> OnBuildingDemolished;

    public static event Action<Building> OnBuildingUpgradeStarted;
    public static event Action<Building> OnBuildingUpgradeCompleted;

    public static event Action<Building> OnBuildingLevelChanged;

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
        //constructionComponent.OnConstructionStarted += HandleConstructionStarted;
        //constructionComponent.OnConstructionCompleted += HandleConstructionCompleted;

        levelComponent.OnLevelChanged += HandleLevelChanged;

        upgradeComponent.OnUpgradeStarted += HandleUpgradeStarted;
        upgradeComponent.OnUpgradeCompleted += HandleUpgradeCompleted;

        WorkComponent.OnWorkerAdded += OnWorkerAdded;
        WorkComponent.OnWorkerRemoved += OnWorkerRemoved;
        WorkComponent.OnWorkerEntered += OnCurrentWorkerAdded;
        WorkComponent.OnWorkerExited += OnCurrentWorkerRemoved;

        RaidComponent.OnRaiderAdded += OnRaiderAdded;
        RaidComponent.OnRaiderRemoved += OnRaiderRemoved;

        SelectComponent.OnSelected += OnSelected;
        SelectComponent.OnDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        //constructionComponent.OnConstructionStarted -= HandleConstructionStarted;
        //constructionComponent.OnConstructionCompleted -= HandleConstructionCompleted;

        levelComponent.OnLevelChanged -= HandleLevelChanged;

        upgradeComponent.OnUpgradeStarted += HandleUpgradeStarted;
        upgradeComponent.OnUpgradeCompleted += HandleUpgradeCompleted;

        WorkComponent.OnWorkerAdded -= OnWorkerAdded;
        WorkComponent.OnWorkerRemoved -= OnWorkerRemoved;
        WorkComponent.OnWorkerEntered -= OnCurrentWorkerAdded;
        WorkComponent.OnWorkerExited -= OnCurrentWorkerRemoved;

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
        constructionComponent.Init(buildingData.Construction);
        upgradeComponent.Init(buildingData.Upgrade);
        levelComponent.Init(buildingData.Level);

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
    public void AssignInteractTransform(CreatureCityNavigator navigator)
    {
        if (interactTransformsDict.ContainsKey(navigator)) {
            interactTransformsDict.Remove(navigator);
        }

        interactTransformsDict.Add(navigator, GetFirstWaypointTransform());
    }

    public void TryRemoveInteractTransform(CreatureCityNavigator navigator)
    {
        if (!interactTransformsDict.ContainsKey(navigator)) return;

        interactTransformsDict.Remove(navigator);
    }

    public Transform GetInteractionTransform(CreatureCityNavigator navigator)
    {
        if (!interactTransformsDict.ContainsKey(navigator))
            return transform;

        return interactTransformsDict[navigator];
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
        UpdateInteractTransforms();
    }

    protected virtual void OnConstructionComplete()
    {
        UpdateInteractTransforms();
    }

    protected virtual void OnLevelChange()
    {

    }

    protected void UpdateConstruction()
    {
        if (!InstanceId.IsRegistered) {
            Debug.Log($"Instance Id is not registrated at {name}");
            return;
        }

        var constructionToSpawn = GetConstructionToSpawn();
        if (!constructionToSpawn) return;

        if (constructionToSpawn == SpawnedConstruction) return;

        if (SpawnedConstruction) {
            Destroy(SpawnedConstruction.gameObject);
            SpawnedConstruction = null;
        }

        var data = new BuildingConstructionData()
        {
            BuildingInstanceId = instanceId.GetInstanceId()
        };

        SpawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, transform, data);
    }

    // Work
    private void OnWorkerAdded(Citizen citizen)
    {
        UpdateWorkerInteractionTransforms();

        strategy.OnInteractBuildingSet(citizen.InteractComponent);
    }

    private void OnWorkerRemoved(Citizen citizen)
    {
        UpdateWorkerInteractionTransforms();

        strategy.OnInteractBuildingRemove(citizen.InteractComponent);
    }

    private void OnCurrentWorkerAdded(Citizen citizen)
    {
        if (WorkComponent.CurrentWorkers.Count == 1)
            StartWorking();

        strategy.OnStartedInteracting(citizen.InteractComponent);
        onCurrentWorkerAdded?.Invoke(citizen.InteractComponent);
    }

    private void OnCurrentWorkerRemoved(Citizen citizen)
    {
        if (WorkComponent.CurrentWorkers.Count == 0)
            StopWorking();

        strategy.OnStoppedInteracting(citizen.InteractComponent);
        onCurrentWorkerRemoved?.Invoke(citizen.InteractComponent);
    }

    // Raid
    private void OnRaiderAdded(Raider raider)
    {
        UpdateRaiderInteractionTransforms();
    }

    private void OnRaiderRemoved(Raider raider)
    {
        UpdateRaiderInteractionTransforms();
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
    //private void HandleConstructionStarted()
    //{
    //    UpdateConstruction();
    //    OnConstructionStart();

    //    OnConstructionStarted?.Invoke();
    //    OnBuildingConstructionStarted?.Invoke(this);
    //}

    //private void HandleConstructionCompleted()
    //{
    //    UpdateConstruction();
    //    OnConstructionComplete();

    //    OnConstructionCompleted?.Invoke();
    //    OnBuildingConstructionCompleted?.Invoke(this);
    //}

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

    private void UpdateWorkerInteractionTransforms()
    {
        for (int i = 0; i < WorkComponent.Workers.Count; i++) {
            var worker = WorkComponent.Workers[i];
            var navigator = worker.CityNavigator;

            AssignInteractTransform(navigator);
        }
    }

    private void UpdateRaiderInteractionTransforms()
    {
        for (int i = 0; i < RaidComponent.Raiders.Count; i++) {
            var raider = RaidComponent.Raiders[i];
            var navigator = raider.CityNavigator;

            AssignInteractTransform(navigator);
        }
    }

    private void UpdateInteractTransforms()
    {
        interactTransformsList.Clear();

        var keys = interactTransformsDict.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++) {
            if (i >= SpawnedConstruction.BuildingInteractions.Length) break;

            var transform = SpawnedConstruction.BuildingInteractions[i].waypoints[0].transform;
            interactTransformsDict[keys[i]] = transform;
            interactTransformsList.Add(transform);
        }
    }

    private Transform GetFirstWaypointTransform()
    {
        var actions = SpawnedConstruction.BuildingInteractions;
        if (actions.Length == 0)
            return transform;

        var interactorsCount = interactTransformsDict.Values.Count;

        var actionIndex = interactorsCount % actions.Length;
        actionIndex = Mathf.Clamp(actionIndex, 0, actionIndex);

        var action = actions[actionIndex];

        if (action.waypoints == null || action.waypoints.Length == 0)
            return transform;

        return action.waypoints[0].transform;
    }
}