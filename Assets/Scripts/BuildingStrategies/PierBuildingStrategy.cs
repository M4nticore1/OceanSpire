using System.Collections;
using UnityEngine;

public class PierBuildingStrategy : BuildingStrategy
{
    public PierBuildingStrategy(Building building) : base(building)
    {

    }

    public override void OnEntityEnter(CreatureCityNavigator navigator)
    {

    }

    public override void OnEntityExit(CreatureCityNavigator navigator)
    {

    }

    public override void OnSetInteractBuilding(InteractComponent interactor)
    {
        if (!BoatsManager.Instance) {
            Debug.LogError("BoatManager is not on the scene.");
            return;
        }

        if (!interactor) {
            Debug.LogError("interactor is not valid.");
            return;
        }

        BoatRider newBoatRider = TryGetBoatRider(interactor.gameObject);
        Boat boat = BoatsManager.Instance.GetBoatByInteractorIndex(interactor.workerIndex);
        if (!boat) return;

        if (newBoatRider.selectedBoat) {
            if (newBoatRider.selectedBoat == boat) {
                if (newBoatRider.isExitingBoat) {
                    newBoatRider.StopExitingBoat();
                }
                 
                if (boat.currentState != BoatStateEnum.UnloadingLoot && boat.Inventory.RemainingWeight != 0) {
                    boat.SetState(BoatStateEnum.FindingLoot);
                }
            }
            else {
                boat.SetState(BoatStateEnum.MovingToDock);
            }
        }
    }

    public override void OnRemoveInteractBuilding(InteractComponent interactor)
    {
        BoatRider boatRider = TryGetBoatRider(interactor?.gameObject);
        if (!boatRider) return;

        if (boatRider.isEnteringBoat) {
            boatRider.StopEnteringBoat();
        }

        if (boatRider.isRidingOnBoat) {      
            boatRider.selectedBoat?.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnStartInteracting(InteractComponent interactor)
    {
        interactor.StartCoroutine(WaitForBoatAndEnter(interactor));
    }

    public override void OnStopInteracting(InteractComponent interactor)
    {

    }

    public override void OnInteracting(InteractComponent interactor)
    {
        
    }

    private IEnumerator WaitForBoatAndEnter(InteractComponent interactor)
    {
        BoatRider boatRider = TryGetBoatRider(interactor.gameObject);
        if (!boatRider) yield break;

        int index = interactor.workerIndex;
        Boat boat = BoatsManager.Instance.GetBoatByInteractorIndex(index);
        boatRider.SetSelectedBoat(boat);

        while (boat && boat.currentState != BoatStateEnum.Idle) {
            yield return new WaitForSeconds(0.5f);
        }

        if (interactor.interactBuilding != building) yield break;

        if (boat && boatRider) {
            boatRider.StartEnteringBoat();
        }
    }

    private BoatRider TryGetBoatRider(GameObject root)
    {
        if (root == null) return null;

        BoatRider rider = root.GetComponent<BoatRider>();
        if (rider == null) {
            Debug.LogWarning($"BoatRider not found on {root.name}");
        }
        return rider;
    }
}