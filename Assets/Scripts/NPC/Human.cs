using System;
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
    HumanStatus creatureStatus = HumanStatus.Citizen;
    Guid interactBuildingInstanceId;
}

public class Human : Creature
{
    private EntityCityNavigator cityNavigator = null;
    private EntityInteractor interactor = null;
    private BoatRider boatRider = null;

    public HumanStatus status { get; private set; } = HumanStatus.Citizen;

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
}
