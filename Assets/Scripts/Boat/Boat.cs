using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum BoatStateEnum
{
    Idle,
    FindingLoot,
    MovingToLoot,
    CollectingLoot,
    MovingToDock,
    UnloadingLoot,
    FloatingAway,
    Demolished
}

public class Boat : MonoBehaviour, IClickable, ILocalizable
{
    [Header("Main")]
    [SerializeField] private BoatDefinition boatData;
    public BoatDefinition Definition => boatData;

    [field: SerializeField] public BoatStateEnum CurrentStateEnum { get; private set; }
    public BoatState CurrentState { get; private set; }

    [field: SerializeField] public HumanStatusEnum CurrentStatus { get; private set; }

    [field: SerializeField] public BoatRider CurrentRider { get; private set; }
    [field: SerializeField] public BoatRider TargetRider { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Movement movement;
    public Movement Movement => movement;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    [SerializeField] private ContextMenuTarget contextMenuTarget;
    public ContextMenuTarget ContextMenuTarget => contextMenuTarget;

    [SerializeField] private BoatShaker boatShake;

    // Dock
    [field: SerializeField] public BoatDockPoint DockPoint { get; private set; }
    [field: SerializeField] public SwimmingDriftingLoot TargetDriftingLoot { get; private set; }

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    public Canvas Canvas => canvas;

    [SerializeField] private HarvestEffectPositionHandler unloadingEffectPositionHandler;
    public HarvestEffectPositionHandler UnloadingEffectPositionHandler => unloadingEffectPositionHandler;

    [SerializeField] private HarvestEffectPositionHandler collectingEffectPositionHandler;
    public HarvestEffectPositionHandler CollectingEffectPositionHandler => collectingEffectPositionHandler;

    [Header("Other")]
    [SerializeField] private int findLootMaxWeightThreshold = 5;
    public int FindLootMaxWeightThreshold => findLootMaxWeightThreshold;

    [SerializeField] private Transform seatSlot;
    public Transform SeatSlot => seatSlot;

    [SerializeField] private bool isClickable = true;
    public bool IsClickable => isClickable;

    private BoatsManager boatsManager => BoatsManager.Instance;
    private BoatDocksManager boatDocksManager => BoatDocksManager.Instance;

    private Coroutine updateStateCoroutine;

    public event Action OnClicked;

    public event Action<BoatRider> OnRiderAdded;
    public event Action<BoatRider> OnRiderRemoved;

    public event Action<BoatState> OnStateEntered;
    public event Action<BoatState> OnStateExited;

    public static event Action<Boat> OnBoatStateEntered;
    public static event Action<Boat> OnBoatStateExited;

    public static event Action<Boat> OnBoatSelected;
    public static event Action<Boat> OnBoatDeselected;
    public static event Action<Boat> OnBoatDestroyed;

    public static event Action<Boat, ItemInstance> OnInventoryItemAmountChanged;

    private void Awake()
    {
        movement.NavAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    private void OnEnable()
    {
        movement.OnMovementStarted += HandleMovementStarted;
        movement.OnMovementStopped += HandleMovementStopped;
        movement.OnDestinationReached += HandleDestinationReached;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;

        inventory.OnItemAmountChanged += HandleInventoryItemAmountChanged;

        BoatsManager.Instance.RegisterBoat(this);
    }

    private void OnDisable()
    {
        movement.OnMovementStarted -= HandleMovementStarted;
        movement.OnMovementStopped -= HandleMovementStopped;
        movement.OnDestinationReached -= HandleDestinationReached;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

        inventory.OnItemAmountChanged -= HandleInventoryItemAmountChanged;

        BoatsManager.Instance.UnregisterBoat(this);
    }

    public void Tick()
    {
        boatShake.Tick();

        if (CurrentState == null) return;
        CurrentState.Tick();
    }

    public void Init()
    {
        Init(BoatData.Default() ?? new BoatData());
    }

    public void Init(BoatData boatData)
    {
        if (boatData == null) {
            Debug.LogError($"[{nameof(Boat)}] BoatData is not valid");
            Init();
            return;
        }

        CurrentStatus = boatData.Status;

        instanceId.SetGuid(boatData.InstanceId);
        inventory.Init(boatData.InventoryData);

        if (boatData.DockInstanceId != null) {
            var boatDockInstance = InstancesManager.Instance.GetInstance(boatData.DockInstanceId.Value);

            if (boatDockInstance != null) {
                var boatDock = boatDockInstance.GetComponent<BoatDockPoint>();

                if (boatDock != null)
                    SetDockPoint(boatDock);
                else
                    Debug.LogError($"[{nameof(Boat)}] DockPoint is not valid by instance {boatDockInstance}");
            }
        }

        FixDockPoint();
        SetState(boatData.State);

        transform.position = boatData.Position.Vector3();
        transform.rotation = Quaternion.Euler(boatData.Rotation.Vector3());

        movement.NavAgent.speed = Definition.BoatSpeed;

        UpdateClickable();
        UpdateContextMenuTarget();
        TryDestroyBoat();
        updateStateCoroutine = StartCoroutine(UpdateStateCoroutine());
    }

    public void HandleReturnedToDock()
    {
        if (Inventory.GetCurrentWeight() > 0) {
            SetState(BoatStateEnum.UnloadingLoot);
        }
        else {
            SetState(BoatStateEnum.Idle);
        }
    }

    public void FloatAway(Vector3 position)
    {
        SetState(BoatStateEnum.FloatingAway);
        movement.TryMoveTo(position);
    }

    // Enter / Exit
    public void SetCurrentRider(BoatRider rider)
    {
        if (rider == null) {
            Debug.LogError("Rider is not valid");
            return;
        }

        CurrentRider = rider;

        if (movement.IsMoving)
            CurrentRider.HandleBoatMovementStarted();
        else
            CurrentRider.HandleBoatMovementStopped();

        UpdateClickable();
        UpdateContextMenuTarget();
        RunUpdateStateCoroutine();

        OnRiderAdded?.Invoke(rider);
    }

    public void RemoveCurrentRider()
    {
        var lastRider = CurrentRider;
        CurrentRider = null;

        UpdateClickable();
        UpdateContextMenuTarget();
        RunUpdateStateCoroutine();

        OnRiderRemoved?.Invoke(lastRider);
    }

    public void SetTargetRider(BoatRider rider)
    {
        TargetRider = rider;

        UpdateClickable();
        UpdateContextMenuTarget();
    }

    public void RemoveTargetRider()
    {
        TargetRider = null;

        UpdateClickable();
        UpdateContextMenuTarget();
    }

    // Dock Point
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        if (dockPoint == null) {
            Debug.LogError($"[{nameof(Boat)}] Dock Point is not valid!");
            return;
        }

        if (dockPoint == DockPoint) return;

        if (DockPoint != null) {
            DockPoint.RemoveBoat(this);
        }

        DockPoint = dockPoint;
        dockPoint.AddBoat(this);

        if (CurrentState != null) {
            CurrentState.OnBoatDockChanged(dockPoint);
        }
    }

    public void RemoveDockPoint()
    {
        if (DockPoint == null) return;

        DockPoint.RemoveBoat(this);
        DockPoint = null;

        if (CurrentState != null) {
            CurrentState.OnBoatDockChanged(null);
        }
    }

    private void FixDockPoint()
    {
        if (DockPoint != null) return;
        if (CurrentStateEnum == BoatStateEnum.FloatingAway) return;

        int index = 0;
        BoatDockPoint dock;

        if (CurrentStatus == HumanStatusEnum.Citizen) {
            index = Array.IndexOf(boatsManager.CitizenBoats.ToArray(), this);
            dock = boatDocksManager.GetCitizenBoatDock(index);
            if (dock != null) {
                SetDockPoint(dock);
            }
        }
        else if (CurrentStatus == HumanStatusEnum.Wanderer) {
            index = Array.IndexOf(boatsManager.WandererBoats.ToArray(), this);
            dock = boatDocksManager.GetWandererBoatDock(index);
            if (dock != null) {
                SetDockPoint(dock);
            }
        }
        else if (CurrentStatus == HumanStatusEnum.Raider) {
            dock = BoatDockUtils.GetNearestFreeDockPoint(boatDocksManager.RaiderDockPoints, transform.position);
            if (dock != null) {
                SetDockPoint(dock);
            }
        }
    }

    private void TryDestroyBoat()
    {
        if (!ShouldDestroyBoat()) return;

        Destroy(gameObject);
    }

    private bool ShouldDestroyBoat()
    {
        if (CurrentStatus == HumanStatusEnum.Citizen && DockPoint == null) {
            var currentCitizen = CurrentRider != null ? CurrentRider.GetComponent<Citizen>() : null;
            if (currentCitizen == null || !currentCitizen.IsEvicted) return true;
        }

        return false;
    }

    // Target Loot
    public bool TrySetTargetLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!ShouldSetTargetLoot(driftingLoot)) return false;

        if (TargetDriftingLoot != null) {
            RemoveTargetLoot();
        }

        TargetDriftingLoot = driftingLoot;
        driftingLoot.SetTargetBoat(this);

        return true;
    }

    public void RemoveTargetLoot()
    {
        if (TargetDriftingLoot == null) return;

        var lastLoot = TargetDriftingLoot;
        TargetDriftingLoot = null;
        lastLoot.RemoveTargetBoat(this);
    }

    public bool ShouldSetTargetLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return false;

        var targetBoat = driftingLoot.TargetBoat;
        if (targetBoat != null && targetBoat != this) return false;

        var swimmingDefinition = driftingLoot.Definition as SwimmingDriftingLootDefinition;
        if (swimmingDefinition == null) return false;

        if (!movement.CanReachPosition(driftingLoot.transform.position)) return false;

        foreach (var item in swimmingDefinition.LootTable) {
            if (item.itemData.Weight < Inventory.GetRemainingWeight()) return true;
        }

        return false;
    }

    // State
    public void RunUpdateStateCoroutine()
    {
        updateStateCoroutine = StartCoroutine(UpdateStateCoroutine());
    }

    public void UpdateBoatState()
    {
        if (ShouldIdle()) {
            SetState(BoatStateEnum.Idle);
            return;
        }
        if (ShouldMovingToLoot()) {
            SetState(BoatStateEnum.MovingToLoot);
            return;
        }
        if (ShouldMovingToDock()) {
            SetState(BoatStateEnum.MovingToDock);
            return;
        }
        if (ShouldUnloadingLoot()) {
            SetState(BoatStateEnum.UnloadingLoot);
            return;
        }
        if (ShouldFloatAway()) {
            SetState(BoatStateEnum.FloatingAway);
            return;
        }
        if (ShouldFindLoot()) {
            SetState(BoatStateEnum.FindingLoot);
            return;
        }
    }

    public void SetState(BoatStateEnum state)
    {
        if (CurrentState != null) {
            if (state == CurrentStateEnum) return;

            CurrentState.Exit();

            OnStateExited?.Invoke(CurrentState);
            OnBoatStateExited?.Invoke(this);
        }

        switch (state) {
            case BoatStateEnum.Idle:
                CurrentState = new IdleBoatState(this);
                break;
            case BoatStateEnum.FindingLoot:
                CurrentState = new FindingLootBoatState(this);
                break;
            case BoatStateEnum.MovingToLoot:
                CurrentState = new MovingToLootBoatState(this);
                break;
            case BoatStateEnum.CollectingLoot:
                CurrentState = new CollectingLootBoatState(this);
                break;
            case BoatStateEnum.MovingToDock:
                CurrentState = new MovingToDockBoatState(this);
                break;
            case BoatStateEnum.UnloadingLoot:
                CurrentState = new UnloadingLootBoatState(this);
                break;
            case BoatStateEnum.FloatingAway:
                CurrentState = new FloatingAwayBoatState(this);
                break;
        }

        CurrentStateEnum = state;
        CurrentState.Enter();

        OnStateEntered?.Invoke(CurrentState);
        OnBoatStateEntered?.Invoke(this);
    }

    public bool ShouldIdle()
    {
        return false;
    }

    public bool ShouldFindLoot()
    {
        if (CurrentStateEnum == BoatStateEnum.CollectingLoot && TargetDriftingLoot != null) return false;
        if (CurrentStateEnum == BoatStateEnum.UnloadingLoot && inventory.Items.Count > 0) return false;
        if (CurrentState as FindingLootBoatState != null) return false;

        if (CurrentRider == null) return false;

        var targetBoat = CurrentRider.TargetBoat;
        if (targetBoat != null && targetBoat != this) return false;

        var citizen = CurrentRider.GetComponent<Citizen>();
        if (citizen == null) return false;
        if (!citizen.IsCitizenAvaliable()) return false;

        var interactBuilding = citizen.InteractComponent.InteractBuilding;
        if (interactBuilding == null) return false;

        var pier = interactBuilding.GetComponent<PierModule>();
        if (pier == null) return false;

        if (IsOverweight()) return false;

        return true;
    }

    public bool ShouldCollectLoot()
    {
        return false;
    }

    public bool ShouldMovingToLoot()
    {
        if (TargetDriftingLoot == null) return false;
        if (CurrentRider == null) return false;
        if (CurrentStateEnum == BoatStateEnum.CollectingLoot) return false;
        if (CurrentStateEnum == BoatStateEnum.UnloadingLoot) return false;

        return true;
    }

    public bool ShouldMovingToDock()
    {
        if (CurrentRider == null && CurrentStateEnum != BoatStateEnum.Idle) return true;

        return false;
    }

    public bool ShouldUnloadingLoot()
    {
        if (inventory.GetRemainingWeightInt() <= 0) return false;
        if (CurrentStateEnum != BoatStateEnum.UnloadingLoot) return false;

        return true;
    }

    public bool ShouldFloatAway()
    {
        if (CurrentRider == null) return false;

        return false;
    }

    public bool IsOverweight()
    {
        return inventory.GetRemainingWeightInt() <= findLootMaxWeightThreshold;
    }

    // IClickable
    public void Click()
    {
        selectComponent.Click();
        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    public bool ShouldClick()
    {
        if (!IsClickable) return false;
        if (CurrentStatus == HumanStatusEnum.Raider) return false;
        if (CurrentStatus == HumanStatusEnum.Wanderer && movement.IsMoving) return false;

        return true;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "name", LocalizationManager.Instance.GetLocalizedText(Definition.NameLocalization) }
        };
    }

    // Movement
    private void HandleMovementStarted()
    {
        if (CurrentRider != null) {
            CurrentRider.HandleBoatMovementStarted();
        }
    }

    private void HandleMovementStopped()
    {
        if (CurrentRider != null) {
            CurrentRider.HandleBoatMovementStopped();
        }
    }

    private void HandleDestinationReached()
    {
        CurrentState.OnReachedPath();
    }

    // Select
    private void OnSelected()
    {
        OnBoatSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        OnBoatDeselected?.Invoke(this);
    }

    // Clickable
    private void UpdateClickable()
    {
        if (CurrentStatus == HumanStatusEnum.Citizen) {
            var targetCitizen = TargetRider != null ? TargetRider.GetComponent<Citizen>() : null;
            var ridingCitizen = CurrentRider != null ? CurrentRider.GetComponent<Citizen>() : null;

            bool targetCondition = targetCitizen != null ? !targetCitizen.IsEvicted : true;
            bool ridingCondition = ridingCitizen != null ? !ridingCitizen.IsEvicted : true;

            SetClickable(targetCondition && ridingCondition);
        }
        else if (CurrentStatus == HumanStatusEnum.Wanderer) {
            SetClickable(!movement.IsMoving);
        }
        else if (CurrentStatus == HumanStatusEnum.Raider) {
            SetClickable(false);
        }
    }

    private void UpdateContextMenuTarget()
    {
        var targetCitizen = TargetRider != null ? TargetRider.GetComponent<Citizen>() : null;
        var ridingCitizen = CurrentRider != null ? CurrentRider.GetComponent<Citizen>() : null;

        bool targetCondition = targetCitizen != null ? !targetCitizen.IsEvicted : true;
        bool ridingCondition = ridingCitizen != null ? !ridingCitizen.IsEvicted : true;

        contextMenuTarget.SetShowContextMenu(CurrentStatus == HumanStatusEnum.Citizen && targetCondition && ridingCondition);
    }

    // Inventory
    private void HandleInventoryItemAmountChanged(ItemInstance item)
    {
        OnInventoryItemAmountChanged?.Invoke(this, item);
    }

    private IEnumerator UpdateStateCoroutine()
    {
        if (updateStateCoroutine != null) yield break;
        yield return new WaitForEndOfFrame();

        UpdateBoatState();
        updateStateCoroutine = null;
    }
}