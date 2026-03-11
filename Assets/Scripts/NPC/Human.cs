using System;
using UnityEngine;

public enum HumanStatus
{
    Citizen,
    Wanderer,
    Attacker
}

[Serializable]
public class HumanEntry : EntityEntry
{
    public HumanEntry(int id, Vector3 position, Vector3 rotation) : base(id, position, rotation)
    {

    }

    public HumanStatus creatureStatus = HumanStatus.Citizen;
    public Guid interactBuildingInstanceId = Guid.Empty;
    public bool isMale = false;
    public int firstNameIndex = 0;
    public int lastNameIndex = 0;
}

public class Human : Entity
{
    public EntityInteractor interactor { get; private set; } = null;
    public BoatRider boatRider { get; private set; } = null;

    public HumanStatus status { get; private set; } = HumanStatus.Citizen;

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

    public override void Init(EntityEntry data)
    {
        base.Init(data);

        HumanEntry humanData = data as HumanEntry;
        AssignGender(humanData);
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
        cityNavigator.HandleInteractBuildingSeted(building);
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
    }

    private void OnExitedBoat(Boat boat)
    {
        movement.SetAgentEnabled(true);
    }

    // Gender
    private void AssignGender(HumanEntry data)
    {
        if (data != null) {
            isMale = data.isMale;
        }
        else {
            int index = UnityEngine.Random.Range(0, 1);
            if (index == 0)
                isMale = false;
            else
                isMale = true;
        }
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
}
