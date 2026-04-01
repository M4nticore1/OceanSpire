using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BoatEntry
{
    public BoatEntry (int id, Vector3 position, Vector3 rotation, float health)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
        this.health = health;
    }

    public int id { get; private set; } = 0;
    public Vector3 position { get; private set; } = Vector3.zero;
    public Vector3 rotation { get; private set; } = Vector3.zero;
    public int dockIndex { get; private set; } = 0;
    public float health { get; private set; } = 0;
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
    public BoatRider rider { get; private set; }

    // Components
    [SerializeField] private NavMeshAgent navAgent;
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
    public BoatDockPoint dockPoint { get; private set; }
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

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        health.Init(data.health);
        PierConstruction pierConstruction = buildingsManager.PierBuilding.spawnedConstruction as PierConstruction;
        SetDockPoint(pierConstruction.BoatDocks[data.dockIndex]);

        SetState(currentState);
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

    // Enter / Exit
    public void EnterBoat(BoatRider rider)
    {
        this.rider = rider;

        Human human = rider.GetComponent<Human>();
        if (human.currentStatus == HumanStatus.Wanderer) {
            selectComponent.SetClickable(false);
        }
    }

    public void ExitBoat()
    {
        rider = null;
    }

    // Setters
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        this.dockPoint = dockPoint;
        dockPoint.SetBoat(this);
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
                this.state = new BoatReturningState(this);
                break;
            case BoatStateEnum.UnloadingLoot:
                this.state = new BoatUnloadingState(this);
                break;
            case BoatStateEnum.FloatingAway:
                this.state = new BoatFloatingAway(this);
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