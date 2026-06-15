using System;
using System.Collections;
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
    private BoatState currentState;

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

    public static event Action<Boat> OnBoatSelected;
    public static event Action<Boat> OnBoatDeselected;
    public static event Action<Boat> onBoatDestroyed;

    private void OnEnable()
    {
        movement.OnMovementStarted += OnMovementStarted;
        movement.OnReachedPath += OnMovementStopped;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;
    }

    private void OnDisable()
    {
        movement.OnMovementStarted -= OnMovementStarted;
        movement.OnReachedPath -= OnMovementStopped;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

        BoatsManager.Instance.UnregisterBoat(this);
    }

    public void Tick()
    {
        boatShake.Tick();

        if (currentState == null) return;

        currentState.Tick();
    }

    public void Init(BoatData boatData)
    {
        BoatStateEnum state = (BoatStateEnum)Enum.GetValues(typeof(BoatStateEnum)).GetValue(boatData.StateId);
        SetState(state);

        instanceId.Register(boatData.InstanceId);

        transform.position = boatData.Position.Vector3();
        transform.rotation = Quaternion.Euler(boatData.Rotation.Vector3());

        if (boatData.DockInstanceId != null) {
            var boatDockInstance = InstancesManager.Instance.GetInstance(boatData.DockInstanceId.Value);

            if (boatDockInstance) {
                var boatDock = boatDockInstance.GetComponent<BoatDockPoint>();

                if (boatDock)
                    SetDockPoint(boatDock);
            }
        }

        CurrentStatus = boatData.Status;

        BoatsManager.Instance.RegisterBoat(this);
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
        CurrentRider = rider;
        movement.SetAgentEnabled(true);
    }

    public void RemoveCurrentRider()
    {
        CurrentRider = null;
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
        dockPoint.SetBoat(this);
        currentState.OnBoatDockChanged(dockPoint);
    }

    public void RemoveDockPoint()
    {
        if (!DockPoint) {
            Debug.Log($"DockPoint is already null at {name}");
            return;
        }

        DockPoint.RemoveBoat();
        DockPoint = null;
        currentState.OnBoatDockChanged(null);
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
        if (currentState != null && state == CurrentStateEnum) return;

        if (currentState != null) {
            currentState.Exit();
        }

        switch (state) {
            case BoatStateEnum.Idle:
                currentState = new BoatIdleState(this);
                break;
            case BoatStateEnum.FindingLoot:
                currentState = new BoatFindingLootState(this);
                break;
            case BoatStateEnum.MovingToLoot:
                currentState = new BoatMovingToLootState(this);
                break;
            case BoatStateEnum.CollectingLoot:
                currentState = new BoatCollectingLootState(this);
                break;
            case BoatStateEnum.MovingToDock:
                currentState = new BoatMovingToDockState(this);
                break;
            case BoatStateEnum.UnloadingLoot:
                currentState = new BoatUnloadingState(this);
                break;
            case BoatStateEnum.FloatingAway:
                currentState = new BoatFloatingAwayState(this);
                break;
            case BoatStateEnum.Demolished:
                currentState = new BoatDemolishState(this);
                break;
        }

        CurrentStateEnum = state;
        currentState.Enter();
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

    private void OnMovementStarted()
    {
        if (!CurrentRider) return;

        CurrentRider.HandleBoatMovementStarted();
    }

    private void OnMovementStopped()
    {
        currentState.OnReachedPath();

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