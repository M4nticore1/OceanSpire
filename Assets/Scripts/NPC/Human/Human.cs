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

public class Human : Creature
{
    [SerializeField] private NameHandler nameHandler;
    public NameHandler NameHandler => nameHandler;

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

    [SerializeField] private WeaponEquipment weaponEquipment;
    public WeaponEquipment WeaponEquipment => weaponEquipment;

    [SerializeField] private SkillsComponent skillsComponent;
    public SkillsComponent SkillsComponent => skillsComponent;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    [SerializeField] private ContextMenuTarget contextMenuTarget;
    public ContextMenuTarget ContextMenuTarget => contextMenuTarget;

    [Header("Status")]
    [SerializeField] private GameObject citizenClothes;
    [SerializeField] private GameObject wandererClothes;
    [SerializeField] private GameObject raiderClothes;

    public HumanStatusEnum currentStatusEnum { get; private set; } = HumanStatusEnum.Citizen;
    public HumanState currentStatus { get; private set; }

    private bool isMale = true;

    public static event Action<Human> onHumanRevived;
    public static event Action<Human> onHumanDied;
    public static event Action<Human> onWandererAccepted;
    public static event Action<Human> onWandererRejected;
    public static event Action<Human> onHumanSelected;
    public static event Action<Human> onHumanDeselected;
    public static event Action<Human> onEnteredBoat;
    public static event Action<Human> onExitedBoat;

    protected override void OnEnable()
    {
        base.OnEnable();

        healthComponent.onRevived += OnRevived;
        healthComponent.onDied += OnDied;

        attackComponent.onStartedAttacking += OnStartedAttacking;
        attackComponent.onStoppedAttacking += OnStoppedAttacking;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onReachedPath += OnReachedPathBuilding;

        interactComponent.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactComponent.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactComponent.onStartedInteracting += OnStartedInteracting;
        interactComponent.onStoppedInteracting += OnStoppedInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;

        RaidManager.Instance.onRaidStarted += OnRaidStarted;
        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        currentStatus.Exit();

        healthComponent.onRevived -= OnRevived;
        healthComponent.onDied -= OnDied;

        attackComponent.onStartedAttacking -= OnStartedAttacking;
        attackComponent.onStoppedAttacking -= OnStoppedAttacking;

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onReachedPath -= OnReachedPathBuilding;

        interactComponent.onSetedInteractBuilding -= OnSetedInteractBuilding;
        interactComponent.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactComponent.onStartedInteracting -= OnStartedInteracting;
        interactComponent.onStoppedInteracting -= OnStoppedInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        RaidManager.Instance.onRaidStarted += OnRaidStarted;
        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        EventBus.onNavMeshBaked -= OnNavMeshBaked;
    }

    private void Update()
    {
        currentStatus.Tick();
    }

    protected override void OnInit(CreatureDataV1 data)
    {
        HumanDataV1 humanData = data as HumanDataV1;

        HideClothes();
        isMale = humanData.isMale;
        SetStatus(humanData.status);

        healthComponent.SetCurrentHealth(humanData.health);
        nameHandler.Init(humanData.name, isMale);

        if (InstancesManager.instance.TryGetInstance(humanData.interactBuildingInstanceId, out var obj)) {
            if (obj.TryGetComponent<Building>(out var building)) {
                interactComponent.SetInteractBuilding(building);
            }
        }

        weaponEquipment.Init(humanData.weapon);
        skillsComponent.Init(humanData.skills);

        if (humanData.boatRider.boatInstanceId >= 0) {
            boatRider.SetSelectedBoat(humanData.boatRider.boatInstanceId);
        }

        if (humanData.boatRider.isRiding) {
            boatRider.EnterBoat();
        }

        EventBus.InvokeCitizenInited(this);
    }

    public void SetInteractBuilding(Building building)
    {
        if (interactComponent.interactBuilding) {
            RemoveInteractBuilding();
        }

        interactComponent.SetInteractBuilding(building);
        cityNavigator.SetTargetBuilding(building);
        currentStatus.OnSetedInteractBuilding(building);
    }

    public void RemoveInteractBuilding()
    {
        cityNavigator.RemoveTargetBuilding();
        currentStatus.OnRemovedInteractBuilding();
        interactComponent.RemoveInteractBuilding();

        if (boatRider.isRidingOnBoat) {
            BoatRider.selectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            cityNavigator.UpdateFollowingPathState();
        }
    }

    public void HandleClickedWorkerWidget()
    {
        if (interactComponent.interactBuilding) {
            RemoveInteractBuilding();
        }
        else {
            Building building = SelectManager.Instance.GetSelectedBuilding();
            if (building.WorkComponent.Workers.Count >= building.LevelData.maxResidentsCount) return;

            SetInteractBuilding(building);
        }
    }

    // Boat
    public void MoveToBoat()
    {
        if (cityNavigator.floorIndex > 0) {
            Building building = BuildingsManager.instance.TowerGate;
            cityNavigator.SetTargetBuilding(building);
            cityNavigator.TryFindPathToTargetBuilding();
        }
        else {
            Vector3 position = boatRider.selectedBoat.dockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }

        boatRider.StartMovingToBoat();
    }

    // Wanderer
    public void AcceptWanderer()
    {
        SetStatus(HumanStatusEnum.Citizen);
        selectComponent.SetClickable(true);
        selectComponent.Deselect();
        Destroy(boatRider.selectedBoat.gameObject);
        boatRider.ExitBoat();
        onWandererAccepted?.Invoke(this);
    }

    public void RejectWanderer()
    {
        CreaturesManager.Instance.UnregisterWanderer(this);
        onWandererRejected?.Invoke(this);
    }

    protected override bool ShouldStartIdle()
    {
        if (movement.isMoving) return false;
        if (interactComponent.isInteracting) return false;
        if (attackComponent.isAttacking) return false;
        if (!healthComponent.isAlive) return false;

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

    // Movement
    protected override void OnStoppedMoving()
    {
        base.OnStoppedMoving();

        currentStatus.OnStoppedMoving();

        if (boatRider.isMovingToBoat && cityNavigator.floorIndex == 0 && movement.NavAgent.remainingDistance <= movement.NavAgent.stoppingDistance) {
            boatRider.StartEnteringBoat();
            boatRider.StopMovingToBoat();
        }
    }

    // Attack
    private void OnStartedAttacking()
    {
        currentStatus.OnStartedAttacking();
        TryStopIdle();
    }

    private void OnStoppedAttacking()
    {
        currentStatus.OnStoppedAttacking();
        TryStartIdle();
    }

    // Entrance
    private void OnEnteredBuilding(Building building)
    {
        currentStatus.OnEnteredBuilding(building);

        if (boatRider.isMovingToBoat && building == cityNavigator.targetBuilding) {
            Vector3 position = boatRider.selectedBoat.dockPoint.EntraceTransform.position;
            movement.TryMoveTo(position);
        }
    }

    private void OnReachedPathBuilding()
    {

    }

    private void OnSetedInteractBuilding()
    {

    }

    private void OnRemovedInteractBuilding()
    {
        cityNavigator.HandleInteractBuildingRemoved();
    }

    private void OnStartedInteracting()
    {
        currentStatus.OnStartedInteracting();
    }

    private void OnStoppedInteracting()
    {
        currentStatus.OnStoppedInteracting();
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

        if (interactComponent.interactBuilding) {
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
                citizenClothes.SetActive(true);
                break;
            case HumanStatusEnum.Wanderer:
                currentStatus = new WandererState(this);
                wandererClothes.SetActive(true);
                break;
            case HumanStatusEnum.Raider:
                currentStatus = new RaiderState(this);
                raiderClothes.SetActive(true);
                break;
        }

        currentStatus.Enter();
    }

    private void ExitStatus(HumanStatusEnum status)
    {
        switch (status) {
            case HumanStatusEnum.Citizen:
                citizenClothes.SetActive(false);
                break;
            case HumanStatusEnum.Wanderer:
                wandererClothes.SetActive(false);
                break;
            case HumanStatusEnum.Raider:
                raiderClothes.SetActive(false);
                break;
        }

        if (currentStatus != null) {
            currentStatus.Exit();
        }
    }

    private void HideClothes()
    {
        citizenClothes.SetActive(false);
        wandererClothes.SetActive(false);
        raiderClothes.SetActive(false);
    }

    private void OnSelected()
    {
        if (currentStatusEnum == HumanStatusEnum.Wanderer) {
            selectComponent.Deselect();
            SelectComponent boatSelectComponent = boatRider.selectedBoat.SelectComponent;
            boatSelectComponent.Select();
            boatSelectComponent.SetClickable(false);
        }
        else {
            onHumanSelected?.Invoke(this);
        }
    }

    private void OnDeselected()
    {
        if (currentStatusEnum == HumanStatusEnum.Wanderer) {
            SelectComponent boatSelectComponent = boatRider.selectedBoat.SelectComponent;
            boatSelectComponent.SetClickable(true);
            boatSelectComponent.Deselect();
        }
        else {
            onHumanDeselected?.Invoke(this);
        }
    }

    private void OnNavMeshBaked()
    {
        if (cityNavigator.IsRidingOnElevator) return;

        movement.SetAgentEnabled(true);
    }
}