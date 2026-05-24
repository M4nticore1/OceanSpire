using System;
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

public class Boat : MonoBehaviour, IClickable
{
    [SerializeField] private BoatDefinition boatData;
    public BoatDefinition Definition => boatData;

    public BoatStateEnum CurrentState { get; private set; }
    private BoatState state;

    public HumanStatusEnum CurrentStatus { get; private set; }

    public BoatRider SelectedRider { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Movement movement;
    public Movement Movement => movement;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [SerializeField] private HealthDrainer healthDrainer;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    [SerializeField] private ContextMenuTarget contextMenuTarget;
    public ContextMenuTarget ContextMenuTarget => contextMenuTarget;

    // Dock
    public BoatDockPoint DockPoint;
    public LootContainer targetLootContainer { get; private set; }

    // Weight
    public float CurrentWeight => inventory.CurrentWeight;
    public float MaxWeight => inventory.WeightLimit;

    [SerializeField] private Transform seatSlot;
    public Transform SeatSlot => seatSlot;

    public static event Action<Boat> onBoatSelected;
    public static event Action<Boat> onBoatDeselected;
    public static event Action<Boat> onBoatDestroyed;

    private void OnEnable()
    {
        movement.OnMovementStarted += OnMovementStarted;
        movement.OnReachedPath += OnMovementStopped;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    private void OnDisable()
    {
        movement.OnMovementStarted -= OnMovementStarted;
        movement.OnReachedPath -= OnMovementStopped;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        BoatsManager.Instance.UnregisterBoat(this);
    }

    private void Update()
    {
        state.Tick();
    }

    public void Init(BoatData data)
    {
        instanceId.Register(data.InstanceId);

        SetState(BoatStateEnum.Idle);
        CurrentStatus = data.Status;

        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        if (data.DockInstanceId != null) {
            var boatDockInstance = InstancesManager.Instance.GetInstance(data.DockInstanceId.Value);
            var boatDock = boatDockInstance?.GetComponent<BoatDockPoint>();

            SetDockPoint(boatDock);
        }

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
    public void SetRider(BoatRider rider)
    {
        SelectedRider = rider;
        movement.SetAgentEnabled(true);
    }

    public void RemoveRider()
    {
        SelectedRider = null;
    }

    // Dock Point
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        if (dockPoint == DockPoint) return;

        DockPoint = dockPoint;
        dockPoint.SetBoat(this);
    }

    public void RemoveDockPoint()
    {
        if (!DockPoint) return;

        DockPoint.RemoveBoat();
        DockPoint = null;
    }

    public void SetTargetLoot(LootContainer lootContainer)
    {
        targetLootContainer = lootContainer;
    }

    public ItemInstance GetItemToUnload()
    {
        return inventory.GetItemByIndex(0);
    }

    public void ProcessDrainHealth()
    {
        healthDrainer.ProcessDrainHealth();
    }

    // State
    public void SetState(BoatStateEnum state)
    {
        if (this.state != null) {
            this.state.Exit();
        }

        switch (state) {
            case BoatStateEnum.Idle:
                this.state = new BoatIdleState(this);
                break;
            case BoatStateEnum.FindingLoot:
                this.state = new BoatFindingLootState(this);
                break;
            case BoatStateEnum.MovingToLoot:
                this.state = new BoatMovingToLootState(this);
                break;
            case BoatStateEnum.CollectingLoot:
                this.state = new BoatCollectingLootState(this);
                break;
            case BoatStateEnum.MovingToDock:
                this.state = new BoatMovingToDockState(this);
                break;
            case BoatStateEnum.UnloadingLoot:
                this.state = new BoatUnloadingState(this);
                break;
            case BoatStateEnum.FloatingAway:
                this.state = new BoatFloatingAwayState(this);
                break;
            case BoatStateEnum.Demolished:
                this.state = new BoatDemolishState(this);
                break;
        }

        this.state.Enter();
        CurrentState = state;
    }

    // IClickable
    public void Click()
    {
        selectComponent.Click();
    }

    public bool ShouldClick()
    {
        return CurrentStatus == HumanStatusEnum.Citizen;
    }

    private void OnMovementStarted()
    {
        SelectedRider.HandleBoatMovementStarted();
    }

    private void OnMovementStopped()
    {
        state.OnReachedPath();
        SelectedRider.HandleBoatMovementStopped();
    }

    // Clickable
    private void OnSelected()
    {
        onBoatSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        onBoatDeselected?.Invoke(this);
    }
}