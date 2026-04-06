using System;
using UnityEngine;

public enum HumanStateEnum
{
    Citizen,
    Wanderer,
    Raider
}

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanStateEnum status { get; private set; } = HumanStateEnum.Citizen;
    public float health { get; private set; } = 0f;
    public int interactBuildingInstanceId { get; private set; } = 0;
    public bool isMale { get; private set; } = false;
    public int firstNameIndex { get; private set; } = 0;
    public int lastNameIndex { get; private set; } = 0;
    public int boatInstanceId { get; private set; } = 0;
    public bool isRidingOnBoat { get; private set; } = false;

    public HumanEntry(int id,
        HumanStateEnum status,
        Vector3 position,
        Vector3 rotation,
        float health,
        int boatInstanceId,
        bool isRidingOnBoat) :
        base(id, position, rotation)
    {
        this.status = status;
        this.health = health;
        this.boatInstanceId = boatInstanceId;
        this.isRidingOnBoat = isRidingOnBoat;
    }
}

[Serializable]
public class RaiderEntry : HumanEntry
{
    public bool isFinishedRaiding { get; private set; } = false;

    public RaiderEntry(int id,
        int instanceId,
        HumanStateEnum status,
        Vector3 position,
        Vector3 rotation,
        float health,
        int boatInstanceId,
        bool isFinishedRaiding,
        bool isRidingOnBoat) :
        base(id, status, position, rotation, health, boatInstanceId, isRidingOnBoat)
    {
        this.isFinishedRaiding = isFinishedRaiding;
    }
}

public class Human : Creature
{
    [SerializeField] private Health health;
    public Health Health => health;

    [SerializeField] private EntityCityNavigator cityNavigator;
    public EntityCityNavigator CityNavigator => cityNavigator;

    [SerializeField] private EntityInteractor interactor;
    public EntityInteractor Interactor => interactor;

    [SerializeField] private BoatRider boatRider;
    public BoatRider BoatRider => boatRider;

    [SerializeField] private Attack attack;
    public Attack Attack => attack;

    [SerializeField] private SelectComponent selectComponent;
    public SelectComponent SelectComponent => selectComponent;

    public HumanStateEnum currentStateEnum { get; private set; } = HumanStateEnum.Citizen;
    public HumanState currentState { get; private set; }

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

    protected void OnEnable()
    {
        health.onDeath += Death;
        attack.onStoppedAttacking += OnStoppedAttacking;
        movement.onStopped += OnStoppedMoving;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onExitedBuilding += OnExitedBuilding;
        cityNavigator.onReachedTarget += OnReachedTargetBuilding;

        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactor.onStoppedInteracting += OnStopInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;

        selectComponent.onSelected += OnSelected;
        selectComponent.onDeselected += OnDeselected;

        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    protected void OnDisable()
    {
        health.onDeath -= Death;
        attack.onStoppedAttacking -= OnStoppedAttacking;
        movement.onStopped -= OnStoppedMoving;

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onExitedBuilding -= OnExitedBuilding;
        cityNavigator.onReachedTarget -= OnReachedTargetBuilding;

        interactor.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactor.onStoppedInteracting -= OnStopInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;

        selectComponent.onSelected -= OnSelected;
        selectComponent.onDeselected -= OnDeselected;

        EventBus.onNavMeshBaked -= OnNavMeshBaked;
    }

    private void Update()
    {
        currentState.Tick();
    }

    public override void Init(CreatureEntry data)
    {
        base.Init(data);

        HumanEntry humanData = data as HumanEntry;
        
        SetStatus(humanData.status);
        isMale = humanData.isMale;
        AssignNameIndexes(humanData);

        health.SetCurrentHealth(humanData.health);

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
        currentState.OnSetedInteractBuilding(building);
    }

    public void RemoveInteractBuilding()
    {
        cityNavigator.RemoveTargetBuilding();
        currentState.OnRemovedInteractBuilding();

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
            Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
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
            boatRider.StartMovingToBoat();
        }
    }

    // Wanderer
    public void AcceptWanderer()
    {
        SetStatus(HumanStateEnum.Citizen);
        selectComponent.SetClickable(true);
        selectComponent.SetSelected(false);
        Destroy(boatRider.selectedBoat.gameObject);
        boatRider.ExitBoat();
        onWandererAccepted?.Invoke(this);
    }

    public void RejectWanderer()
    {
        CreaturesManager.instance.UnregisterWanderer(this);
        onWandererRejected?.Invoke(this);
    }

    protected override void OnDeath()
    {
        Debug.Log("OnDeath");
        currentState.OnDeath();
    }

    // Movement
    private void OnStoppedMoving()
    {
        currentState.OnStoppedMoving();
    }

    // Attack
    private void OnStoppedAttacking()
    {
        currentState.OnStoppedAttacking();
    }

    // Entrance
    private void OnEnteredBuilding(Building building)
    {
        building.EnterBuilding(cityNavigator);
        currentState.OnEnteredBuilding(building);
    }

    private void OnExitedBuilding(Building building)
    {
        building.ExitBuilding(cityNavigator);
    }

    private void OnReachedTargetBuilding()
    {

    }

    private void OnRemovedInteractBuilding(Building building)
    {
        cityNavigator.HandleInteractBuildingRemoved();
    }

    private void OnStopInteracting(Building building)
    {

    }

    // Boat
    private void OnEnteredBoat(Boat boat)
    {
        movement.SetAgentEnabled(false);
        currentState.OnEnteredBoat(boat);
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
    public void SetStatus(HumanStateEnum status)
    {
        ExitStatus(currentStateEnum);
        currentStateEnum = status;
        EnterStatus(currentStateEnum);
    }

    private void EnterStatus(HumanStateEnum status)
    {
        switch (status) {
            case HumanStateEnum.Citizen:
                currentState = new CitizenState(this);
                CreaturesManager.instance.RegisterCitizen(this);
                break;
            case HumanStateEnum.Wanderer:
                currentState = new WandererState(this);
                CreaturesManager.instance.RegisterWanderer(this);
                selectComponent.SetClickable(false);
                break;
            case HumanStateEnum.Raider:
                currentState = new RaiderState(this);
                CreaturesManager.instance.RegisterRaider(this);
                break;
        }
    }

    private void ExitStatus(HumanStateEnum status)
    {
        switch (status) {
            case HumanStateEnum.Citizen:
                CreaturesManager.instance.UnregisterCitizen(this);
                break;
            case HumanStateEnum.Wanderer:
                CreaturesManager.instance.UnregisterWanderer(this);
                selectComponent.SetClickable(true);
                break;
            case HumanStateEnum.Raider:
                CreaturesManager.instance.UnregisterRaider(this);
                break;
        }
    }

    private void OnSelected()
    {
        if (currentStateEnum == HumanStateEnum.Wanderer) {
            selectComponent.SetSelected(false);
            SelectComponent boatSelectComponent = boatRider.selectedBoat.SelectComponent;
            boatSelectComponent.SetSelected(true);
            boatSelectComponent.SetClickable(false);
        }
        else {
            onHumanSelected?.Invoke(this);
        }
    }

    private void OnDeselected()
    {
        if (currentStateEnum == HumanStateEnum.Wanderer) {
            SelectComponent boatSelectComponent = boatRider.selectedBoat.SelectComponent;
            boatSelectComponent.SetClickable(true);
            boatSelectComponent.SetSelected(false);
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