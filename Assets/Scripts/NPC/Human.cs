using System;
using UnityEngine;

public enum HumanStatus
{
    Citizen,
    Wanderer,
    Enemy
}

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanEntry(int id, HumanStatus status, Vector3 position, Vector3 rotation) : base(id, position, rotation)
    {
        this.status = status;
    }

    public HumanStatus status { get; private set; } = HumanStatus.Citizen;
    public Guid interactBuildingInstanceId { get; private set; } = Guid.Empty;
    public bool isMale { get; private set; } = false;
    public int firstNameIndex { get; private set; } = 0;
    public int lastNameIndex { get; private set; } = 0;
}

public class Human : Creature
{
    public EntityInteractor interactor { get; private set; } = null;
    public BoatRider boatRider { get; private set; } = null;

    public HumanStatus currentStatus { get; private set; } = HumanStatus.Citizen;

    private bool isMale = false;

    private int firstNameIndex = 0;
    private int lastNameIndex = 0;

    public string firstName { get; private set; } = "";
    public string lastName { get; private set; } = "";

    protected override void Awake()
    {
        base.Awake();

        interactor = GetComponent<EntityInteractor>();
        boatRider = GetComponent<BoatRider>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        movement.onStoppedMoving += OnStoppedMoving;

        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onExitedBuilding += OnExitedBuilding;
        cityNavigator.onReachedTarget += OnReachedTargetBuilding;

        interactor.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactor.onStoppedInteracting += OnStopInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        cityNavigator.onEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.onExitedBuilding -= OnExitedBuilding;

        interactor.onSetedInteractBuilding -= OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding -= OnRemovedInteractBuilding;
        interactor.onStoppedInteracting -= OnStopInteracting;

        boatRider.onEnteredBoat -= OnEnteredBoat;
        boatRider.onExitedBoat -= OnExitedBoat;
    }

    public override void Init(CreatureEntry data)
    {
        base.Init(data);

        HumanEntry humanData = data as HumanEntry;
        SetStatus(humanData.status);
        isMale = humanData.isMale;
        AssignNameIndexes(humanData);

        EventBus.InvokeCitizenInited(this);
    }

    // Movement
    private void OnStoppedMoving()
    {
        if (interactor.InteractBuilding && interactor.InteractBuilding == cityNavigator.currentBuilding) {
            interactor.HandleStoppedMoving();
        }
    }

    private void OnEnteredBuilding(Building building)
    {
        building.EnterBuilding(cityNavigator);
    }

    private void OnExitedBuilding(Building building)
    {
        building.ExitBuilding(cityNavigator);
    }

    private void OnSetedInteractBuilding(Building building)
    {
        cityNavigator.SetTargetBuilding(building);

        if (boatRider.isRidingOnBoat)
            return;

        cityNavigator.TryFindPathToTargetBuilding();
    }

    private void OnRemovedInteractBuilding(Building building)
    {
        cityNavigator.HandleInteractBuildingRemoved();
    }

    private void OnReachedTargetBuilding(Building building)
    {
        //interactor.OnReachedTargetBuilding();
    }

    private void OnStopInteracting(Building building)
    {

    }

    private void OnEnteredBoat(Boat boat)
    {
        movement.SetAgentEnabled(false);

        if (currentStatus == HumanStatus.Citizen) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else if (currentStatus == HumanStatus.Wanderer) {
            boat.SetState(BoatStateEnum.ReturningToDock);
        }
    }

    private void OnExitedBoat(Boat boat)
    {
        movement.SetAgentEnabled(true);

        if (!interactor.InteractBuilding)
            return;

        cityNavigator.TryFindPathToTargetBuilding();
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
    public void SetStatus(HumanStatus status)
    {
        ExitStatus(currentStatus);
        currentStatus = status;
        EnterStatus(currentStatus);
    }

    private void EnterStatus(HumanStatus status)
    {
        switch (status) {
            case HumanStatus.Citizen:
                EntitiesManager.instance.RegisterCitizen(this);
                break;
            case HumanStatus.Wanderer:
                EntitiesManager.instance.RegisterWanderer(this);
                break;
            case HumanStatus.Enemy:
                EntitiesManager.instance.RegisterEnemy(this);
                break;
        }
    }

    private void ExitStatus(HumanStatus status)
    {
        switch (status) {
            case HumanStatus.Citizen:
                EntitiesManager.instance.UnregisterCitizen(this);
                break;
            case HumanStatus.Wanderer:
                EntitiesManager.instance.UnregisterWanderer(this);
                break;
            case HumanStatus.Enemy:
                EntitiesManager.instance.UnregisterEnemy(this);
                break;
        }
    }
}
