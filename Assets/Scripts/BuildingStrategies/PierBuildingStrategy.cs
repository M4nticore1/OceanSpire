using System.Collections;
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

        UpdateTargetBoats();
    }

    public override void OnInteractBuildingRemove(CreatureInteractComponent interactor)
    {
        var boatRider = interactor.GetComponent<BoatRider>();
        boatRider.RemoveTargetBoat();

        UpdateTargetBoats();
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

    private void UpdateTargetBoats()
    {
        for (int i = 0; i < building.WorkComponent.Workers.Count; i++) {
            var worker = building.WorkComponent.Workers[i];
            if (!worker) {
                Debug.LogError($"Worker not found at {building.name}");
                continue;
            }

            var boatRider = worker.GetComponent<BoatRider>();
            if (!boatRider) {
                Debug.LogError($"Boat Rider not found at {worker.name}");
                continue;
            }

            if (BoatsManager.Instance.CitizenBoats.Count <= i) {
                Debug.LogError($"Citizen Boats count is less than worker index {i}");
                continue;
            }

            var boat = BoatsManager.Instance.CitizenBoats[i];
            if (!boat) {
                Debug.LogError($"Citizen Boat not found at {BoatsManager.Instance.name}");
                continue;
            }

            if (!boatRider.TrySetTargetBoat(boat)) continue;

            boatRider.TryStopEnteringBoat();
        }
    }
}