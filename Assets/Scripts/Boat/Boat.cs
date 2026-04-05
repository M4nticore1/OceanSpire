using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BoatEntry
{
    public int id { get; private set; } = 0;
    public int instanceId { get; private set; } = 0;
    public BoatStateEnum state { get; private set; } = BoatStateEnum.Idle;
    public Vector3 position { get; private set; } = Vector3.zero;
    public Vector3 rotation { get; private set; } = Vector3.zero;
    public int dockInstanceId { get; private set; } = 0;
    public float health { get; private set; } = 0;

    public BoatEntry(int id, int instanceId, BoatStateEnum state, Vector3 position, Vector3 rotation, float health, int dockInstanceId)
    {
        this.id = id;
        this.state = state;
        this.instanceId = instanceId;
        this.position = position;
        this.rotation = rotation;
        this.health = health;
        this.dockInstanceId = dockInstanceId;
    }
}

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
    private BuildingsManager buildingsManager;

    [SerializeField] private BoatData boatData;
    public BoatData BoatData => boatData;

    public BoatStateEnum currentState { get; private set; } = BoatStateEnum.Idle;
    private BoatState state;
    public BoatRider currentRider { get; private set; }

    public int instanceId { get; private set; } = 0;

    // Components
    [SerializeField] private BoatLootHandler lootHandler;

    [SerializeField] private EntityMovement movement;
    public EntityMovement Movement => movement;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [SerializeField] private Health health;
    public Health Health => health;

    [SerializeField] private HealthDrainer healthDrainer;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    // Dock
    public BoatDockPoint dockPoint;
    public LootContainer targetLootContainer { get; private set; }

    // Health
    public float CurrentHealth => health.CurrentHealth;
    public float MaxHealth => health.MaxHealth;

    // Weight
    public float CurrentWeight => inventory.CurrentWeight;
    public float MaxWeight => inventory.MaxWeight;

    [SerializeField] private Transform seatSlot;
    public Transform SeatSlot => seatSlot;

    public bool isDemolished { get; private set; } = false;

    public ContextMenu spawnedDetailsMenu { get; set; }

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
        state.Process();
    }

    public void Init(BoatEntry data)
    {
        buildingsManager = FindAnyObjectByType<BuildingsManager>();
        lootHandler = GetComponent<BoatLootHandler>();

        lootHandler.Init();

        instanceId = data.instanceId;

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        health.Init(data.health);

        BoatDockPoint dockPoint = DockPointsManager.instance.DockPointsDict[data.dockInstanceId];
        SetDockPoint(dockPoint);

        SetState(data.state);
        BoatsManager.Instance.RegisterBoat(this);
    }

    public void HandleReturnedToDock()
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
        currentRider = rider;

        Human human = rider.GetComponent<Human>();
        if (human.currentStateEnum == HumanStateEnum.Wanderer) {
            selectComponent.SetClickable(false);
        }

        movement.SetAgentEnabled(true);
    }

    public void RemoveRider()
    {
        currentRider = null;
    }

    // Dock Point
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        this.dockPoint = dockPoint;
        dockPoint.SetBoat(this);
    }

    public void RemoveDockPoint()
    {
        dockPoint.RemoveBoat();
        dockPoint = null;
    }

    public void SetTargetLoot(LootContainer lootContainer)
    {
        targetLootContainer = lootContainer;
    }

    public ItemInstance GetItemToUnload()
    {
        return inventory.items[0].item;
    }

    public void ProcessDrainHealth()
    {
        healthDrainer.ProcessDrainHealth();
    }

    // Events
    private void OnReachedPath()
    {
        state.HandleReachedPath();
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