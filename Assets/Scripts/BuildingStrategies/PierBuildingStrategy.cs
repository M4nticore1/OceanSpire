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

    public override void OnInteractBuildingSet(BuildingInteractComponent interactor)
    {
        if (!BoatsManager.Instance) {
            Debug.Log("BoatManager is not on the scene.");
            return;
        }

        if (!interactor) {
            Debug.Log("Interactor not found");
            return;
        }

        var boatRider = interactor.GetComponent<BoatRider>();
        if (!boatRider) {
            Debug.Log($"BoatRider not found at {interactor.name}");
            return;
        }

        var citizen = interactor.GetComponent<Citizen>();
        if (!citizen) {
            Debug.Log($"Citizen not found at {interactor.name}");
            return;
        }

        int? index = building.WorkComponent.TryGetWorkerIndex(citizen);
        if (index == null) return;

        var boat = BoatsManager.Instance.CitizenBoats[index.Value];
        boatRider.TrySetTargetBoat(boat);

        if (boatRider.RidingBoat) {
            if (boatRider.IsExitingBoat) {
                boatRider.StopExitingBoat();
            }

            if (boatRider.RidingBoat == boat &&
                boatRider.TargetBoat.CurrentStateEnum != BoatStateEnum.UnloadingLoot &&
                boatRider.TargetBoat.Inventory.RemainingWeight != 0) {
                boatRider.TargetBoat.SetState(BoatStateEnum.FindingLoot);
            }
        }
    }

    public override void OnInteractBuildingRemove(BuildingInteractComponent interactor)
    {
        UpdateTargetBoats();
    }

    public override void OnStartedInteracting(BuildingInteractComponent interactor)
    {
        var boatRider = interactor.GetComponent<BoatRider>();
        if (!boatRider) {
            Debug.Log($"Boat Rider not found at {interactor}");
            return;
        }

        boatRider.WaitForBoatAndEnter();
    }

    public override void OnStoppedInteracting(BuildingInteractComponent interactor)
    {

    }

    public override void OnInteracting(BuildingInteractComponent interactor)
    {
        
    }

    private void UpdateTargetBoats()
    {
        for (int i = 0; i < building.WorkComponent.Workers.Count; i++) {
            var worker = building.WorkComponent.Workers[i];
            if (!worker) {
                Debug.Log($"Worker not found at {building.name}");
                continue;
            }

            var boatRider = worker.GetComponent<BoatRider>();
            if (!boatRider) {
                Debug.Log($"Boat Rider not found at {worker.name}");
                continue;
            }

            var boat = BoatsManager.Instance.CitizenBoats[i];
            if (!boat) {
                Debug.Log($"Citizen Boat not found at {BoatsManager.Instance.name}");
                continue;
            }

            if (!boatRider.TrySetTargetBoat(boat)) continue;

            boatRider.TryStopEnteringBoat();
            boatRider.TryMoveToBoat();
        }
    }
}