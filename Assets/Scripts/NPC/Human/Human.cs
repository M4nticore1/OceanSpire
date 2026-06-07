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

    [SerializeField] private BuildingInteractComponent interactComponent;
    public BuildingInteractComponent InteractComponent => interactComponent;

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

        movement.OnMovementStopped += OnStoppedMoving;
        healthComponent.onDied += OnDied;

        reviveComponent.onRevived += OnRevived;
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
        boatRider.OnStartedMovingToBoat += OnStartedMovingToBoat;
        boatRider.OnStoppedMovingToBoat += OnStoppedMovingToBoat;
        boatRider.OnBoatSetedIdle += OnBoatSetedIdle;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;

        RaidManager.Instance.OnRaidStarted += OnRaidStarted;
        RaidManager.Instance.OnRaidEnded += OnRaidEnded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        movement.OnMovementStopped -= OnStoppedMoving;
        healthComponent.onDied -= OnDied;

        reviveComponent.onRevived -= OnRevived;
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
        boatRider.OnStartedMovingToBoat -= OnStartedMovingToBoat;
        boatRider.OnStoppedMovingToBoat -= OnStoppedMovingToBoat;
        boatRider.OnBoatSetedIdle -= OnBoatSetedIdle;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

        RaidManager.Instance.OnRaidStarted -= OnRaidStarted;
        RaidManager.Instance.OnRaidEnded -= OnRaidEnded;
    }

    protected virtual void Update()
    {

    }

    protected override void OnInit(CreatureData creatureData)
    {
        base.OnInit(creatureData);

        var humanData = creatureData as HumanData;

        if (humanData.EnteredBuildingInstanceId != null) {
            var instanceId = InstancesManager.Instance.GetInstance(humanData.EnteredBuildingInstanceId.Value);
            var interactBuilding = instanceId?.GetComponent<Building>();

            cityNavigator.EnterBuilding(interactBuilding);
        }

        if (humanData.InteractBuildingInstanceId != null) {
            var instanceId = InstancesManager.Instance.GetInstance(humanData.InteractBuildingInstanceId.Value);
            var interactBuilding = instanceId?.GetComponent<Building>();

            interactComponent.SetInteractBuilding(interactBuilding);
        }

        nameComponent.Init(humanData.Name);
        healthComponent.Init(humanData.Health);
        weaponComponent.Init(humanData.Weapon);
        skillsComponent.Init(humanData.Skills);
        boatRider.Init(humanData.BoatRider);

        if (humanData.MovementStateId == (int)FollowingPathState.Riding) {
            cityNavigator.SetState(FollowingPathState.Riding);
        }

        OnHumanInited?.Invoke(this);
    }

    protected override void OnInitedNextFrame()
    {
        base.OnInitedNextFrame();

        if (cityNavigator.IsRidingOnElevator) return;
        if (boatRider.RidingBoat) return;

        movement.SetAgentEnabled(true);
        cityNavigator.FollowPath();

        if (boatRider.IsMovingToBoat) {
            boatRider.MoveToBoat();
        }
    }

    // IClickable
    public void Click()
    {
        if (boatRider.TargetBoat) {
            BoatRider.TargetBoat.SelectComponent.Click();
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
        interactComponent.TryRemoveInteractBuilding();
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
    protected override void OnStoppedMoving()
    {
        base.OnStoppedMoving();

        TryStartInteracting();
    }

    // Attack
    protected virtual void OnAttackStarted()
    {
        TryStopIdle();
    }

    protected virtual void OnAttackStopped()
    {
        TryStartIdle();
    }

    // Entrance
    protected virtual void OnEnteredBuilding(Building building)
    {
        if (boatRider.IsMovingToBoat && building == cityNavigator.TargetBuilding) {
            Vector3 position = boatRider.TargetBoat.DockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }
    }

    protected virtual void OnExitedBuilding(Building building)
    {
        //if (boatRider.IsMovingToBoat && !cityNavigator.TargetBuilding) {
        //    Vector3 position = boatRider.SelectedBoat.DockPoint.EntraceTransform.position;
        //    movement.TryMoveTo(position);
        //}
    }

    protected virtual void OnInteractBuildingSeted(Building building)
    {
        cityNavigator.SetTargetBuilding(building);

        if (cityNavigator.TryFindPathToTargetBuilding()) {
            cityNavigator.FollowPath();
        }

        //if (building == cityNavigator.CurrentBuilding) {
        //    movement.TryMoveTo(building.GetInteractionTransform(cityNavigator).position);
        //    TryStartInteracting();
        //}
    }

    protected virtual void OnInteractBuildingRemoved(Building building)
    {
        cityNavigator.SetTargetBuilding(null);
        cityNavigator.RemovePath();

        if (boatRider.RidingBoat) {
            BoatRider.RidingBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else if (boatRider.IsEnteringBoat) {
            boatRider.TryStopEnteringBoat();
            boatRider.StopWaitingForBoat();
        }
        else {
            cityNavigator.UpdateFollowingPathState();
        }
    }

    protected virtual void OnInteractionStarted(Building building)
    {
        StopIdle();
    }

    protected virtual void OnInteractionStopped(Building building)
    {
        TryStartIdle();
    }

    // Boat
    protected virtual void HandleEnteredBoat(Boat boat)
    {
        movement.SetAgentEnabled(false);
        OnEnteredBoat?.Invoke(this);
    }

    protected virtual void HandleExitedBoat(Boat boat)
    {
        if (interactComponent.InteractBuilding) {
            Debug.Log(cityNavigator.TryFindPathToTargetBuilding());
            if (cityNavigator.TryFindPathToTargetBuilding()) {
                cityNavigator.FollowPath();
            }
        }

        OnExitedBoat?.Invoke(this);
    }

    protected virtual void OnBoatSetedIdle(Boat boat)
    {

    }

    private void OnStartedMovingToBoat(Boat boat)
    {
        if (cityNavigator.FloorIndex > 0) {
            var building = BuildingsManager.Instance.TowerGate;
            cityNavigator.SetTargetBuilding(building);

            if (cityNavigator.TryFindPathToTargetBuilding()) {
                cityNavigator.FollowPath();
            }
        }
        else if (boatRider.RidingBoat) {
            Debug.Log("MoveToBoat " + NameComponent.GetName());
            boatRider.RidingBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            var position = boatRider.TargetBoat.DockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }
    }

    private void OnStoppedMovingToBoat(Boat boat)
    {

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

    private void TryStartInteracting()
    {
        if (!ShouldStartInteracting()) return;

        InteractComponent.TryStartInteracting();
    }

    private bool ShouldStartInteracting()
    {
        if (attackComponent.IsAttacking) return false;
        if (!interactComponent.InteractBuilding) return false;
        if (interactComponent.InteractBuilding != cityNavigator.CurrentBuilding) return false;
        if (!boatRider.RidingBoat && Vector3.Distance(transform.position, interactComponent.InteractBuilding.GetInteractionTransform(cityNavigator).position) > movement.NavAgent.stoppingDistance) return false;

        return true;
    }
}