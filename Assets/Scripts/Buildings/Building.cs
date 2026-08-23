using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Building : MonoBehaviour, IUpgradable, ILocalizable, IInformationable
{
    [Header("Data")]
    [SerializeField] protected BuildingDefinition buildingData;
    public BuildingDefinition Definition => buildingData;

    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public IReadOnlyList<BuildingLevelData> LevelsDefinitions => buildingLevelsData;

    public BuildingLevelData LevelDefinition => LevelsDefinitions.Count > levelComponent.Level - 1 ? LevelsDefinitions[levelComponent.Level - 1] : null;
    public BuildingLevelData NextLevelDefinition => LevelsDefinitions.Count > levelComponent.Level ? LevelsDefinitions[levelComponent.Level] : null;

    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    [Header("Main")]
    [SerializeField] protected ConstructionComponent constructionComponent;
    public ConstructionComponent ConstructionComponent => constructionComponent;

    [SerializeField] protected LevelComponent levelComponent;
    public LevelComponent LevelComponent => levelComponent;

    [SerializeField] private UpgradeComponent upgradeComponent;
    public UpgradeComponent UpgradeComponent => upgradeComponent;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private SkillId skillId;
    public SkillId SkillId => skillId;

    private BuildingCitizensHandler citizensHandler;
    public BuildingCitizensHandler CitizensHandler => citizensHandler != null ? citizensHandler : GetComponent<BuildingCitizensHandler>();

    private BuildingRaidersHandler raidersHandler;
    public BuildingRaidersHandler RaidersHandler => raidersHandler != null ? raidersHandler : GetComponent<BuildingRaidersHandler>();

    public SelectComponent SelectComponent { get; private set; }

    public List<BuildingModule> buildingModules = new();

    [Header("Audio")]
    [SerializeField] protected AudioSource workAudioSource;

    private BuildingStrategy buildingStrategy;

    public bool isWorking { get; private set; } = false;
    public bool IsDemolished { get; private set; } = false;

    public BuildingConstruction SpawnedConstruction;

    public const float DemolishionResourcesRefundPercent = 0.5f;

    public bool IsInited { get; private set; } = false;

    private Coroutine updateConstructionCoroutine;
    private Coroutine refreshConstructionCoroutine;

    public event Action OnInited;
    public event Action OnWorkStarted;
    public event Action OnWorkStopped;

    public event Action<CreatureCityNavigator> OnEnteredBuilding;
    public event Action<CreatureCityNavigator> onExitBuilding;

    public event Action OnConstructionStarted;
    public event Action OnConstructionFinished;

    public event Action OnUpgradeStarted;
    public event Action OnUpgradeFinished;

    public event Action OnLevelChanged;
    public event Action OnDemolished;

    public event Action<BuildingConstruction> OnConstructionChanged;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingInited;
    public static event Action<Building> OnBuildingDemolished;

    public static event Action<Building> OnBuildingUpgradeStarted;
    public static event Action<Building> OnBuildingUpgradeFinished;

    public static event Action<Building> OnBuildingConstructionStarted;
    public static event Action<Building> OnBuildingConstructionFinished;

    public static event Action<Building> OnBuildingLevelChanged;

    public static event Action<Building> OnBuildingSelected;
    public static event Action<Building> OnBuildingDeselected;

    protected virtual void Awake()
    {
        citizensHandler = GetComponent<BuildingCitizensHandler>();
        raidersHandler = GetComponent<BuildingRaidersHandler>();
        SelectComponent = GetComponent<SelectComponent>();
        buildingModules = GetComponents<BuildingModule>().ToList();
    }

    protected virtual void OnEnable()
    {
        levelComponent.OnLevelChanged += HandleLevelChanged;

        constructionComponent.OnConstructionStarted += HandleConstructionStarted;
        constructionComponent.OnConstructionFinished += HandleConstructionFinished;

        upgradeComponent.OnUpgradeStarted += HandleUpgradeStarted;
        upgradeComponent.OnUpgradeFinished += HandleUpgradeFinished;

        CitizensHandler.OnInteractorAdded += OnWorkerAdded;
        CitizensHandler.OnInteractorRemoved += OnWorkerRemoved;
        CitizensHandler.OnCurrentInteractorAdded += OnCurrentWorkerAdded;
        CitizensHandler.OnCurrentInteractorRemoved += OnCurrentWorkerRemoved;

        RaidersHandler.OnInteractorAdded += OnRaiderAdded;
        RaidersHandler.OnInteractorAdded += OnRaiderRemoved;

        SelectComponent.OnSelected += OnSelected;
        SelectComponent.OnDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        levelComponent.OnLevelChanged -= HandleLevelChanged;

        constructionComponent.OnConstructionStarted -= HandleConstructionStarted;
        constructionComponent.OnConstructionFinished -= HandleConstructionFinished;

        upgradeComponent.OnUpgradeStarted -= HandleUpgradeStarted;
        upgradeComponent.OnUpgradeFinished -= HandleUpgradeFinished;

        CitizensHandler.OnInteractorAdded -= OnWorkerAdded;
        CitizensHandler.OnInteractorRemoved -= OnWorkerRemoved;
        CitizensHandler.OnCurrentInteractorAdded -= OnCurrentWorkerAdded;
        CitizensHandler.OnCurrentInteractorRemoved -= OnCurrentWorkerRemoved;

        RaidersHandler.OnInteractorAdded -= OnRaiderAdded;
        RaidersHandler.OnInteractorAdded -= OnRaiderRemoved;

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

        levelComponent.Init(buildingData.Level);
        upgradeComponent.Init(buildingData.Upgrade);
        constructionComponent.Init(buildingData.Construction);
    }

    protected virtual void OnDemolish()
    {
        RemoveWorkers();
    }

    protected abstract BuildingConstruction GetConstructionToSpawn();

    // Residents Management
    public void EnterBuilding(CreatureCityNavigator navigator)
    {
        buildingStrategy.OnEntityEnter(navigator);
        OnEnteredBuilding?.Invoke(navigator);
    }

    public void ExitBuilding(CreatureCityNavigator navigator)
    {
        buildingStrategy.OnEntityExit(navigator);
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
        return LevelDefinition.ResourcesToBuild;
    }

    public ItemInstance[] GetResourcesToRefund()
    {
        int count = LevelDefinition.ResourcesToBuild.Length;
        var resources = new ItemInstance[count];

        for (int i = 0; i < count; i++) {
            var resource = LevelDefinition.ResourcesToBuild[i];
            var definition = resource.Definition;
            int amount = (int)(resource.Amount * DemolishionResourcesRefundPercent);

            var item = definition.CreateInstance();
            item.SetAmount(amount);

            resources[i] = item;
        }

        return resources;
    }

    public List<ItemInstance> GetRaidResources()
    {
        var raidables = GetComponents<IRaidable>();
        if (raidables == null) return null;

        var resources = new List<ItemInstance>();
        foreach (var raidable in raidables) {
            var loot = raidable.GetRaidLoot();
            if (loot == null) continue;

            resources.AddRange(loot);
        }

        return resources;
    }

    public int GetUpgradeTime()
    {
        if (NextLevelDefinition == null) return 0;

        return NextLevelDefinition.UpgradeTime;
    }

    // Localization
    public Dictionary<string, string> GetLocalization()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var startTime = constructionComponent.ConstructionStartTime;
        var finishTime = constructionComponent.ConstructionFinishTime;
        var currentConstructionTime = startTime != null ? currentTime - startTime : 0;
        var ConstructionTime = startTime != null && finishTime != null ? constructionComponent.ConstructionFinishTime - constructionComponent.ConstructionStartTime : 0;

        return new Dictionary<string, string>()
        {
            { "name", LocalizationManager.Instance.GetLocalizedText(buildingData.NameLocalizationItem) },
            { "currentLevel", levelComponent.Level.ToString() },
            { "nextLevel", (levelComponent.Level + 1).ToString() },
            { "constructionTime", TimeFormatter.SecondsToTimer((int)currentConstructionTime).ToString() + "/" + TimeFormatter.SecondsToTimer((int)ConstructionTime).ToString() },
        };
    }

    // Information
    public LocalizationItem GetInformationName()
    {
        if (Definition == null) return null;

        return Definition.NameLocalizationItem;
    }

    public LocalizationItem GetInformationDescription()
    {
        if (Definition == null) return null;

        return Definition.DescriptionLocalizationItem;
    }

    public Sprite GetInformationImage()
    {
        if (LevelDefinition == null) return null;

        return LevelDefinition.BuildingThumb;
    }

    // Construction
    protected virtual void OnConstructionStart()
    {
        if (SpawnedConstruction == null) return;

        //SpawnedConstruction.UpdateInteractTransforms();
    }

    protected virtual void HandleConstructionRefresh()
    {
        if (SpawnedConstruction == null) return;

        //SpawnedConstruction.UpdateInteractTransforms();
    }

    protected virtual void OnLevelChange()
    {

    }

    // Work
    public void RemoveWorkers()
    {
        for (int i = citizensHandler.CurrentInteractors.Count - 1; i >= 0; i--) {
            var worker = citizensHandler.CurrentInteractors[i];
            if (worker == null) continue;

            var building = worker.InteractComponent.InteractBuilding;
            worker.InteractComponent.RemoveInteractBuilding();
            worker.InteractComponent.TryStopInteracting(building);
        }

        for (int i = citizensHandler.Interactors.Count - 1; i >= 0; i--) {
            var worker = citizensHandler.Interactors[i];
            if (worker == null) continue;

            var building = worker.InteractComponent.InteractBuilding;
            worker.InteractComponent.RemoveInteractBuilding();
            worker.InteractComponent.TryStopInteracting(building);
        }
    }

    public int GetInteractIndex(Human human)
    {
        if (human as Citizen != null) {
            return citizensHandler.Interactors.Count;
        }
        else if (human as Raider != null) {
            return raidersHandler.Interactors.Count;
        }

        return 0;
    }

    public BuildingAction GetInteractPoint(int index)
    {
        if (SpawnedConstruction == null) {
            Debug.LogError($"[{nameof(Building)}] Spawned Construction is not valid at {this}!");
            return null;
        }

        return SpawnedConstruction.InteractionPointsHandler.GetInteractPoint(index);
    }

    public BuildingAction GetInteractPoint(CreatureInteractComponent interactor)
    {
        if (interactor == null) {
            Debug.LogError($"[{nameof(Building)}] Interactor is not valid!");
            return null;
        }
        if (buildingStrategy == null) {
            Debug.LogError($"[{nameof(Building)}] Building Strategy is not valid!");
            return null;
        }

        return buildingStrategy.GetInteractPoint(interactor);
    }

    private void OnWorkerAdded(Human human)
    {
        //UpdateWorkerInteractionTransforms();

        buildingStrategy.OnInteractBuildingSet(human.InteractComponent);
    }

    private void OnWorkerRemoved(Human human)
    {
        //UpdateWorkerInteractionTransforms();

        buildingStrategy.OnInteractBuildingRemove(human.InteractComponent);
    }

    private void OnCurrentWorkerAdded(Human human)
    {
        if (CitizensHandler.CurrentInteractors.Count == 1)
            StartWorking();

        buildingStrategy.OnStartedInteracting(human.InteractComponent);
    }

    private void OnCurrentWorkerRemoved(Human human)
    {
        if (CitizensHandler.CurrentInteractors.Count == 0)
            StopWorking();

        buildingStrategy.OnStoppedInteracting(human.InteractComponent);
    }

    // Raid
    public bool CanBeRaided()
    {
        if (Definition == null) {
            Debug.LogError($"[{nameof(Building)}] Definition is not valid at {name}");
            return false;
        }

        if (!Definition.IsRaidable) return false;
        if (constructionComponent.IsUnderConstruction) return false;

        return true;
    }

    private void OnRaiderAdded(Human human)
    {
        //UpdateRaiderInteractionTransforms();
    }

    private void OnRaiderRemoved(Human human)
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
                buildingStrategy = new WorkBuildingStrategy(this);
                break;
            case BuildingStrategyEnum.Pier:
                buildingStrategy = new PierBuildingStrategy(this);
                break;
        }
    }

    private void HandleConstructionStarted()
    {
        RunRefreshConstructionCoroutine();
        OnConstructionStarted?.Invoke();
        OnBuildingConstructionStarted?.Invoke(this);
    }

    private void HandleConstructionFinished()
    {
        RunRefreshConstructionCoroutine();
        OnConstructionFinished?.Invoke();
        OnBuildingConstructionFinished?.Invoke(this);
    }

    private void HandleUpgradeStarted()
    {
        RunRefreshConstructionCoroutine();
        OnUpgradeStarted?.Invoke();
        OnBuildingUpgradeStarted?.Invoke(this);
    }

    private void HandleUpgradeFinished()
    {
        RunRefreshConstructionCoroutine();
        OnUpgradeFinished?.Invoke();
        OnBuildingUpgradeFinished?.Invoke(this);
    }

    private void HandleLevelChanged()
    {
        OnLevelChange();

        OnLevelChanged?.Invoke();
        OnBuildingLevelChanged?.Invoke(this);
    }

    // Energy
    public float GetElectricityConsumption()
    {
        if (LevelDefinition == null) return 0;

        return LevelDefinition.EnergyConsumption;
    }

    // Audio
    private void PlayWorkSound()
    {
        if (workAudioSource == null) return;

        workAudioSource.Play();
    }

    private void StopWorkSound()
    {
        if (workAudioSource == null) return;

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

    // UpdateConstruction
    protected void RunUpdateConstructionCoroutine()
    {
        if (updateConstructionCoroutine == null) {
            updateConstructionCoroutine = StartCoroutine(UpdateConstructionCoroutine());
        }
    }

    private void UpdateConstruction()
    {
        var constructionToSpawn = GetConstructionToSpawn();
        if (constructionToSpawn == null && buildingData.BuildingId != BuildingIdEnum.FloorFrame) {
            Debug.LogError($"[{nameof(Building)}] Construction To Spawn is not valid at {name}");
            return;
        }

        if (constructionToSpawn == SpawnedConstruction) return;

        if (SpawnedConstruction != null) {
            Destroy(SpawnedConstruction.gameObject);
            SpawnedConstruction = null;
        }

        var data = new BuildingConstructionData()
        {
            OwnedBuildingInstanceId = instanceId.GetGuid()
        };

        SpawnedConstruction = ConstructionFactory.CreateConstruction(constructionToSpawn, transform, data);
        OnConstructionChanged?.Invoke(SpawnedConstruction);
    }

    private IEnumerator UpdateConstructionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        updateConstructionCoroutine = null;
        UpdateConstruction();
    }

    // Refresh Construction
    protected void RunRefreshConstructionCoroutine()
    {
        if (refreshConstructionCoroutine == null) {
            refreshConstructionCoroutine = StartCoroutine(RefreshConstructionCoroutine());
        }
    }

    private void RefreshConstruction()
    {
        UpdateConstruction();
        HandleConstructionRefresh();

        if (SelectComponent.IsSelected) {
            SelectComponent.Select();
        }
    }

    private IEnumerator RefreshConstructionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        refreshConstructionCoroutine = null;
        RefreshConstruction();
    }
}