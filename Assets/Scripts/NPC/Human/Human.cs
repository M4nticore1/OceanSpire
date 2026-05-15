using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum HumanStatusEnum
{
    Citizen,
    Wanderer,
    Raider
}

public enum HumanActivity
{
    Idle,
    Working,
    Raiding
}

public class Human : Creature, IClickable
{
    [Header("Human")]
    [SerializeField] private HumanStatusEnum currentStatusEnum = HumanStatusEnum.Citizen;
    public HumanStatusEnum CurrentStatusEnum => currentStatusEnum;

    public HumanState currentStatus { get; private set; }

    [SerializeField] private GenderComponent genderComponent;
    public GenderComponent GenderComponent => genderComponent;

    [SerializeField] private NameComponent nameComponent;
    public NameComponent NameComponent => nameComponent;

    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent => healthComponent;

    [SerializeField] private ReviveComponent reviveComponent;
    public ReviveComponent ReviveComponent => reviveComponent;

    [SerializeField] private CreatureCityNavigator cityNavigator;
    public CreatureCityNavigator CityNavigator => cityNavigator;

    [SerializeField] private InteractComponent interactComponent;
    public InteractComponent InteractComponent => interactComponent;

    [SerializeField] private BoatRider boatRider;
    public BoatRider BoatRider => boatRider;

    [SerializeField] private AttackComponent attackComponent;
    public AttackComponent AttackComponent => attackComponent;

    [SerializeField] private EquipmentComponent weaponComponent;
    public EquipmentComponent WeaponComponent => weaponComponent;

    [SerializeField] private SkillsComponent skillsComponent;
    public SkillsComponent SkillsComponent => skillsComponent;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    [SerializeField] private ContextMenuTarget contextMenuTarget;
    public ContextMenuTarget ContextMenuTarget => contextMenuTarget;

    public static event Action<Human> onHumanInited;
    public static event Action<Human> onHumanRevived;
    public static event Action<Human> onHumanDied;
    public static event Action<Human> onWandererAccepted;
    public static event Action<Human> onWandererRejected;
    public static event Action<Human> onHumanSelected;
    public static event Action<Human> onHumanDeselected;
    public static event Action<Human> onEnteredBoat;
    public static event Action<Human> onExitedBoat;

    private void Awake()
    {
        movement.NavAgent.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        healthComponent.onDied += OnDied;

        reviveComponent.onRevived += OnRevived;
        reviveComponent.onLimitTimeOvered += OnReviveLimitTimeOvered;

        attackComponent.onAttackStarted += OnAttackStarted;
        attackComponent.onAttackStopped += OnAttackStopped;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onReachedPath += OnReachedPathBuilding;

        interactComponent.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactComponent.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactComponent.onInteractionStarted += OnInteractionStarted;
        interactComponent.onInteractionStopped += OnStoppedInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;

        RaidManager.Instance.onRaidStarted += OnRaidStarted;
        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        EventBus.OnNavMeshBaked += OnNavMeshBaked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        currentStatus.Exit();

        healthComponent.onDied -= OnDied;

        reviveComponent.onRevived -= OnRevived;
        reviveComponent.onLimitTimeOvered -= OnReviveLimitTimeOvered;

        attackComponent.onAttackStarted -= OnAttackStarted;
        attackComponent.onAttackStopped -= OnAttackStopped;

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onReachedPath -= OnReachedPathBuilding;

        interactComponent.onSetedInteractBuilding -= OnSetedInteractBuilding;
        interactComponent.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactComponent.onInteractionStarted -= OnInteractionStarted;
        interactComponent.onInteractionStopped -= OnStoppedInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        RaidManager.Instance.onRaidStarted += OnRaidStarted;
        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        EventBus.OnNavMeshBaked -= OnNavMeshBaked;
    }

    private void Update()
    {
        currentStatus.Tick();
    }

    protected override void OnInit(CreatureData data)
    {
        HumanData humanData = data as HumanData;

        SetStatus(currentStatusEnum);

        healthComponent.SetCurrentHealth(humanData.Health);
        nameComponent.Init(humanData.Name);

        if (humanData.EnteredBuildingInstanceId != null) {
            InstanceId instanceId = InstancesManager.Instance.GetInstance(humanData.EnteredBuildingInstanceId.Value);
            Building interactBuilding = instanceId?.GetComponent<Building>();

            cityNavigator.TryEnterBuilding(interactBuilding);
        }

        if (humanData.InteractBuildingInstanceId != null) {
            InstanceId instanceId = InstancesManager.Instance.GetInstance(humanData.InteractBuildingInstanceId.Value);
            Building interactBuilding = instanceId?.GetComponent<Building>();

            interactComponent.SetInteractBuilding(interactBuilding);
        }

        if (humanData.BoatRider.BoatInstanceId != null) {
            InstanceId instanceId = InstancesManager.Instance.GetInstance(humanData.BoatRider.BoatInstanceId.Value);
            Boat selectedBoat = instanceId.GetComponent<Boat>();

            boatRider.SetSelectedBoat(selectedBoat);
        }

        weaponComponent.Init(humanData.Weapon);
        skillsComponent.Init(humanData.Skills);

        if (humanData.RidingOnElevator) {
            cityNavigator.SetState(FollowingPathState.Riding);
        }

        if (humanData.BoatRider.IsRiding) {
            boatRider.EnterBoat();
        }

        onHumanInited?.Invoke(this);
    }

    // Boat
    public void TryMoveToBoat()
    {
        if (!boatRider.SelectedBoat) return;
        if (!movement.CanMove()) return;

        if (cityNavigator.FloorIndex > 0) {
            Building building = BuildingsManager.Instance.TowerGate;
            cityNavigator.SetTargetBuilding(building);
            cityNavigator.TryFindPathToTargetBuilding();
        }
        else {
            Vector3 position = boatRider.SelectedBoat.DockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }

        boatRider.StartMovingToBoat();
    }

    // Wanderer
    public void AcceptWanderer()
    {
        SetStatus(HumanStatusEnum.Citizen);
        Destroy(boatRider.SelectedBoat.gameObject);
        boatRider.ExitBoat();
        onWandererAccepted?.Invoke(this);
    }

    public void RejectWanderer()
    {
        onWandererRejected?.Invoke(this);
    }

    // IClickable
    public void Click()
    {
        BoatRider.SelectedBoat.SelectComponent.Select();
    }

    public bool ShouldClick()
    {
        return currentStatusEnum == HumanStatusEnum.Wanderer;
    }

    protected override bool ShouldStartIdle()
    {
        if (movement.IsMoving) return false;
        if (interactComponent.IsInteracting) return false;
        if (attackComponent.IsAttacking) return false;
        if (!healthComponent.IsAlive) return false;

        return true;
    }

    // Health
    private void OnRevived()
    {
        currentStatus.OnRevived();
        TryStartIdle();
        contextMenuTarget.SetShowContextMenu(true);

        onHumanRevived?.Invoke(this);
    }

    private void OnDied()
    {
        currentStatus.OnDied();
        interactComponent.RemoveInteractBuilding();
        TryStopIdle();
        contextMenuTarget.SetShowContextMenu(false);

        onHumanDied?.Invoke(this);
    }

    // Revive
    private void OnReviveLimitTimeOvered()
    {
        Destroy(gameObject);
    }

    // Movement
    protected override void OnStoppedMoving()
    {
        base.OnStoppedMoving();

        currentStatus.OnStoppedMoving();

        if (ShouldStartInteracting()) {
            interactComponent.StartInteracting();
        }
        else if (ShouldStartEnteringBoat()) {
            boatRider.StartEnteringBoat();
            boatRider.StopMovingToBoat();
        }
    }

    // Attack
    private void OnAttackStarted()
    {
        currentStatus.OnAttackStarted();
        TryStopIdle();
    }

    private void OnAttackStopped()
    {
        currentStatus.OnAttackStopped();
        TryStartIdle();
    }

    // Entrance
    private void OnEnteredBuilding(Building building)
    {
        currentStatus.OnEnteredBuilding(building);

        if (boatRider.isMovingToBoat && building == cityNavigator.TargetBuilding) {
            Vector3 position = boatRider.SelectedBoat.DockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }
    }

    private void OnReachedPathBuilding()
    {

    }

    private void OnSetedInteractBuilding(Building building)
    {
        Building interactBuilding = interactComponent.InteractBuilding;

        cityNavigator.SetTargetBuilding(interactBuilding);
        currentStatus.OnSetedInteractBuilding(interactBuilding);
    }

    private void OnRemovedInteractBuilding(Building building)
    {
        cityNavigator.RemoveTargetBuilding();
        cityNavigator.RemovePath();

        if (boatRider.IsRidingOnBoat) {
            BoatRider.SelectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            cityNavigator.UpdateFollowingPathState();
        }

        currentStatus.OnRemovedInteractBuilding(building);
    }

    private void OnInteractionStarted()
    {
        currentStatus.OnInteractionStarted();
        StopIdle();
    }

    private void OnStoppedInteracting()
    {
        currentStatus.OnInteractionStopped();
        TryStartIdle();
    }

    // Boat
    private void OnEnteredBoat(Boat boat)
    {
        movement.SetAgentEnabled(false);
        currentStatus.OnEnteredBoat(boat);
        onEnteredBoat?.Invoke(this);
    }

    private void OnExitedBoat(Boat boat)
    {
        movement.SetAgentEnabled(true);
        movement.NavAgent.Warp(transform.position);

        if (interactComponent.InteractBuilding) {
            cityNavigator.TryFindPathToTargetBuilding();
        }

        currentStatus.OnExitedBoat(boat);
        onExitedBoat?.Invoke(this);
    }
    
    // Raid
    private void OnRaidStarted()
    {
        movement.SetMovementMethod(MovementMethod.Run);
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        movement.SetMovementMethod(MovementMethod.Walk);
    }

    // Status
    public void SetStatus(HumanStatusEnum status)
    {
        ExitStatus(currentStatusEnum);
        currentStatusEnum = status;
        EnterStatus(currentStatusEnum);
    }

    private void EnterStatus(HumanStatusEnum status)
    {
        switch (status) {
            case HumanStatusEnum.Citizen:
                currentStatus = new CitizenState(this);
                break;
            case HumanStatusEnum.Wanderer:
                currentStatus = new WandererState(this);
                break;
            case HumanStatusEnum.Raider:
                currentStatus = new RaiderState(this);
                break;
        }

        currentStatus.Enter();
    }

    private void ExitStatus(HumanStatusEnum status)
    {
        switch (status) {
            case HumanStatusEnum.Citizen:
                break;
            case HumanStatusEnum.Wanderer:
                break;
            case HumanStatusEnum.Raider:
                break;
        }

        if (currentStatus != null) {
            currentStatus.Exit();
        }
    }

    private void OnSelected()
    {
        onHumanSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        onHumanDeselected?.Invoke(this);
    }

    private void OnNavMeshBaked()
    {
        if (cityNavigator.IsRidingOnElevator) return;
        if (boatRider.IsRidingOnBoat) return;

        movement.SetAgentEnabled(true);
        cityNavigator.UpdateFollowingPathState();
    }

    private bool ShouldStartInteracting()
    {
        if (attackComponent.IsAttacking) return false;
        if (!interactComponent.InteractBuilding) return false;
        if (interactComponent.InteractBuilding != cityNavigator.CurrentBuilding) return false;

        return true;
    }

    private bool ShouldStartEnteringBoat()
    {
        if (!boatRider.isMovingToBoat) return false;
        if (cityNavigator.FloorIndex != 0) return false;
        if (movement.NavAgent.remainingDistance > movement.NavAgent.stoppingDistance) return false;

        return true;
    }
}