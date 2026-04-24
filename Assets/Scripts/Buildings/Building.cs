using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingData
{
    public int Id { get; private set; }
    public int InstanceId { get; private set; }
    public int Level { get; private set; }
    public ConstructionData ConstructionData { get; private set; }

    public BuildingData(int id, int instanceId, int level, ConstructionData constructionData)
    {
        Id = id;
        InstanceId = instanceId;
        Level = level;
        ConstructionData = constructionData;
    }
}

public abstract class Building : MonoBehaviour, ILocalizable
{
    [SerializeField] protected ConstructionComponent constructionComponent;
    public ConstructionComponent ConstructionComponent => constructionComponent;

    [SerializeField] protected LevelComponent levelComponent;
    public LevelComponent LevelComponent => levelComponent;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    public SelectComponent SelectComponent { get; private set; }

    [Header("Audio")]
    [SerializeField] protected AudioSource workAudioSource;

    private BuildingStrategy strategy;

    private bool isWorking = false;

    public List<CreatureCityNavigator> enteredEntities { get; private set; } = new List<CreatureCityNavigator>();
    public List<InteractComponent> workers { get; private set; } = new List<InteractComponent>();
    public List<InteractComponent> currentWorkers { get; private set; } = new List<InteractComponent>();

    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    [Header("Data")]
    [SerializeField] protected BuildingDefinition buildingData = null;
    public BuildingDefinition BuildingData => buildingData;
    [SerializeField] protected List<BuildingLevelData> buildingLevelsData = new List<BuildingLevelData>();
    public List<BuildingLevelData> LevelsData => buildingLevelsData;
    public BuildingLevelData LevelData => LevelsData.Count > levelComponent.level - 1 ? LevelsData[levelComponent.level - 1] : null;
    public BuildingLevelData NextLevelData => LevelsData.Count > levelComponent.level ? LevelsData[levelComponent.level] : null;
    [SerializeField] private bool isRuined = false;
    public bool IsRuined => isRuined;

    public const float DemolishionResourcesRefundPercent = 0.2f;

    public event Action onInited;
    public event Action onStartWorking;
    public event Action onStopWorking;

    public static event Action<Building> onBuildingInited;
    public static event Action<Building> onBuildingDemolished;

    public event Action onConstructionStarted;
    public event Action onConstructionFinished;

    public static event Action<Building> onBuildingConstructionStarted;
    public static event Action<Building> onBuildingConstructionFinished;

    public event Action<CreatureCityNavigator> onEnterBuilding;
    public event Action<CreatureCityNavigator> onExitBuilding;

    public static event Action<Building> onBuildingSelected;
    public static event Action<Building> onBuildingDeselected;

    private void Awake()
    {
        SelectComponent = GetComponent<SelectComponent>();
    }

    protected virtual void OnEnable()
    {
        EventBus.onClickedContextDemolishButton += OnDemolishClicked;
        EventBus.onClickedContextUpgradeButton += OnUpgradeClicked;

        constructionComponent.onConstructionStarted += OnConstructionStarted;
        constructionComponent.onConstructionFinished += OnConstructionFinished;

        SelectComponent.onSelected += OnSelected;
        SelectComponent.onDeselected += OnDeselected;
    }

    protected virtual void OnDisable()
    {
        EventBus.onClickedContextDemolishButton -= OnDemolishClicked;
        EventBus.onClickedContextUpgradeButton -= OnUpgradeClicked;

        constructionComponent.onConstructionStarted -= OnConstructionStarted;
        constructionComponent.onConstructionFinished -= OnConstructionFinished;

        SelectComponent.onSelected -= OnSelected;
        SelectComponent.onDeselected -= OnDeselected;
    }

    // Constructing
    public void Init(BuildingData data)
    {
        AsssignStrategy();
        constructionComponent.Init(data.ConstructionData);

        OnInit(data);
        UpdateConstruction();

        onInited?.Invoke();
        onBuildingInited?.Invoke(this);

        InvokeBuildingInited();
    }

    public void Demolish()
    {
        Destroy(gameObject);
        onBuildingDemolished?.Invoke(this);

        InvokeBuildingDemolished();
    }

    protected abstract void OnInit(BuildingData saveData);

    protected abstract BuildingConstruction GetConstructionToSpawn();

    protected virtual void InvokeBuildingInited()
    {
        foreach (var module in GetComponents<IOwnedBuildingListener>()) {
            module.OnOwnedBuildingInited();
        }
    }

    protected virtual void InvokeBuildingDemolished()
    {
        foreach (var module in GetComponents<IOwnedBuildingListener>()) {
            module.OnOwnedBuildingDemolished();
        }
    }

    // Residents Management
    public void EnterBuilding(CreatureCityNavigator navigator)
    {
        enteredEntities.Add(navigator);
        onEnterBuilding?.Invoke(navigator);
        strategy.OnEntityEnter(navigator);
        InvokeEnterBuilding(navigator);
    }

    public void ExitBuilding(CreatureCityNavigator navigator)
    {
        enteredEntities.Remove(navigator);
        onExitBuilding?.Invoke(navigator);
        strategy.OnEntityExit(navigator);
        InvokeExitBuilding(navigator);
    }

    // Workers
    public void AddWorker(InteractComponent interactor)
    {
        workers.Add(interactor);
        strategy.OnSetInteractBuilding(interactor);
    }

    public void RemoveWorker(InteractComponent interactor)
    {
        workers.Remove(interactor);
        strategy.OnRemoveInteractBuilding(interactor);
    }

    public void AddCurrentWorker(InteractComponent interactor)
    {
        currentWorkers.Add(interactor);

        if (currentWorkers.Count == 1)
            StartWorking();

        strategy.OnStartInteracting(interactor);
        InvokeCurrentWorkerAdded(interactor);
    }

    public void RemoveCurrentWorker(InteractComponent interactor)
    {
        currentWorkers.Remove(interactor);

        if (currentWorkers.Count == 0)
            StopWorking();

        strategy.OnStopInteracting(interactor);
        InvokeCurrentWorkerRemoved(interactor);
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

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "level", levelComponent.level.ToString() },
            { "constructionTime", TimeFormatter.SecondsToMinuteTime((int)constructionComponent.CurrentConstructionTime).ToString() + "/" + TimeFormatter.SecondsToMinuteTime((int)constructionComponent.ConstructionTime).ToString() },
        };
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
    }

    // Events
    private void InvokeEnterBuilding(CreatureCityNavigator navigator)
    {
        foreach (var listener in GetComponentsInChildren<IEnterExitListener>()) {
            listener.OnEnterBuilding(navigator);
        }
    }

    private void InvokeExitBuilding(CreatureCityNavigator navigator)
    {
        foreach (var listener in GetComponentsInChildren<IEnterExitListener>()) {
            listener.OnExitBuilding(navigator);
        }
    }

    private void InvokeCurrentWorkerAdded(InteractComponent interactor)
    {
        foreach (var listener in GetComponentsInChildren<ICurrentWorkersListener>()) {
            listener.OnCurrentWorkerAdded(interactor);
        }
    }

    private void InvokeCurrentWorkerRemoved(InteractComponent interactor)
    {
        foreach (var listener in GetComponentsInChildren<ICurrentWorkersListener>()) {
            listener.OnCurrentWorkerRemoved(interactor);
        }
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
        onStartWorking?.Invoke();
    }

    private void StopWorking()
    {
        if (!isWorking) {
            Debug.Log("Building is already not working");
            return;
        }

        StopWorkSound();

        isWorking = false;
        onStopWorking?.Invoke();
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

    // Construction
    private void OnConstructionStarted()
    {
        UpdateConstruction();

        onBuildingConstructionStarted?.Invoke(this);
        onConstructionStarted?.Invoke();
    }

    private void OnConstructionFinished()
    {
        UpdateConstruction();

        if (SelectComponent.isSelected) {
            SelectComponent.Select();
        }

        onBuildingConstructionFinished?.Invoke(this);
        onConstructionFinished?.Invoke();
    }

    // Events
    private void OnDemolishClicked()
    {
        
    }

    private void OnUpgradeClicked()
    {

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