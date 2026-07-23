using System;
using System.Collections;
using System.Collections.Generic;
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

public abstract class Human : Creature, IClickable, ILocalizable
{
    [Header("Human")]
    [SerializeField] private GenderComponent genderComponent;
    public GenderComponent GenderComponent => genderComponent;

    [SerializeField] private NameComponent nameComponent;
    public NameComponent NameComponent => nameComponent;

    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent => healthComponent;

    [SerializeField] private HealthDisplay healthDisplay;
    public HealthDisplay HealthDisplay => healthDisplay;

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

    [SerializeField] private bool isClickable = true;
    public bool IsClickable => isClickable;

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
        reviveComponent.OnLimitTimeOvered += OnReviveLimitTimeOvered;

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
        BoatRider.OnBoatMovementStarted += OnBoatMovementStarted;
        BoatRider.OnBoatMovementStopped += OnBoatMovementStopped;

        selectComponent.OnSelected += OnSelected;
        selectComponent.OnDeselected += OnDeselected;

        if (RaidManager.Instance) {
            RaidManager.Instance.OnRaidStarted += OnRaidStarted;
            RaidManager.Instance.OnRaidEnded += OnRaidEnded;
        }
        else
            Debug.Log("raidManager is not valid", this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        healthComponent.OnDied -= OnDied;

        reviveComponent.OnRevived -= OnRevived;
        reviveComponent.OnLimitTimeOvered -= OnReviveLimitTimeOvered;

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
        BoatRider.OnBoatMovementStarted -= OnBoatMovementStarted;
        BoatRider.OnBoatMovementStopped -= OnBoatMovementStopped;

        selectComponent.OnSelected -= OnSelected;
        selectComponent.OnDeselected -= OnDeselected;

        if (RaidManager.Instance) {
            RaidManager.Instance.OnRaidStarted -= OnRaidStarted;
            RaidManager.Instance.OnRaidEnded -= OnRaidEnded;
        }
    }

    public override void Tick()
    {
        base.Tick();
    }

    protected override void OnInit(CreatureData creatureData)
    {
        base.OnInit(creatureData);

        var humanData = creatureData as HumanData;

        if (humanData == null) {
            Debug.LogError("humanData is not valid");
            humanData = HumanData.Default();
        }

        nameComponent.Init(humanData.Name);
        skillsComponent.Init(humanData.Skills);
        healthComponent.Init(humanData.Health);
        reviveComponent.Init(humanData.Revive);
        cityNavigator.Init(humanData.CityNavigator);
        interactComponent.Init(humanData.Interaction);
        weaponComponent.Init(humanData.Weapon);
        boatRider.Init(humanData.BoatRider);

        OnHumanInited?.Invoke(this);
    }

    protected override void OnInitNextFrame()
    {
        if (!elevatorPassenger.IsRiding && !boatRider.RidingBoat) {
            movement.SetAgentEnabled(true);
            cityNavigator.FollowPath();
        }

        base.OnInitNextFrame();
    }

    // Action
    protected override void DetermineNextAction()
    {
        if (ShouldStartInteracting()) {
            //Debug.Log("ShouldStartInteracting");
            StartInteracting();
            return;
        }
        if (ShouldStopInteracting()) {
            //Debug.Log("ShouldStopInteracting");
            StopInteracting();
            return;
        }
        if (ShouldMoveToTargetBoat()) {
            //Debug.Log("ShouldMoveToTargetBoat");
            MoveToTargetBoat();
            return;
        }
        if (ShouldWaitForEnteringBoat()) {
            //Debug.Log("ShouldStartEnteringBoat");
            StartEnteringBoat();
            return;
        }
        if (ShouldStopEnteringBoat()) {
            //Debug.Log("ShouldStopEnteringBoat");
            StopEnteringBoat();
            return;
        }
        if (ShouldStartExitingBoat()) {
            //Debug.Log("ShouldStartExitingBoat");
            StartExitingBoat();
            return;
        }
        if (ShouldStopExitingBoat()) {
            //Debug.Log("ShouldStopExitingBoat");
            StopExitingBoat();
            return;
        }
        if (ShouldBoatMoveToDock()) {
            //Debug.Log("ShouldBoatMoveToDock");
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFindLoot()) {
            //Debug.Log("ShouldBoatFindLoot");
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            //Debug.Log("ShouldBoatFloatAway");
            BoatFloatAway();
            return;
        }
        if (ShouldStartAttacking()) {
            //Debug.Log("ShouldStartAttacking");
            StartAttacking();
            return;
        }
        if (ShouldStopAttacking()) {
            //Debug.Log("ShouldStopAttacking");
            StopAttacking();
            return;
        }
        if (ShouldFollowPath()) {
            //Debug.Log("ShouldFollowPath");
            FollowPath();
            return;
        }
    }

    protected virtual void StartInteracting()
    {
        InteractComponent.TryStartInteracting();
        UpdateIdle();
    }

    protected virtual void StopInteracting()
    {
        var interactBuilding = InteractComponent.InteractBuilding;
        InteractComponent.TryStopInteracting(interactBuilding);
        UpdateIdle();
    }

    protected virtual void StartEnteringBoat()
    {
        boatRider.WaitForBoatAndEnter();
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
        if (!movement.TryMoveTo(boatRider.TargetBoat.DockPoint.EntraceTransform.position)) {
            cityNavigator.SetTargetBuilding(BuildingsManager.Instance.TowerGate);
            cityNavigator.TryFindPathToTargetBuilding();
            cityNavigator.FollowPath();
        }

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

    protected virtual void FollowPath()
    {
        cityNavigator.FollowPath();
        UpdateIdle();
    }

    public virtual bool ShouldStartInteracting()
    {
        if (interactComponent.IsInteracting) return false;

        var interactBuilding = interactComponent.InteractBuilding;
        if (!interactBuilding) return false;
        if (!cityNavigator.CurrentBuilding) return false;
        if (cityNavigator.CurrentBuilding != interactBuilding) return false;
        if (interactBuilding.GetComponent<PierModule>()) return false;

        if (boatRider.RidingBoat) return false;
        if(!healthComponent.IsAlive) return false;
        if (attackComponent.IsAttacking) return false;

        var waypoint = cityNavigator.WaypointsComponent.GetCurrentWaypoint();
        if (waypoint == null || !waypoint.Transform) {
            Debug.LogError("waypoint or its transform is not valid", this);
            return false;
        }

        if (!movement.IsReachedPosition(waypoint.Transform.position)) return false;

        return true;
    }

    public virtual bool ShouldStopInteracting()
    {
        if (!interactComponent.IsInteracting) return false;
        if (!interactComponent.InteractBuilding) return false;
        if (!healthComponent.IsAlive) return true;
        if (attackComponent.IsAttacking) return true;

        return false;
    }

    public virtual bool ShouldMoveToTargetBoat()
    {
        var targetBoat = boatRider.TargetBoat;
        if (!targetBoat) return false;
        if (!targetBoat.DockPoint) return false;
        if (!targetBoat.DockPoint.EntraceTransform) return false;

        if (boatRider.RidingBoat) return false;
        if (cityNavigator.FloorIndex > 0) return false;
        if (attackComponent.IsAttacking) return false;
        if (movement.IsReachedPosition(targetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    public virtual bool ShouldWaitForEnteringBoat()
    {
        var targetBoat = boatRider.TargetBoat;
        if (!targetBoat) return false;
        if (!targetBoat.DockPoint) return false;
        if (!targetBoat.DockPoint.EntraceTransform) return false;
        if (!movement.IsReachedPosition(targetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    public virtual bool ShouldStopEnteringBoat()
    {
        if (boatRider.TargetBoat) return false;
        if (!boatRider.IsEnteringBoat) return false;

        return true;
    }

    public virtual bool ShouldStartExitingBoat()
    {
        if (!boatRider.RidingBoat) return false;
        if (boatRider.TargetBoat && boatRider.TargetBoat == BoatRider.RidingBoat) return false;
        if (boatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;
        if (!BoatRider.RidingBoat.Movement.IsReachedPosition(boatRider.RidingBoat.DockPoint.DockTransform.position)) return false;

        return true;
    }

    public virtual bool ShouldStopExitingBoat()
    {
        if (!boatRider.RidingBoat) return false;
        if (!boatRider.IsExitingBoat) return false;
        if (boatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;

        return true;
    }

    public virtual bool ShouldBoatMoveToDock()
    {
        var ridingBoat = boatRider.RidingBoat;
        if (!ridingBoat) return false;

        var dockPoint = ridingBoat.DockPoint;
        if (!dockPoint) return false;

        var boatState = ridingBoat.CurrentStateEnum;
        if (boatState == BoatStateEnum.MovingToDock) return false;
        if (boatState == BoatStateEnum.Idle && ridingBoat.Movement.IsReachedPosition(dockPoint.DockTransform.position)) return false;

        if (boatRider.IsExitingBoat) return false;

        return true;
    }

    public virtual bool ShouldBoatFindLoot()
    {
        if (!boatRider.RidingBoat) return false;
        if (!boatRider.RidingBoat.ShouldFindLoot()) return false;

        return false;
    }

    public virtual bool ShouldBoatFloatAway()
    {
        if (!boatRider.RidingBoat) return false;

        return true;
    }

    public virtual bool ShouldStartAttacking()
    {
        if (attackComponent.IsAttacking) return false;
        if (!healthComponent.IsAlive) return false;

        return true;
    }

    public virtual bool ShouldStopAttacking()
    {
        if (!attackComponent.IsAttacking) return false;

        return true;
    }

    public virtual bool ShouldFollowPath()
    {
        if (boatRider.RidingBoat) return false;
        if (attackComponent.IsAttacking) return false;

        return true;
    }

    // IClickable
    public void Click()
    {
        OnClick();
        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    public virtual bool ShouldClick()
    {
        return true;
    }

    protected virtual void OnClick()
    {

    }

    protected override bool ShouldStartIdle()
    {
        if (movement.IsMoving) return false;
        if (interactComponent.IsInteracting) return false;
        if (boatRider.RidingBoat && boatRider.RidingBoat.Movement.IsMoving) return false;
        if (attackComponent.IsAttacking) return false;
        if (!healthComponent.IsAlive) return false;

        return true;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "name",  nameComponent.GetLocalization()["name"]}
        };
    }

    // Health
    protected virtual void OnRevived()
    {
        DetermineNextAction();
        contextMenuTarget.SetShowContextMenu(true);
        OnHumanRevived?.Invoke(this);
    }

    protected virtual void OnDied()
    {
        DetermineNextAction();
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

    // Interaction Building
    protected virtual void OnInteractBuildingSeted(Building building)
    {
        cityNavigator.SetTargetBuilding(building);
        cityNavigator.TryFindPathToTargetBuilding();
        DetermineNextAction();
    }

    protected virtual void OnInteractBuildingRemoved(Building building)
    {
        cityNavigator.RemoveTargetBuilding();
        cityNavigator.RemovePath();

        DetermineNextAction();
    }

    // Interaction
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

    private void OnBoatMovementStarted(Boat boat)
    {
        UpdateIdle();
    }

    private void OnBoatMovementStopped(Boat boat)
    {
        UpdateIdle();
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

    private IEnumerator DetermineNextActionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        DetermineNextAction();
    }
}