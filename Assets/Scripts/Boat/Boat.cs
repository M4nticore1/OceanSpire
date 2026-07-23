using System;
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

    public BoatRider CurrentRider;
    public BoatRider TargetRider;

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

    // Weight
    public float CurrentWeight => inventory.CurrentWeight;
    public float MaxWeight => inventory.WeightLimit;

    [Header("Other")]
    [SerializeField] private int findLootMaxWeightThreshold = 5;
    public int FindLootMaxWeightThreshold => findLootMaxWeightThreshold;

    [SerializeField] private Transform seatSlot;
    public Transform SeatSlot => seatSlot;

    [SerializeField] private bool isClickable = true;
    public bool IsClickable => isClickable;

    public bool SentToPier { get; private set; } = false;

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

    private void OnEnable()
    {
        movement.OnMovementStarted += OnMovementStarted;
        movement.OnReachedDestination += OnReachedPath;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;

        BoatsManager.Instance.RegisterBoat(this);
    }

    private void OnDisable()
    {
        movement.OnMovementStarted -= OnMovementStarted;
        movement.OnReachedDestination -= OnReachedPath;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

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

        instanceId.SetGuid(boatData.InstanceId);
        CurrentStatus = boatData.Status;
        inventory.Init(boatData.InventoryData);

        if (boatData.DockInstanceId != null) {
            var boatDockInstance = InstancesManager.Instance.GetInstance(boatData.DockInstanceId.Value);

            if (boatDockInstance) {
                var boatDock = boatDockInstance.GetComponent<BoatDockPoint>();

                if (boatDock)
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
    }

    public void OnReturnedToDock()
    {
        if (Inventory.CurrentWeight > 0) {
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
        if (!rider) {
            Debug.LogError("rider is not valid", this);
            return;
        }

        CurrentRider = rider;

        if (movement.IsMoving)
            CurrentRider.HandleBoatMovementStarted();
        else
            CurrentRider.HandleBoatMovementStopped();

        UpdateClickable();
        UpdateContextMenuTarget();
        UpdateState();

        OnRiderAdded?.Invoke(rider);
    }

    public void RemoveCurrentRider()
    {
        var lastRider = CurrentRider;
        CurrentRider = null;

        UpdateClickable();
        UpdateContextMenuTarget();

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
        if (!dockPoint) {
            Debug.Log($"[{nameof(Boat)}] DockPoint is not valid. Use RemoveDockPoint method to remove it insteod of this.");
            return;
        }

        if (dockPoint == DockPoint) return;

        DockPoint = dockPoint;
        dockPoint.AddBoat(this);

        if (CurrentState != null) {
            CurrentState.OnBoatDockChanged(dockPoint);
        }
    }

    public void RemoveDockPoint()
    {
        if (!DockPoint) return;

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

        switch (CurrentStatus) {
            case HumanStatusEnum.Citizen:
                index = BoatsManager.Instance.CitizenBoats.ToList().IndexOf(this);
                SetDockPoint(BoatDocksManager.Instance.CitizenBoatDocks[index]);
                break;
            case HumanStatusEnum.Wanderer:
                index = BoatsManager.Instance.WandererBoats.ToList().IndexOf(this);
                SetDockPoint(BoatDocksManager.Instance.WandererDockPoints[index]);
                break;
            case HumanStatusEnum.Raider:
                SetDockPoint(BoatDockUtils.GetNearestFreeDockPoint(BoatDocksManager.Instance.RaiderDockPoints, transform.position));
                break;
        }
    }

    // Target Loot
    public bool TrySetTargetLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!ShouldSetTargetLoot(driftingLoot)) return false;

        if (TargetDriftingLoot) {
            RemoveTargetLoot();
        }

        TargetDriftingLoot = driftingLoot;
        driftingLoot.SetTargetBoat(this);

        UpdateState();
        return true;
    }

    public void RemoveTargetLoot()
    {
        if (!TargetDriftingLoot) return;

        var lastLoot = TargetDriftingLoot;
        TargetDriftingLoot = null;
        lastLoot.RemoveTargetBoat(this);
    }

    public bool ShouldSetTargetLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!driftingLoot) return false;
        //if (driftingLoot == TargetDriftingLoot) return false;

        var targetBoat = driftingLoot.TargetBoat;
        if (targetBoat && targetBoat != this) return false;

        //if (TargetDriftingLoot && TargetDriftingLoot.FocusComponent.IsFocused && !driftingLoot.FocusComponent.IsFocused) return false;

        var swimmingDefinition = driftingLoot.Definition as SwimmingDriftingLootDefinition;
        if (!swimmingDefinition) return false;

        if (!movement.CanReachPosition(driftingLoot.transform.position)) return false;

        foreach (var item in swimmingDefinition.LootTable) {
            if (item.itemData.Weight < Inventory.RemainingWeight) return true;
        }

        return false;
    }

    // State
    public void UpdateState()
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
        if (CurrentStateEnum == BoatStateEnum.CollectingLoot) return false;
        if (CurrentStateEnum == BoatStateEnum.UnloadingLoot) return false;
        if (CurrentState as FindingLootBoatState != null) return false;

        if (!CurrentRider) return false;

        var targetBoat = CurrentRider.TargetBoat;
        if (targetBoat && targetBoat != this) return false;

        var citizen = CurrentRider.GetComponent<Citizen>();
        if (!citizen) return false;

        if (citizen.IsEvicted) return false;
        if (!citizen.HealthComponent.IsAlive) return false;

        var interactBuilding = citizen.InteractComponent.InteractBuilding;
        if (!interactBuilding) return false;

        var pier = interactBuilding.GetComponent<PierModule>();
        if (!pier) return false;

        if (IsOverweight()) return false;

        return true;
    }

    public bool ShouldCollectLoot()
    {
        return false;
    }

    public bool ShouldMovingToLoot()
    {
        if (!TargetDriftingLoot) return false;
        if (!CurrentRider) return false;
        if (CurrentStateEnum == BoatStateEnum.CollectingLoot) return false;
        if (CurrentStateEnum == BoatStateEnum.UnloadingLoot) return false;

        return true;
    }

    public bool ShouldMovingToDock()
    {
        return false;
    }

    public bool ShouldUnloadingLoot()
    {
        if (inventory.RemainingWeightInt <= 0) return false;
        if (CurrentStateEnum != BoatStateEnum.UnloadingLoot) return false;

        return true;
    }

    public bool ShouldFloatAway()
    {
        if (!CurrentRider) return false;

        return false;
    }

    public bool IsOverweight()
    {
        return inventory.RemainingWeightInt <= findLootMaxWeightThreshold;
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
    private void OnMovementStarted()
    {
        if (!CurrentRider) return;

        CurrentRider.HandleBoatMovementStarted();
    }

    private void OnReachedPath()
    {
        CurrentState.OnReachedPath();

        if (CurrentRider) {
            CurrentRider.HandleBoatMovementStopped();
        }
    }

    // Clickable
    private void OnSelected()
    {
        OnBoatSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        OnBoatDeselected?.Invoke(this);
    }

    private void UpdateClickable()
    {
        if (CurrentStatus == HumanStatusEnum.Citizen) {
            var targetCitizen = TargetRider?.GetComponent<Citizen>();
            var ridingCitizen = CurrentRider?.GetComponent<Citizen>();
            SetClickable(CurrentStatus == HumanStatusEnum.Citizen && targetCitizen ? !targetCitizen.IsEvicted : true && ridingCitizen ? !ridingCitizen.IsEvicted : true);
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
        var targetCitizen = TargetRider?.GetComponent<Citizen>();
        var ridingCitizen = CurrentRider?.GetComponent<Citizen>();
        contextMenuTarget.SetShowContextMenu(CurrentStatus == HumanStatusEnum.Citizen && targetCitizen ? !targetCitizen.IsEvicted : true && ridingCitizen ? !ridingCitizen.IsEvicted : true);
    }
}