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

        //if (!BoatsManager.Instance) {
        //    Debug.Log("BoatManager is not on the scene.");
        //    return;
        //}

        //if (!interactor) {
        //    Debug.Log("Interactor not found");
        //    return;
        //}

        //var boatRider = interactor.GetComponent<BoatRider>();
        //if (!boatRider) {
        //    Debug.Log($"BoatRider not found at {interactor.name}");
        //    return;
        //}

        //var citizen = interactor.GetComponent<Citizen>();
        //if (!citizen) {
        //    Debug.Log($"Citizen not found at {interactor.name}");
        //    return;
        //}

        //int? index = building.WorkComponent.TryGetWorkerIndex(citizen);
        //if (index == null) return;

        //var boat = BoatsManager.Instance.CitizenBoats[index.Value];
        //boatRider.TrySetTargetBoat(boat);

        //if (boatRider.RidingBoat) {
        //    if (boatRider.IsExitingBoat) {
        //        boatRider.StopExitingBoat();
        //    }

        //    if (boatRider.RidingBoat == boat &&
        //        boatRider.TargetBoat.CurrentStateEnum != BoatStateEnum.UnloadingLoot &&
        //        boatRider.TargetBoat.Inventory.RemainingWeight != 0) {
        //        boatRider.TargetBoat.SetState(BoatStateEnum.FindingLoot);
        //    }
        //}
    }

    public override void OnInteractBuildingRemove(CreatureInteractComponent interactor)
    {
        var boatRider = interactor.GetComponent<BoatRider>();
        boatRider.RemoveTargetBoat();

        //UpdateTargetBoats();
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
        if (!boatRider) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Boat Rider is not valid!");
            return null;
        }

        int? index = 0;
        var targetBoat = boatRider.TargetBoat;

        if (targetBoat != null) {
            index = BoatsManager.Instance.CitizenBoats.ToList().IndexOf(targetBoat);
        }
        else {
            index = GetFirstFreeBoatIndex();
            if (index == null) {
                //Debug.LogError($"[{nameof(PierBuildingStrategy)}] Not free boats fount at {building}!");
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

    private void UpdateTargetBoats()
    {
        for (int i = 0; i < building.CitizensHandler.Interactors.Count; i++) {
            var worker = building.CitizensHandler.Interactors[i];
            if (worker == null) {
                Debug.LogError($"Worker not found at {building.name}");
                continue;
            }

            var boatRider = worker.GetComponent<BoatRider>();
            if (boatRider == null) {
                Debug.LogError($"Boat Rider not found at {worker.name}");
                continue;
            }

            if (BoatsManager.Instance.CitizenBoats.Count <= i) {
                Debug.LogError($"Citizen Boats count is less than worker index {i}");
                continue;
            }

            var boat = BoatsManager.Instance.CitizenBoats[i];
            if (boat == null) {
                Debug.LogError($"Citizen Boat not found at {BoatsManager.Instance.name}");
                continue;
            }

            if (!boatRider.TrySetTargetBoat(boat)) continue;

            boatRider.TryStopEnteringBoat();
        }
    }

    private void TryAssignFreeBoat(BoatRider boatRider)
    {
        if (!boatRider) return;

        var ridingBoat = boatRider.RidingBoat;
        if (ridingBoat) {
            boatRider.TrySetTargetBoat(ridingBoat);
        }
        else {
            var boat = GetFirstFreeBoat();
            if (boat == null) return;

            boatRider.TrySetTargetBoat(boat);
        }
    }

    private Boat GetFirstFreeBoat()
    {
        var index = GetFirstFreeBoatIndex();
        if (index == null) return null;

        return BoatsManager.Instance.CitizenBoats[index.Value];
    }

    private int? GetFirstFreeBoatIndex()
    {
        var citizenBoats = BoatsManager.Instance.CitizenBoats;
        for (int i = 0; i < citizenBoats.Count; i++) {
            var boat = citizenBoats[i];
            if (boat == null) continue;

            var currentRider = boat.CurrentRider;
            if (currentRider != null) {
                var currentCitizen = currentRider.GetComponent<Citizen>();
                if (currentCitizen != null) {
                    if (currentRider && currentCitizen.InteractComponent.InteractBuilding?.gameObject == pier.gameObject) continue;
                }
            }

            var targetRider = boat.TargetRider;
            if (targetRider != null) {
                var targetCitizen = targetRider.GetComponent<Citizen>();
                if (targetCitizen != null) {
                    if (targetCitizen && targetCitizen.InteractComponent.InteractBuilding?.gameObject == pier.gameObject) continue;
                }
            }

            return i;
        }

        return null;
    }
}