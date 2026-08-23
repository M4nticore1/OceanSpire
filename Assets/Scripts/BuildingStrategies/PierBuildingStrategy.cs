using System.Linq;
using UnityEngine;

public class PierBuildingStrategy : BuildingStrategy
{
    private PierModule pier;

    public PierBuildingStrategy(Building building) : base(building)
    {
        pier = building.GetComponent<PierModule>();
    }

    public override void OnEntityEnter(CreatureCityNavigator navigator)
    {

    }

    public override void OnEntityExit(CreatureCityNavigator navigator)
    {

    }

    public override void OnInteractBuildingSet(CreatureInteractComponent interactor)
    {
        if (interactor == null) return;

        var boatRider = interactor.GetComponent<BoatRider>();
        if (boatRider == null) return;

        TryAssignFreeBoat(boatRider);
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
            index = GetFirstFreeBoatIndex(boatRider);
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

    //private void UpdateTargetBoats()
    //{
    //    for (int i = 0; i < building.CitizensHandler.Interactors.Count; i++) {
    //        var worker = building.CitizensHandler.Interactors[i];
    //        if (worker == null) {
    //            Debug.LogError($"Worker not found at {building.name}");
    //            continue;
    //        }

    //        var boatRider = worker.GetComponent<BoatRider>();
    //        if (boatRider == null) {
    //            Debug.LogError($"Boat Rider not found at {worker.name}");
    //            continue;
    //        }

    //        if (BoatsManager.Instance.CitizenBoats.Count <= i) {
    //            Debug.LogError($"Citizen Boats count is less than worker index {i}");
    //            continue;
    //        }

    //        var boat = BoatsManager.Instance.CitizenBoats[i];
    //        if (boat == null) {
    //            Debug.LogError($"Citizen Boat not found at {BoatsManager.Instance.name}");
    //            continue;
    //        }

    //        if (!boatRider.TrySetTargetBoat(boat)) continue;

    //        boatRider.TryStopEnteringBoat();
    //    }
    //}

    private void TryAssignFreeBoat(BoatRider boatRider)
    {
        if (boatRider == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Boat Rider is not valid!");
            return;
        }

        var boat = GetFirstFreeBoat(boatRider);
        if (boat == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Free Boat is not valid for {boatRider}!");
            return;
        }

        boatRider.TrySetTargetBoat(boat);
    }

    private Boat GetFirstFreeBoat(BoatRider boatRider)
    {
        var index = GetFirstFreeBoatIndex(boatRider);
        if (index == null) return null;

        return BoatsManager.Instance.CitizenBoats[index.Value];
    }

    private int? GetFirstFreeBoatIndex(BoatRider boatRider)
    {
        if (boatRider == null) return null;
        var citizenBoats = BoatsManager.Instance.CitizenBoats;

        for (int i = 0; i < citizenBoats.Count; i++) {
            var boat = citizenBoats[i];
            if (boat == null) continue;

            var targetRider = boat.TargetRider;
            var currentRider = boat.CurrentRider;

            if (targetRider == boatRider)
                return i;

            if (targetRider == null && currentRider == null)
                return i;
        }

        for (int i = 0; i < citizenBoats.Count; i++) {
            var boat = citizenBoats[i];
            if (boat == null) continue;

            var targetRider = boat.TargetRider;
            var currentRider = boat.CurrentRider;

            if (currentRider == boatRider && targetRider == null)
                return i;

            if (currentRider == null && targetRider == boatRider)
                return i;

            if (targetRider == null)
                return i;
        }

        return null;
    }
}