using System.Linq;
using UnityEngine;

public class PierBuildingStrategy : BuildingStrategy
{
    private PierModule pierModule;

    public PierBuildingStrategy(Building building) : base(building)
    {
        pierModule = building.GetComponent<PierModule>();
    }

    public override void OnEntityEnter(CreatureCityNavigator navigator)
    {

    }

    public override void OnEntityExit(CreatureCityNavigator navigator)
    {

    }

    public override void OnInteractBuildingSet(CreatureInteractComponent interactor)
    {
        pierModule.UpdatePierWorkersBoats();
    }

    public override void OnInteractBuildingRemove(CreatureInteractComponent interactor)
    {
        if (interactor == null) return;

        var boatRider = interactor.GetComponent<BoatRider>();
        if (boatRider == null) return;

        boatRider.RemoveTargetBoat();
    }

    public override void OnStartedInteracting(CreatureInteractComponent interactor)
    {

    }

    public override void OnStoppedInteracting(CreatureInteractComponent interactor)
    {

    }

    public override void OnInteracting(CreatureInteractComponent interactor)
    {
        
    }

    public override void OnConstructionStarted()
    {

    }

    public override void OnConstructionFinished()
    {

    }

    public override BuildingAction GetInteractPoint(CreatureInteractComponent interactor)
    {
        var boatRider = interactor.GetComponent<BoatRider>();
        if (boatRider == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Boat Rider is not valid!");
            return null;
        }

        int? index = 0;
        var targetBoat = boatRider.TargetBoat;
        if (targetBoat != null && targetBoat.CurrentStatus != BoatStatusEnum.Citizen) return null;

        if (targetBoat != null) {
            index = BoatsManager.Instance.CitizenBoats.ToList().IndexOf(targetBoat);
        }
        else {
            index = pierModule.GetFirstFreeBoatIndex(boatRider);
            if (index == null) {
                Debug.LogError($"[{nameof(PierBuildingStrategy)}] Not free boats fount at {building} for {boatRider}!");
                return null;
            }
        }

        var construction = building.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Construction is not valid at {building}!");
            return null;
        }

        var interaction = construction.InteractionPointsHandler.GetInteractPoint(index.Value);
        if (interaction == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Interaction is not valid at index {index.Value}!");
            return null;
        }

        return interaction;
    }
}