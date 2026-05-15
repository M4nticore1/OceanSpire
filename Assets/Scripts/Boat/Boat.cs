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

public class Boat : MonoBehaviour
{
    [SerializeField] private BoatDefinition boatData;
    public BoatDefinition Definition => boatData;

    public BoatStateEnum currentState { get; private set; } = BoatStateEnum.Idle;
    private BoatState state;
    public BoatRider SelectedRider { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    // Components
    [SerializeField] private BoatLootHandler lootHandler;

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

    public bool isDemolished { get; private set; } = false;

    public static event Action<Boat> onBoatSelected;
    public static event Action<Boat> onBoatDeselected;
    public static event Action<Boat> onBoatDestroyed;

    private void OnEnable()
    {
        movement.onReachedPath += OnReachedPath;
        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    private void OnDisable()
    {
        movement.onReachedPath -= OnReachedPath;
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
        lootHandler = GetComponent<BoatLootHandler>();
        lootHandler.Init();

        instanceId.Init(data.InstanceId);

        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        if (data.DockInstanceId != null) {
            InstanceId boatDockInstance = InstancesManager.Instance.GetInstance(data.DockInstanceId.Value);
            BoatDockPoint boatDock = boatDockInstance?.GetComponent<BoatDockPoint>();

            SetDockPoint(boatDock);
        }

        SetState(BoatStateEnum.Idle);
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
        movement.TryMoveTo(position);
        SetState(BoatStateEnum.FloatingAway);
    }

    // Enter / Exit
    public void SetRider(BoatRider rider)
    {
        SelectedRider = rider;

        Human human = rider.GetComponent<Human>();
        if (human.CurrentStatusEnum == HumanStatusEnum.Wanderer) {
            contextMenuTarget.SetShowContextMenu(false);
        }
        else if (human.CurrentStatusEnum == HumanStatusEnum.Raider) {
            selectComponent.SetClickable(false);
        }

        movement.SetAgentEnabled(true);
    }

    public void RemoveRider()
    {
        SelectedRider = null;
    }

    // Dock Point
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        this.DockPoint = dockPoint;
        dockPoint.SetBoat(this);
    }

    public void RemoveDockPoint()
    {
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

    // Events
    private void OnReachedPath()
    {
        state.OnReachedPath();
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
        currentState = state;
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