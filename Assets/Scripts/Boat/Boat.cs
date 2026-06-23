using System;
using UnityEngine;

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

public class Boat : MonoBehaviour, IClickable
{
    [SerializeField] private BoatDefinition boatData;
    public BoatDefinition Definition => boatData;

    public BoatStateEnum CurrentStateEnum { get; private set; }
    public BoatState CurrentState { get; private set; }

    public HumanStatusEnum CurrentStatus { get; private set; }

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
    public BoatDockPoint DockPoint;
    public SwimmingDriftingLoot TargetDriftingLoot { get; private set; }

    // Weight
    public float CurrentWeight => inventory.CurrentWeight;
    public float MaxWeight => inventory.WeightLimit;

    [SerializeField] private Transform seatSlot;
    public Transform SeatSlot => seatSlot;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    public event Action<BoatRider> OnRiderAdded;
    public event Action<BoatRider> OnRiderRemoved;

    public event Action<BoatState> OnStateEntered;
    public event Action<BoatState> OnStateExited;

    public static event Action<Boat> OnBoatSelected;
    public static event Action<Boat> OnBoatDeselected;
    public static event Action<Boat> OnBoatDestroyed;

    private void OnEnable()
    {
        movement.OnMovementStarted += OnMovementStarted;
        movement.OnReachedDestination += OnReachedPath;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;
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

    public void Init(BoatData boatData)
    {
        if (boatData == null) {
            Debug.LogError("boatData is not valid");
        }

        CurrentStatus = boatData.Status;

        instanceId.Register(boatData.InstanceId);
        BoatsManager.Instance.RegisterBoat(this);

        var state = (BoatStateEnum)Enum.GetValues(typeof(BoatStateEnum)).GetValue(boatData.StateId);
        SetState(state);

        transform.position = boatData.Position.Vector3();
        transform.rotation = Quaternion.Euler(boatData.Rotation.Vector3());

        if (boatData.DockInstanceId != null) {
            var boatDockInstance = InstancesManager.Instance.GetInstance(boatData.DockInstanceId.Value);

            if (boatDockInstance) {
                var boatDock = boatDockInstance.GetComponent<BoatDockPoint>();

                if (boatDock)
                    SetDockPoint(boatDock);
                else
                    Debug.LogError($"dockPoint is not valid by instance {boatDockInstance}");
            }
        }

        movement.NavAgent.speed = Definition.BoatSpeed;
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

        OnRiderAdded?.Invoke(rider);
    }

    public void RemoveCurrentRider()
    {
        var lastRider = CurrentRider;
        CurrentRider = null;
        OnRiderRemoved?.Invoke(lastRider);
    }

    public void SetTargetRider(BoatRider rider)
    {
        TargetRider = rider;
    }

    public void RemoveTargetRider()
    {
        TargetRider = null;
    }

    // Dock Point
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        if (!dockPoint) {
            Debug.Log($"DockPoint is not valid. Use RemoveDockPoint method to remove it insteod of this.");
            return;
        }

        if (dockPoint == DockPoint) return;

        DockPoint = dockPoint;
        dockPoint.AddBoat(this);
        CurrentState.OnBoatDockChanged(dockPoint);
    }

    public void RemoveDockPoint()
    {
        if (!DockPoint) {
            Debug.Log($"DockPoint is already null at {name}");
            return;
        }

        DockPoint.RemoveBoat(this);
        DockPoint = null;
        CurrentState.OnBoatDockChanged(null);
    }

    public void SetTargetLoot(SwimmingDriftingLoot driftingLoot)
    {
        TargetDriftingLoot = driftingLoot;
    }

    public ItemInstance TryGetItemToUnload()
    {
        return inventory.TryGetItemByIndex(0);
    }

    // State
    public void SetState(BoatStateEnum state)
    {
        if (CurrentState != null && state == CurrentStateEnum) return;

        if (CurrentState != null) {
            CurrentState.Exit();
            OnStateExited?.Invoke(CurrentState);
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
            case BoatStateEnum.Demolished:
                CurrentState = new DemolishBoatState(this);
                break;
        }

        CurrentStateEnum = state;
        CurrentState.Enter();

        OnStateEntered?.Invoke(CurrentState);
    }

    public bool ShouldFindLoot()
    {
        if (!CurrentRider) return false;
        if (!CurrentRider.GetComponent<Citizen>()) return false;
        if (inventory.RemainingWeight <= 0) return false;

        return true;
    }

    // IClickable
    public void Click()
    {
        selectComponent.Click();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
        OnClicked?.Invoke();
    }

    public bool ShouldClick()
    {
        if (!IsClickable) return false;
        if (CurrentStatus == HumanStatusEnum.Raider) return false;
        if (CurrentStatus == HumanStatusEnum.Wanderer && movement.IsMoving) return false;

        return true;
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

        if (!CurrentRider) return;

        CurrentRider.HandleBoatMovementStopped();
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
}