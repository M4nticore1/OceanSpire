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

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanStatusEnum status { get; private set; } = HumanStatusEnum.Citizen;
    public float health { get; private set; } = 0f;
    public WeaponHandlerData weaponData { get; private set; }
    public SkillsData skills { get; private set; }
    public int interactBuildingInstanceId { get; private set; } = 0;
    public bool isMale { get; private set; } = false;
    public int firstNameIndex { get; private set; } = 0;
    public int lastNameIndex { get; private set; } = 0;
    public int boatInstanceId { get; private set; } = 0;
    public bool isRidingOnBoat { get; private set; } = false;

    public HumanEntry(int id,
        HumanStatusEnum status,
        Vector3 position,
        Vector3 rotation,
        float health,
        WeaponHandlerData weaponData,
        SkillsData skills,
        int boatInstanceId,
        bool isRidingOnBoat) :
        base(id, position, rotation)
    {
        this.status = status;
        this.health = health;
        this.weaponData = weaponData;
        this.skills = skills;
        this.boatInstanceId = boatInstanceId;
        this.isRidingOnBoat = isRidingOnBoat;
    }
}

public class Human : Creature
{
    [SerializeField] private Health health;
    public Health Health => health;

    [SerializeField] private CreatureCityNavigator cityNavigator;
    public CreatureCityNavigator CityNavigator => cityNavigator;

    [SerializeField] private CreatureInteractor interactor;
    public CreatureInteractor Interactor => interactor;

    [SerializeField] private BoatRider boatRider;
    public BoatRider BoatRider => boatRider;

    [SerializeField] private Attack attack;
    public Attack Attack => attack;

    [SerializeField] private WeaponHandler weaponHandler;
    public WeaponHandler WeaponHandler => WeaponHandler;

    [SerializeField] private SkillsComponent skills;
    public SkillsComponent Skills => skills;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    [SerializeField] private float maxTimeToRevive = 60f;
    public float MaxDeadTime => maxTimeToRevive;
    public float currentDeadTime { get; private set; } = 0f;

    [Header("Status")]
    [SerializeField] private GameObject citizenClothes;
    [SerializeField] private GameObject wandererClothes;
    [SerializeField] private GameObject raiderClothes;

    public HumanStatusEnum currentStatusEnum { get; private set; } = HumanStatusEnum.Citizen;
    public HumanState currentStatus { get; private set; }

    private bool isMale = false;

    private int firstNameIndex = 0;
    private int lastNameIndex = 0;

    public string firstName { get; private set; }
    public string lastName { get; private set; }

    public static event Action<Human> onWandererAccepted;
    public static event Action<Human> onWandererRejected;
    public static event Action<Human> onHumanSelected;
    public static event Action<Human> onHumanDeselected;
    public static event Action<Human> onEnteredBoat;
    public static event Action<Human> onExitedBoat;

    protected override void OnEnable()
    {
        base.OnEnable();

        health.onRevived += OnRevived;
        health.onDied += OnDied;

        attack.onStoppedAttacking += OnStoppedAttacking;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onReachedPath += OnReachedPathBuilding;

        interactor.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;

        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        currentStatus.OnDisable();

        health.onRevived -= OnRevived;
        health.onDied -= OnDied;

        attack.onStoppedAttacking -= OnStoppedAttacking;

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onReachedPath -= OnReachedPathBuilding;

        interactor.onSetedInteractBuilding -= OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding -= OnRemovedInteractBuilding;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        EventBus.onNavMeshBaked -= OnNavMeshBaked;
    }

    private void Update()
    {
        currentStatus.Tick();

        ReviveCitizenAdReward reviveReward = RewardedAdsManager.instance.currentReward as ReviveCitizenAdReward;
        bool isAdDisplayed = RewardedAdsManager.instance.AdsManager.isAdDisplayed;

        if (!health.isAlive && reviveReward == null && !isAdDisplayed) {
            currentDeadTime += Time.deltaTime;

            if (currentDeadTime >= maxTimeToRevive) {
                Destroy(gameObject);
            }
        }
    }

    protected override void OnInit(CreatureEntry data)
    {
        HumanEntry humanData = data as HumanEntry;

        HideClothes();
        SetStatus(humanData.status);
        isMale = humanData.isMale;
        AssignNameIndexes(humanData);

        health.SetCurrentHealth(humanData.health);
        weaponHandler.Init(humanData.weaponData);
        skills.Init(humanData.skills);

        if (humanData.boatInstanceId >= 0) {
            boatRider.SetSelectedBoat(humanData.boatInstanceId);
        }

        if (humanData.isRidingOnBoat) {
            boatRider.EnterBoat();
        }

        EventBus.InvokeCitizenInited(this);
    }

    public void SetInteractBuilding(Building building)
    {
        if (interactor.interactBuilding) {
            RemoveInteractBuilding();
        }

        interactor.SetInteractBuilding(building);
        cityNavigator.SetTargetBuilding(building);
        currentStatus.OnSetedInteractBuilding(building);
    }

    public void RemoveInteractBuilding()
    {
        cityNavigator.RemoveTargetBuilding();
        currentStatus.OnRemovedInteractBuilding();

        if (boatRider.isRidingOnBoat) {
            BoatRider.selectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            cityNavigator.UpdateFollowingPathState();
        }
    }

    public void HandleClickedWorkerWidget()
    {
        if (interactor.interactBuilding) {
            RemoveInteractBuilding();
        }
        else {
            Building building = SelectManager.Instance.GetSelectedBuilding();
            if (building.workers.Count >= building.LevelData.maxResidentsCount) return;

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
        CreaturesManager.instance.UnregisterWanderer(this);
        onWandererRejected?.Invoke(this);
    }

    protected override bool ShouldStartIdle()
    {
        if (movement.isMoving) return false;
        if (interactor.isInteracting) return false;
        if (attack.isAttacking) return false;
        if (!health.isAlive) return false;

        return true;
    }

    // Health
    private void OnRevived()
    {
        currentDeadTime = 0f;
        currentStatus.OnDied();
    }

    private void OnDied()
    {
        currentStatus.OnDied();
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
    private void OnStoppedAttacking()
    {
        currentStatus.OnStoppedAttacking();
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

        if (interactor.interactBuilding) {
            cityNavigator.TryFindPathToTargetBuilding();
        }

        onExitedBoat?.Invoke(this);
    }

    // Names
    private void AssignNameIndexes(HumanEntry data)
    {
        if (data != null) {
            firstNameIndex = data.firstNameIndex;
            lastNameIndex = data.lastNameIndex;
        }
        else {
            if (isMale) {
                //firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_first_names.Length);
                //lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_last_names.Length);
            }
            else {
                //firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_first_names.Length);
                //lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_last_names.Length);
            }
            firstNameIndex = 0;
            lastNameIndex = 0;
        }

        firstName = LocalizationManager.Instance.GetFirstName(isMale, firstNameIndex);
        lastName = LocalizationManager.Instance.GetLastName(isMale, lastNameIndex);
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