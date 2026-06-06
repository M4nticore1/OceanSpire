using System.Collections;
using System.Collections.Generic;
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

    public override void OnSetInteractBuilding(BuildingInteractComponent interactor)
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

        int index = building.WorkComponent.TryGetIndexOf(citizen);
        var boat = BoatsManager.Instance.CitizenBoats.Values.ToArray()[index];
        boatRider.SetSelectedBoat(boat);

        if (boatRider.IsRidingOnBoat) {
            if (boatRider.IsExitingBoat) {
                boatRider.StopExitingBoat();
            }

            if (!boatRider.IsEnteringBoat && boatRider.SelectedBoat.CurrentStateEnum != BoatStateEnum.UnloadingLoot && boatRider.SelectedBoat.Inventory.RemainingWeight != 0) {
                boatRider.SelectedBoat.SetState(BoatStateEnum.FindingLoot);
            }
        }
    }

    public override void OnRemoveInteractBuilding(BuildingInteractComponent interactor)
    {
        if (!interactor) {
            Debug.Log($"Interactor not found");
            return;
        }

        var boatRider = interactor.GetComponent<BoatRider>();
        if (!boatRider) {
            Debug.Log($"Boat Rider not found at {interactor}");
            return;
        }

        if (boatRider.IsEnteringBoat) {
            boatRider.StopEnteringBoat();
        }

        if (boatRider.IsRidingOnBoat) {      
            boatRider.SelectedBoat?.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnStartedInteracting(BuildingInteractComponent interactor)
    {
        interactor.StartCoroutine(WaitForBoatAndEnter(interactor));
    }

    public override void OnStoppedInteracting(BuildingInteractComponent interactor)
    {

    }

    public override void OnInteracting(BuildingInteractComponent interactor)
    {
        
    }

    private IEnumerator WaitForBoatAndEnter(BuildingInteractComponent interactor)
    {
        if (interactor.InteractBuilding != building) yield break;

        var boatRider = interactor.GetComponent<BoatRider>();
        if (!boatRider) {
            Debug.Log($"Boat Rider not found at {boatRider.name}");
            yield break;
        }

        if (boatRider.IsRidingOnBoat) yield break;

        var boat = boatRider.SelectedBoat;
        if (!boat) {
            Debug.Log($"Selected Boat not found at {boatRider.name}");
            yield break;
        }

        while (boat.CurrentStateEnum != BoatStateEnum.Idle) {
            Debug.Log(boat.CurrentStateEnum);
            yield return new WaitForEndOfFrame();
        }

        boatRider.StartEnteringBoat();
    }
}