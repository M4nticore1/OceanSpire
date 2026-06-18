using System;
using UnityEngine;

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

public abstract class Human : Creature, IClickable
{
    [Header("Human")]
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

    [SerializeField] private ElevatorPassenger elevatorPassenger;
    public ElevatorPassenger ElevatorPassenger => elevatorPassenger;

    [SerializeField] private CreatureInteractComponent interactComponent;
    public CreatureInteractComponent InteractComponent => interactComponent;

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

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    public static event Action<Human> OnHumanInited;
    public static event Action<Human> OnHumanRevived;
    public static event Action<Human> OnHumanDied;
    public static event Action<Human> OnHumanSelected;
    public static event Action<Human> OnHumanDeselected;
    public static event Action<Human> OnEnteredBoat;
    public static event Action<Human> OnExitedBoat;

    private void Awake()
    {
        movement.NavAgent.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        healthComponent.OnDied += OnDied;

        reviveComponent.OnRevived += OnRevived;
        reviveComponent.onLimitTimeOvered += OnReviveLimitTimeOvered;

        attackComponent.OnAttackStarted += OnAttackStarted;
        attackComponent.OnAttackStopped += OnAttackStopped;

        cityNavigator.OnEnteredBuilding += OnEnteredBuilding;
        cityNavigator.OnExitedBuilding += OnExitedBuilding;

        interactComponent.OnInteractBuildingSeted += OnInteractBuildingSeted;
        interactComponent.OnInteractBuildingRemoved += OnInteractBuildingRemoved;
        interactComponent.OnInteractionStarted += OnInteractionStarted;
        interactComponent.OnInteractionStopped += OnInteractionStopped;

        boatRider.OnEnteredBoat += HandleEnteredBoat;
        boatRider.OnExitedBoat += HandleExitedBoat;
        boatRider.OnTargetBoatSeted += OnTargetBoatSeted;
        boatRider.OnTargetBoatRemoved += OnTargetBoatRemoved;
        boatRider.OnBoatSetedIdle += OnBoatSetedIdle;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;

        if (RaidManager.Instance) {
            RaidManager.Instance.OnRaidStarted += OnRaidStarted;
            RaidManager.Instance.OnRaidEnded += OnRaidEnded;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        healthComponent.OnDied -= OnDied;

        reviveComponent.OnRevived -= OnRevived;
        reviveComponent.onLimitTimeOvered -= OnReviveLimitTimeOvered;

        attackComponent.OnAttackStarted -= OnAttackStarted;
        attackComponent.OnAttackStopped -= OnAttackStopped;

        cityNavigator.OnEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.OnExitedBuilding -= OnExitedBuilding;

        interactComponent.OnInteractBuildingSeted -= OnInteractBuildingSeted;
        interactComponent.OnInteractBuildingRemoved -= OnInteractBuildingRemoved;
        interactComponent.OnInteractionStarted -= OnInteractionStarted;
        interactComponent.OnInteractionStopped -= OnInteractionStopped;

        boatRider.OnEnteredBoat -= HandleEnteredBoat;
        boatRider.OnExitedBoat -= HandleExitedBoat;
        boatRider.OnTargetBoatSeted -= OnTargetBoatSeted;
        boatRider.OnTargetBoatRemoved -= OnTargetBoatRemoved;
        boatRider.OnBoatSetedIdle -= OnBoatSetedIdle;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

        if (RaidManager.Instance) {
            RaidManager.Instance.OnRaidStarted -= OnRaidStarted;
            RaidManager.Instance.OnRaidEnded -= OnRaidEnded;
        }
    }

    protected virtual void Update()
    {

    }

    protected override void OnInit(CreatureData creatureData)
    {
        base.OnInit(creatureData);

        var humanData = creatureData as HumanData;

        nameComponent.Init(humanData.Name);
        healthComponent.Init(humanData.Health);
        cityNavigator.Init(humanData.CityNavigator);
        interactComponent.Init(humanData.Interaction);
        weaponComponent.Init(humanData.Weapon);
        skillsComponent.Init(humanData.Skills);
        boatRider.Init(humanData.BoatRider);

        DetermineNextAction();

        OnHumanInited?.Invoke(this);
    }

    protected override void OnInitedNextFrame()
    {
        base.OnInitedNextFrame();

        if (elevatorPassenger.IsRiding) return;
        if (boatRider.RidingBoat) return;

        movement.SetAgentEnabled(true);
        cityNavigator.FollowPath();

        
    }

    // Action
    protected override void DetermineNextAction()
    {
        if (ShouldStartInteracting()) {
            StartInteracting();
            return;
        }
        if (ShouldMoveToTargetBoat()) {
            MoveToTargetBoat();
            return;
        }
        if (ShouldStartEnteringBoat()) {
            StartEnteringBoat();
            return;
        }
        if (ShouldStopEnteringBoat()) {
            StopEnteringBoat();
            return;
        }
        if (ShouldStartExitingBoat()) {
            StartExitingBoat();
            return;
        }
        if (ShouldStopExitingBoat()) {
            StopExitingBoat();
            return;
        }
        if (ShouldBoatMoveToDock()) {
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFindLoot()) {
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            BoatFloatAway();
            return;
        }
        if (ShouldStartAttacking()) {
            StartAttacking();
            return;
        }
        if (ShouldStopAttacking()) {
            StopAttacking();
            return;
        }
        if (ShouldFollowPath()) {
            FollowPath();
            return;
        }
    }

    protected virtual void StartInteracting()
    {
        InteractComponent.TryStartInteracting();
        UpdateIdle();
    }

    protected virtual void StartEnteringBoat()
    {
        boatRider.TryStartEnteringBoat(boatRider.TargetBoat);
        UpdateIdle();
    }

    protected virtual void StopEnteringBoat()
    {
        boatRider.TryStopEnteringBoat();
        UpdateIdle();
    }

    protected virtual void StartExitingBoat()
    {
        boatRider.StartExitingBoat();
        UpdateIdle();
    }

    protected virtual void StopExitingBoat()
    {
        boatRider.StopExitingBoat();
        UpdateIdle();
    }

    protected virtual void MoveToTargetBoat()
    {
        movement.TryMoveTo(boatRider.TargetBoat.DockPoint.EntraceTransform.position);
        UpdateIdle();
    }

    protected virtual void StartAttacking()
    {
        UpdateIdle();
    }

    protected virtual void StopAttacking()
    {
        UpdateIdle();
    }

    protected virtual void FollowPath()
    {
        cityNavigator.FollowPath();
        UpdateIdle();
    }

    protected virtual void BoatMoveToDock()
    {
        BoatRider.RidingBoat.SetState(BoatStateEnum.MovingToDock);
        UpdateIdle();
    }

    protected virtual void BoatFindLoot()
    {
        boatRider.RidingBoat.SetState(BoatStateEnum.FindingLoot);
        UpdateIdle();
    }

    protected virtual void BoatFloatAway()
    {
        UpdateIdle();
    }

    protected virtual bool ShouldStartInteracting()
    {
        if (interactComponent.IsInteracting) return false;
        if (!interactComponent.InteractBuilding) return false;
        if (!cityNavigator.TargetBuilding) return false;
        if (!cityNavigator.CurrentBuilding) return false;
        if (cityNavigator.CurrentBuilding != interactComponent.InteractBuilding) return false;
        if (boatRider.RidingBoat) return false;
        if (!boatRider.RidingBoat && Vector3.Distance(transform.position, interactComponent.InteractBuilding.SpawnedConstruction.GetInteraction(cityNavigator).GetWaypoint(0).Transform.position) > movement.NavAgent.stoppingDistance) return false;
        if (attackComponent.IsAttacking) return false;

        return true;
    }

    protected virtual bool ShouldFollowPath()
    {
        if (boatRider.RidingBoat) return false;

        return true;
    }

    protected virtual bool ShouldMoveToTargetBoat()
    {
        if (!boatRider.TargetBoat) return false;
        if (!boatRider.TargetBoat.DockPoint) return false;
        if (boatRider.RidingBoat) return false;
        if (cityNavigator.FloorIndex > 0) return false;
        if (cityNavigator.TargetBuilding && cityNavigator.TargetBuilding != cityNavigator.CurrentBuilding) return false;
        if (movement.IsReachedPosition(boatRider.TargetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    protected virtual bool ShouldStartEnteringBoat()
    {
        if (!boatRider.TargetBoat) return false;
        if (boatRider.TargetBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;
        if (!movement.IsReachedPosition(boatRider.TargetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    protected virtual bool ShouldStopEnteringBoat()
    {
        if (boatRider.TargetBoat) return false;
        if (!BoatRider.IsEnteringBoat) return false;

        return true;
    }

    protected virtual bool ShouldStartExitingBoat()
    {
        if (!boatRider.RidingBoat) return false;
        if (boatRider.TargetBoat && boatRider.TargetBoat == BoatRider.RidingBoat) return false;
        if (boatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;
        if (!BoatRider.RidingBoat.Movement.IsReachedPosition(boatRider.RidingBoat.DockPoint.DockTransform.position)) return false;

        return true;
    }

    protected virtual bool ShouldStopExitingBoat()
    {
        if (!boatRider.RidingBoat) return false;
        if (!boatRider.IsExitingBoat) return false;
        if (boatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;

        return true;
    }

    protected virtual bool ShouldBoatMoveToDock()
    {
        if (!boatRider.RidingBoat) return false;
        if (!boatRider.RidingBoat.DockPoint) return false;
        if (boatRider.RidingBoat.CurrentStateEnum == BoatStateEnum.MovingToDock) return false;
        if (BoatRider.RidingBoat.Movement.IsReachedPosition(boatRider.RidingBoat.DockPoint.DockTransform.position)) return false;

        return true;
    }

    protected virtual bool ShouldBoatFindLoot()
    {
        if (!boatRider.RidingBoat) return false;
        if (!boatRider.RidingBoat.ShouldFindLoot()) return false;

        return true;
    }

    protected virtual bool ShouldBoatFloatAway()
    {
        if (!boatRider.RidingBoat) return false;

        return true;
    }

    protected virtual bool ShouldStartAttacking()
    {
        return false;
    }

    protected virtual bool ShouldStopAttacking()
    {
        return false;
    }

    // IClickable
    public void Click()
    {
        if (boatRider.RidingBoat) {
            BoatRider.RidingBoat.SelectComponent.Click();
        }
        else {
            selectComponent.Click();
        }

        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public virtual bool ShouldClick()
    {
        return true;
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
    protected virtual void OnRevived()
    {
        TryStartIdle();
        contextMenuTarget.SetShowContextMenu(true);

        OnHumanRevived?.Invoke(this);
    }

    protected virtual void OnDied()
    {
        var interactBuilding = interactComponent.InteractBuilding;
        interactComponent.TryRemoveInteractBuilding();
        interactComponent.TryStopInteracting(interactBuilding);

        TryStopIdle();
        contextMenuTarget.SetShowContextMenu(false);

        OnHumanDied?.Invoke(this);
    }

    // Revive
    private void OnReviveLimitTimeOvered()
    {
        Destroy(gameObject);
    }

    // Movement
    protected override void OnMovementStopped()
    {
        base.OnMovementStopped();

        DetermineNextAction();
    }

    // Attack
    protected virtual void OnAttackStarted()
    {
        DetermineNextAction();
    }

    protected virtual void OnAttackStopped()
    {
        DetermineNextAction();
    }

    // Entrance
    protected virtual void OnEnteredBuilding(Building building)
    {
        DetermineNextAction();
    }

    protected virtual void OnExitedBuilding(Building building)
    {
        DetermineNextAction();
    }

    protected virtual void OnInteractBuildingSeted(Building building)
    {
        cityNavigator.SetTargetBuilding(building);
        cityNavigator.TryFindPathToTargetBuilding();
        DetermineNextAction();
    }

    protected virtual void OnInteractBuildingRemoved(Building building)
    {
        if (building == cityNavigator.TargetBuilding) {
            cityNavigator.RemoveTargetBuilding();
            cityNavigator.RemovePath();
        }

        DetermineNextAction();
    }

    protected virtual void OnInteractionStarted(Building building)
    {
        DetermineNextAction();
    }

    protected virtual void OnInteractionStopped(Building building)
    {
        DetermineNextAction();
    }

    // Boat
    protected virtual void HandleEnteredBoat(Boat boat)
    {
        DetermineNextAction();

        OnEnteredBoat?.Invoke(this);
    }

    protected virtual void HandleExitedBoat(Boat boat)
    {
        DetermineNextAction();

        OnExitedBoat?.Invoke(this);
    }

    protected virtual void OnBoatSetedIdle(Boat boat)
    {
        DetermineNextAction();
    }

    private void OnTargetBoatSeted(Boat boat)
    {
        var interactBuilding = interactComponent.InteractBuilding;
        if (interactBuilding && !interactBuilding.GetComponent<PierModule>()) {
            interactComponent.RemoveInteractBuilding();
            interactComponent.TryStopInteracting(interactBuilding);
        }

        if (cityNavigator.CurrentBuilding && cityNavigator.CurrentBuilding as TowerBuilding) {
            cityNavigator.SetTargetBuilding(BuildingsManager.Instance.TowerGate);

            if (cityNavigator.TryFindPathToTargetBuilding()) {
                cityNavigator.FollowPath();
            }
        }

        DetermineNextAction();
    }

    private void OnTargetBoatRemoved(Boat boat)
    {
        DetermineNextAction();
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

    private void OnSelected()
    {
        OnHumanSelected?.Invoke(this);
    }

    private void OnDeselected()
    {
        OnHumanDeselected?.Invoke(this);
    }
}