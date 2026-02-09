using System;
using Unity.Mathematics;
using UnityEngine;

public enum HumanStatus
{
    Citizen,
    Wanderer,
    Attacker
}

[Serializable]
public class HumanEntry : CreatureEntry
{
    public HumanStatus creatureStatus = HumanStatus.Citizen;
    public Guid interactBuildingInstanceId = Guid.Empty;
    public bool isMale = false;
    public int firstNameIndex = 0;
    public int lastNameIndex = 0;
}

public class Human : Entity
{
    public EntityCityNavigator cityNavigator { get; private set; } = null;
    public EntityInteractor interactor { get; private set; } = null;
    public BoatRider boatRider { get; private set; } = null;

    public HumanStatus status { get; private set; } = HumanStatus.Citizen;

    private bool isMale = false;

    private int firstNameIndex = 0;
    private int lastNameIndex = 0;

    public string firstName { get; private set; } = "";
    public string lastName { get; private set; } = "";

    private void Awake()
    {
        cityNavigator = GetComponent<EntityCityNavigator>();
        interactor = GetComponent<EntityInteractor>();
        boatRider = GetComponent<BoatRider>();
    }

    private void OnEnable()
    {
        cityNavigator.onEnteredBuilding += OnEnteredBuilding;
        cityNavigator.onExitedBuilding += OnExitedBuilding;
        cityNavigator.onReachedTarget += OnReachedTarget;

        interactor.onSetedInteractBuilding += OnSetedInteractBuilding;
        interactor.onRemovedInteractBuilding += OnRemovedInteractBuilding;
        interactor.onStoppedInteracting += OnStopInteracting;

        boatRider.onEnteredBoat += OnEnteredBoat;
        boatRider.onExitedBoat += OnExitedBoat;
    }

    private void OnDisable()
    {
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
        AssignGender(humanData);
        AssignNameIndexes(humanData);
    }

    private void OnEnteredBuilding(Building building)
    {
        building.EnterBuilding(this);
    }

    private void OnExitedBuilding(Building building)
    {
        building.ExitBuilding(this);
    }

    private void OnSetedInteractBuilding(Building building)
    {
        building.AddWorker(this);
        cityNavigator.OnSetedInteractBuilding(building);
    }

    private void OnRemovedInteractBuilding(Building building)
    {
        building.RemoveWorker(this);
        cityNavigator.OnRemovedInteractBuilding();
    }

    private void OnReachedTarget(Building building)
    {
        interactor.StartInteractingBuilding();
        building.AddCurrentWorker(this);

        PierModule pier = building.GetComponent<PierModule>();
        if (pier) {
            boatRider.SetBoat(CityManager.Instance.citizenBoats[interactor.interacterIndex]);
            boatRider.StartEnteringBoat();
        }
    }

    private void OnStopInteracting(Building building)
    {
        building.RemoveCurrentWorker(this);

        PierModule pier = building.GetComponent<PierModule>();
        if (pier) {
            boatRider.SetBoat(null);
            boatRider.StartExitingBoat();
        }
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
                firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_first_names.Length);
                lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.male_last_names.Length);
            }
            else {
                firstNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_first_names.Length);
                lastNameIndex = UnityEngine.Random.Range(0, LocalizationManager.Instance.currentLocalization.female_last_names.Length);
            }
        }

        firstName = LocalizationManager.Instance.GetFirstName(isMale, firstNameIndex);
        lastName = LocalizationManager.Instance.GetLastName(isMale, lastNameIndex);
    }
}
