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

    public static event Action<Human, Building> OnHumanEnteredBuilding;
    public static event Action<Human, Building> OnHumanExitedBuilding;

    public static event Action<Human> OnHumanEnteredBoat;
    public static event Action<Human> OnHumanExitedBoat;

    public static event Action<Human, AttackComponent> OnHumanCombatStarted;
    public static event Action<Human, AttackComponent> OnHumanCombatStopped;

    public static event Action<Human> OnHumanSelected;
    public static event Action<Human> OnHumanDeselected;

    protected override void Awake()
    {
        base.Awake();

        movement.SetAgentEnabled(false);
        movement.NavAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        healthComponent.OnDied += HandleDied;

        reviveComponent.OnRevived += HandleRevived;
        reviveComponent.OnLimitTimeOvered += HandleReviveLimitTimeOvered;

        attackComponent.OnTargetSeted += HandleAttackTargetSeted;
        attackComponent.OnTargetRemoved += HandleAttackTargetRemoved;
        attackComponent.OnAttackStarted += HandleAttackStarted;
        attackComponent.OnAttackStopped += HandleAttackStopped;

        cityNavigator.OnEnteredBuilding += HandleEnteredBuilding;
        cityNavigator.OnExitedBuilding += HandleExitedBuilding;

        interactComponent.OnInteractBuildingSeted += HandleInteractBuildingSeted;
        interactComponent.OnInteractBuildingRemoved += HandleInteractBuildingRemoved;
        interactComponent.OnInteractionStarted += HandleInteractionStarted;
        interactComponent.OnInteractionStopped += HandleInteractionStopped;

        boatRider.OnEnteredBoat += HandleEnteredBoat;
        boatRider.OnExitedBoat += HandleExitedBoat;
        boatRider.OnTargetBoatSeted += HandleTargetBoatSeted;
        boatRider.OnTargetBoatRemoved += HandleTargetBoatRemoved;
        boatRider.OnBoatSetIdle += HandleBoatSetIdle;
        BoatRider.OnBoatMovementStarted += HandleBoatMovementStarted;
        BoatRider.OnBoatMovementStopped += HandleBoatMovementStopped;

        selectComponent.OnSelected += HandleSelected;
        selectComponent.OnDeselected += HandleDeselected;

        RaidManager.Instance.OnRaidStarted += HandleRaidStarted;
        RaidManager.Instance.OnRaidEnded += HandleRaidEnded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        healthComponent.OnDied -= HandleDied;

        reviveComponent.OnRevived -= HandleRevived;
        reviveComponent.OnLimitTimeOvered -= HandleReviveLimitTimeOvered;

        attackComponent.OnTargetSeted -= HandleAttackTargetSeted;
        attackComponent.OnTargetRemoved -= HandleAttackTargetRemoved;
        attackComponent.OnAttackStarted -= HandleAttackStarted;
        attackComponent.OnAttackStopped -= HandleAttackStopped;

        cityNavigator.OnEnteredBuilding -= HandleEnteredBuilding;
        cityNavigator.OnExitedBuilding -= HandleExitedBuilding;

        interactComponent.OnInteractBuildingSeted -= HandleInteractBuildingSeted;
        interactComponent.OnInteractBuildingRemoved -= HandleInteractBuildingRemoved;
        interactComponent.OnInteractionStarted -= HandleInteractionStarted;
        interactComponent.OnInteractionStopped -= HandleInteractionStopped;

        boatRider.OnEnteredBoat -= HandleEnteredBoat;
        boatRider.OnExitedBoat -= HandleExitedBoat;
        boatRider.OnTargetBoatSeted -= HandleTargetBoatSeted;
        boatRider.OnTargetBoatRemoved -= HandleTargetBoatRemoved;
        boatRider.OnBoatSetIdle -= HandleBoatSetIdle;
        BoatRider.OnBoatMovementStarted -= HandleBoatMovementStarted;
        BoatRider.OnBoatMovementStopped -= HandleBoatMovementStopped;

        selectComponent.OnSelected -= HandleSelected;
        selectComponent.OnDeselected -= HandleDeselected;

        if (RaidManager.Instance) {
            RaidManager.Instance.OnRaidStarted -= HandleRaidStarted;
            RaidManager.Instance.OnRaidEnded -= HandleRaidEnded;
        }
    }

    public override void Tick()
    {
        base.Tick();
    }

    protected override void HandleInit(CreatureData creatureData)
    {
        base.HandleInit(creatureData);

        var humanData = creatureData as HumanData;

        if (humanData == null) {
            Debug.LogError("humanData is not valid");
            humanData = HumanData.Default();
        }

        nameComponent.Init(humanData.Name);
        skillsComponent.Init(humanData.Skills);
        healthComponent.Init(humanData.Health);
        reviveComponent.Init(humanData.Revive);
        weaponComponent.Init(humanData.Weapon);
        interactComponent.Init(humanData.Interaction);
        cityNavigator.Init(humanData.CityNavigator);
        boatRider.Init(humanData.BoatRider);

        OnHumanInited?.Invoke(this);
    }

    protected override void HandleInitNextFrame()
    {
        if (elevatorPassenger != null && !elevatorPassenger.IsRiding && (boatRider == null || boatRider.RidingBoat == null)) {
            movement.SetAgentEnabled(true);
            cityNavigator.FollowPath();
        }

        base.HandleInitNextFrame();
    }

    protected override CreatureData GetDefaultData()
    {
        return HumanData.Default();
    }

    // Action
    protected override void DetermineNextAction()
    {
        if (ShouldStartInteracting()) {
            //Debug.Log($"{name} ShouldStartInteracting");
            StartInteracting();
            return;
        }
        if (ShouldStopInteracting()) {
            //Debug.Log($"{name} ShouldStopInteracting");
            StopInteracting();
            return;
        }
        if (ShouldMoveToTargetBoat()) {
            //Debug.Log($"{name} ShouldMoveToTargetBoat");
            MoveToTargetBoat();
            return;
        }
        if (ShouldWaitForEnteringBoat()) {
            //Debug.Log($"{name} ShouldStartEnteringBoat");
            StartEnteringBoat();
            return;
        }
        if (ShouldStopEnteringBoat()) {
            //Debug.Log($"{name} ShouldStopEnteringBoat");
            StopEnteringBoat();
            return;
        }
        if (ShouldStartExitingBoat()) {
            //Debug.Log($"{name} ShouldStartExitingBoat");
            StartExitingBoat();
            return;
        }
        if (ShouldStopExitingBoat()) {
            //Debug.Log($"{name} ShouldStopExitingBoat");
            StopExitingBoat();
            return;
        }
        if (ShouldBoatMoveToDock()) {
            //Debug.Log($"{name} ShouldBoatMoveToDock");
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFindLoot()) {
            //Debug.Log($"{name} ShouldBoatFindLoot");
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            //Debug.Log($"{name} ShouldBoatFloatAway");
            BoatFloatAway();
            return;
        }
        if (ShouldSetCombatTarget()) {
            //Debug.Log($"{name} ShouldStartAttacking");
            SetCombatTarget();
            return;
        }
        if (ShouldStopAttacking()) {
            //Debug.Log($"{name} ShouldStopAttacking");
            StopAttacking();
            return;
        }
        if (ShouldFollowPath()) {
            //Debug.Log($"{name} ShouldFollowPath");
            FollowPath();
            return;
        }

        base.DetermineNextAction();
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
        var position = boatRider.TargetBoat.DockPoint.EntraceTransform.position;
        movement.TryMoveTo(position);

        UpdateIdle();
    }

    protected virtual void SetCombatTarget()
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

    protected override bool ShouldStartIdle()
    {
        if (!base.ShouldStartIdle()) return false;

        //if (movement != null && movement.IsMoving) return false;
        if (CityNavigator.IsFollowingPath && movement.IsMoving) return false;
        if (interactComponent != null && interactComponent.IsInteracting) return false;
        if (boatRider != null && boatRider.RidingBoat != null && boatRider.RidingBoat.Movement != null && boatRider.RidingBoat.Movement.IsMoving) return false;
        if (attackComponent != null && attackComponent.IsAttacking) return false;
        if (healthComponent != null && !healthComponent.IsAlive) return false;

        return true;
    }

    public virtual bool ShouldStartInteracting()
    {
        //Debug.Log($"ShouldStartInteracting {this}");
        if (interactComponent != null && interactComponent.IsInteracting) return false;

        //Debug.Log($"ShouldStartInteracting1 {this} {interactComponent.InteractBuilding}");
        var interactBuilding = interactComponent != null ? interactComponent.InteractBuilding : null;
        if (interactBuilding == null) return false;

        var currentBuilding = cityNavigator.CurrentBuilding;
        //Debug.Log($"ShouldStartInteracting2 {this} {currentBuilding}");
        if (currentBuilding != interactBuilding) return false;

        //Debug.Log($"ShouldStartInteracting3 {this}");
        if (interactBuilding.GetComponent<PierModule>() != null) return false;

        //Debug.Log($"ShouldStartInteracting4 {this}");
        if (currentBuilding == null) return false;

        //Debug.Log($"ShouldStartInteracting5 {this}");
        if (boatRider != null && boatRider.RidingBoat != null) return false;

        //Debug.Log($"ShouldStartInteracting6 {this}");
        if (healthComponent != null && !healthComponent.IsAlive) return false;

        //Debug.Log($"ShouldStartInteracting7 {this}");
        if (attackComponent != null && attackComponent.IsAttacking) return false;

        //Debug.Log($"ShouldStartInteracting8 {this}");
        var waypoint = cityNavigator != null && cityNavigator.WaypointsComponent != null ? cityNavigator.WaypointsComponent.GetCurrentWaypoint() : null;
        if (waypoint == null || waypoint.Transform == null) {
            Debug.LogError("waypoint or its transform is not valid", this);
            return false;
        }

        //Debug.Log($"ShouldStartInteracting9 {this}");
        //Debug.Log(movement.GetDistanceToPosition(waypoint.Transform.position));
        if (!movement.IsReachedPosition(waypoint.Transform.position)) return false;

        //Debug.Log($"ShouldStartInteracting10 {this}");
        return true;
    }

    public virtual bool ShouldStopInteracting()
    {
        if (interactComponent != null && !interactComponent.IsInteracting) return false;
        if (interactComponent != null && interactComponent.InteractBuilding == null) return false;
        if (healthComponent != null && !healthComponent.IsAlive) return true;
        if (attackComponent != null && attackComponent.IsAttacking) return true;

        return false;
    }

    public virtual bool ShouldMoveToTargetBoat()
    {
        if (boatRider == null) return false;
        if (cityNavigator == null) return false;
        if (attackComponent == null) return false;

        if (boatRider.RidingBoat != null) return false;
        if (cityNavigator.FloorIndex > 0) return false;
        if (attackComponent.IsAttacking) return false;

        var buildingsManager = BuildingsManager.Instance;
        if (buildingsManager == null) return false;

        var path = new List<Building>();
        if (!cityNavigator.TryFindPathToBuilding(buildingsManager.EntranceBuildingPlace.PlacedBuilding, out path)) return false;

        var targetBoat = boatRider != null ? boatRider.TargetBoat : null;
        if (targetBoat == null) return false;

        var dockPoint = targetBoat.DockPoint;
        if (dockPoint == null) {
            Debug.LogError($"[{nameof(Human)}] Target Boat Dock is not valid at {targetBoat}!");
            return false;
        }

        var entranceTransform = dockPoint.EntraceTransform;
        if (entranceTransform == null) {
            Debug.LogError($"[{nameof(Human)}] Entrance Transform is not valid at {dockPoint}!");
            return false;
        }

        if (movement.IsReachedPosition(entranceTransform.position)) return false;

        return true;
    }

    public virtual bool ShouldWaitForEnteringBoat()
    {
        var targetBoat = boatRider != null ? boatRider.TargetBoat : null;
        if (targetBoat == null) return false;
        if (targetBoat.DockPoint == null) return false;
        if (targetBoat.DockPoint.EntraceTransform == null) return false;
        if (!movement.IsReachedPosition(targetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    public virtual bool ShouldStopEnteringBoat()
    {
        if (boatRider != null && boatRider.TargetBoat != null) return false;
        if (boatRider != null && !boatRider.IsEnteringBoat) return false;

        return true;
    }

    public virtual bool ShouldStartExitingBoat()
    {
        //Debug.Log("ShouldStartExitingBoat1");
        var ridingBoat = boatRider.RidingBoat;
        if (ridingBoat == null) return false;

        //Debug.Log("ShouldStartExitingBoat2");
        var targetBoat = boatRider.TargetBoat;
        if (targetBoat != null && targetBoat == ridingBoat) return false;

        //Debug.Log("ShouldStartExitingBoat3");
        var stateEnum = ridingBoat.CurrentStateEnum;
        if (stateEnum != BoatStateEnum.Idle) return false;

        //Debug.Log("ShouldStartExitingBoat4");
        var dockPoint = ridingBoat.DockPoint;
        if (dockPoint == null) return false;

        //Debug.Log("ShouldStartExitingBoat5");
        var dockTransform = dockPoint.DockTransform;
        if (dockTransform == null) return false;

        //Debug.Log("ShouldStartExitingBoat6");
        if (ridingBoat.Movement != null && !ridingBoat.Movement.IsReachedPosition(dockTransform.position)) return false;

        //Debug.Log("ShouldStartExitingBoat7");
        return true;
    }

    public virtual bool ShouldStopExitingBoat()
    {
        var ridingBoat = boatRider != null ? boatRider.RidingBoat : null;
        if (ridingBoat == null) return false;
        if (boatRider != null && !boatRider.IsExitingBoat) return false;
        if (ridingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;

        return true;
    }

    public virtual bool ShouldBoatMoveToDock()
    {
        var ridingBoat = boatRider != null ? boatRider.RidingBoat : null;
        if (ridingBoat == null) return false;

        var dockPoint = ridingBoat.DockPoint;
        if (dockPoint == null) return false;

        var boatState = ridingBoat.CurrentStateEnum;
        if (boatState == BoatStateEnum.MovingToDock) return false;
        if (boatState == BoatStateEnum.Idle && ridingBoat.Movement != null && ridingBoat.Movement.IsReachedPosition(dockPoint.DockTransform.position)) return false;

        //if (boatRider != null && boatRider.IsExitingBoat) return false;

        return true;
    }

    public virtual bool ShouldBoatFindLoot()
    {
        var ridingBoat = boatRider != null ? boatRider.RidingBoat : null;
        if (ridingBoat == null) return false;
        if (!ridingBoat.ShouldFindLoot()) return false;

        return true;
    }

    public virtual bool ShouldBoatFloatAway()
    {
        var ridingBoat = boatRider != null ? boatRider.RidingBoat : null;
        if (ridingBoat == null) return false;

        return true;
    }

    public virtual bool ShouldSetCombatTarget()
    {
        if (attackComponent != null && attackComponent.IsAttacking) return false;
        if (healthComponent != null && !healthComponent.IsAlive) return false;

        return false;
    }

    public virtual bool ShouldStopAttacking()
    {
        if (attackComponent != null && !attackComponent.IsAttacking) return false;

        return true;
    }

    public virtual bool ShouldFollowPath()
    {
        if (cityNavigator == null) return false;
        //Debug.Log("ShouldFollowPath");
        if (cityNavigator.TargetBuilding == null) return false;
        if (!cityNavigator.HasPath) return false;
        //Debug.Log("ShouldFollowPath1");
        if (healthComponent != null && !healthComponent.IsAlive) return false;
        //Debug.Log("ShouldFollowPath2");
        if (boatRider != null && boatRider.RidingBoat != null) return false;
        //Debug.Log("ShouldFollowPath3");
        if (attackComponent != null && attackComponent.IsAttacking) return false;
        //Debug.Log("ShouldFollowPath4");
        if (attackComponent != null && attackComponent.CurrentTarget != null) return false;

        //Debug.Log("ShouldFollowPath5");
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

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "name",  nameComponent != null ? nameComponent.GetLocalization()["name"] : string.Empty }
        };
    }

    // Health
    protected virtual void HandleRevived()
    {
        RunDetermineNextActionCoroutine();
        if (contextMenuTarget != null)
            contextMenuTarget.SetShowContextMenu(true);

        OnHumanRevived?.Invoke(this);
    }

    protected virtual void HandleDied()
    {
        RunDetermineNextActionCoroutine();
        if (contextMenuTarget != null)
            contextMenuTarget.SetShowContextMenu(false);

        OnHumanDied?.Invoke(this);
    }

    // Revive
    private void HandleReviveLimitTimeOvered()
    {
        Destroy(gameObject);
    }

    // Movement
    protected override void OnMovementStopped()
    {
        base.OnMovementStopped();

        RunDetermineNextActionCoroutine();
    }

    // Attack
    protected virtual void HandleAttackTargetSeted(AttackComponent combatComponent)
    {
        RunDetermineNextActionCoroutine();
    }

    protected virtual void HandleAttackTargetRemoved(AttackComponent combatComponent)
    {
        RunDetermineNextActionCoroutine();
    }

    protected virtual void HandleAttackStarted(AttackComponent combatComponent)
    {
        RunDetermineNextActionCoroutine();
        OnHumanCombatStarted?.Invoke(this, combatComponent);
    }

    protected virtual void HandleAttackStopped(AttackComponent combatComponent)
    {
        RunDetermineNextActionCoroutine();
        OnHumanCombatStopped?.Invoke(this, combatComponent);
    }

    // Entrance
    protected virtual void HandleEnteredBuilding(Building building)
    {
        if (building == null) return;

        RunDetermineNextActionCoroutine();
        OnHumanEnteredBuilding?.Invoke(this, building);
    }

    protected virtual void HandleExitedBuilding(Building building)
    {
        if (building == null) return;

        RunDetermineNextActionCoroutine();
        OnHumanExitedBuilding?.Invoke(this, building);
    }

    // Interaction Building
    protected virtual void HandleInteractBuildingSeted(Building building)
    {
        if (building == null) return;

        if (cityNavigator != null) {
            cityNavigator.SetTargetBuilding(building);
            cityNavigator.TryUpdatePathToTargetBuilding();
        }
        RunDetermineNextActionCoroutine();
    }

    protected virtual void HandleInteractBuildingRemoved(Building building)
    {
        if (building == null) return;

        if (cityNavigator != null) {
            cityNavigator.RemoveTargetBuilding();
            cityNavigator.RemovePathAndTargetBuilding();
        }
        RunDetermineNextActionCoroutine();
    }

    // Interaction
    protected virtual void HandleInteractionStarted(Building building)
    {
        if (building == null) return;

        RunDetermineNextActionCoroutine();
    }

    protected virtual void HandleInteractionStopped(Building building)
    {
        if (building == null) return;

        RunDetermineNextActionCoroutine();
    }

    // Boat
    protected virtual void HandleEnteredBoat(Boat boat)
    {
        if (boat == null) return;

        RunDetermineNextActionCoroutine();
        OnHumanEnteredBoat?.Invoke(this);
    }

    protected virtual void HandleExitedBoat(Boat boat)
    {
        if (boat == null) return;

        RunDetermineNextActionCoroutine();
        OnHumanExitedBoat?.Invoke(this);
    }

    protected virtual void HandleBoatSetIdle(Boat boat)
    {
        RunDetermineNextActionCoroutine();
    }

    private void HandleTargetBoatSeted(Boat boat)
    {
        var interactBuilding = interactComponent != null ? interactComponent.InteractBuilding : null;
        if (interactBuilding != null && interactBuilding.GetComponent<PierModule>() == null) {
            if (interactComponent != null) {
                interactComponent.RemoveInteractBuilding();
                interactComponent.TryStopInteracting(interactBuilding);
            }
        }

        if (cityNavigator != null && cityNavigator.CurrentBuilding != null && cityNavigator.CurrentBuilding is TowerBuilding) {
            cityNavigator.SetTargetBuilding(BuildingsManager.Instance.TowerGate);

            if (cityNavigator.TryUpdatePathToTargetBuilding()) {
                cityNavigator.FollowPath();
            }
        }

        RunDetermineNextActionCoroutine();
    }

    private void HandleTargetBoatRemoved(Boat boat)
    {
        RunDetermineNextActionCoroutine();
    }

    private void HandleBoatMovementStarted(Boat boat)
    {
        UpdateIdle();
    }

    private void HandleBoatMovementStopped(Boat boat)
    {
        UpdateIdle();
    }

    // Raid
    private void HandleRaidStarted()
    {
        if (movement != null)
            movement.SetMovementMethod(MovementMethod.Run);
    }

    private void HandleRaidEnded(RaidEndedResult result)
    {
        if (movement != null)
            movement.SetMovementMethod(MovementMethod.Walk);
    }

    private void HandleSelected()
    {
        OnHumanSelected?.Invoke(this);
    }

    private void HandleDeselected()
    {
        OnHumanDeselected?.Invoke(this);
    }
}