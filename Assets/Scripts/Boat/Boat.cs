using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BoatEntry
{
    public int dockIndex = 0;
    public float health = 0;
    public Vector3 position;
    public Vector3 rotation;
}

public enum BoatStateEnum
{
    Idle,
    FindingLoot,
    MovingToLoot,
    CollectingLoot,
    ReturningToDock,
    UnloadingLoot,
    Demolished
}

public class Boat : MonoBehaviour
{
    [SerializeField] private BoatData boatData = null;
    public BoatData BoatData => boatData;

    public BoatStateEnum currentState { get; private set; } = BoatStateEnum.Idle;
    private BoatState state = null;
    public BoatRider rider { get; private set; } = null;

    // Components
    [SerializeField] private NavMeshAgent navAgent = null;

    [SerializeField] private EntityMovement movement = null;
    public EntityMovement Movement => movement;

    [SerializeField] private Inventory inventory = null;
    public Inventory Inventory => inventory;

    [SerializeField] private Health health = null;
    public Health Health => health;

    [SerializeField] private HealthDrainer healthDrainer = null;

    [SerializeField] private SelectComponent selectComponent = null;

    // Dock
    public BoatDockPoint dockPoint { get; private set; } = null;
    public LootContainer targetLootContainer { get; private set; } = null;

    // Health
    public float CurrentHealth => health.CurrentHealth;
    public float MaxHealth => health.MaxHealth;

    // Weight
    public float CurrentWeight => inventory.CurrentWeight;
    public float MaxWeight => inventory.MaxWeight;

    [SerializeField] private Transform seatSlot = null;
    public Transform SeatSlot => seatSlot;

    public bool isDemolished { get; private set; } = false;

    public ContextMenuBase spawnedDetailsMenu { get; set; } = null;

    public static event Action<Boat> OnBoadDestroyed;

    private void OnEnable()
    {
        movement.onReachedPath += OnReachedPath;
        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;
    }

    private void OnDisable()
    {
        movement.onReachedPath -= OnReachedPath;
        selectComponent.onDeselected -= OnDeselected;
    }

    private void Update()
    {
        state.Process();
    }

    public void Init(BoatEntry data)
    {
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        health.Init(data.health);
        PierConstruction pierConstruction = CityManager.Instance.PierBuilding.spawnedConstruction as PierConstruction;
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
        SetState(BoatStateEnum.FindingLoot);
    }

    public void ExitBoat()
    {
        rider = null;
    }

    // Setters
    public void SetDockPoint(BoatDockPoint dockPoint)
    {
        this.dockPoint = dockPoint;
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
        Debug.Log(state);

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
            case BoatStateEnum.ReturningToDock:
                this.state = new BoatReturningState(this);
                break;
            case BoatStateEnum.UnloadingLoot:
                this.state = new BoatUnloadingState(this);
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
        EventBus.InvokeSelectedBoat(this);
    }

    private void OnDeselected()
    {
        EventBus.InvokeDeselectedBoat(this);
    }
}