using System.Collections;
using UnityEngine;

public class PierBuildingStrategy : BuildingStrategy
{
    public PierBuildingStrategy(Building building) : base(building)
    {

    }

    public override void OnEntityEnter(EntityCityNavigator navigator)
    {

    }

    public override void OnEntityExit(EntityCityNavigator navigator)
    {

    }

    public override void OnSetInteractBuilding(EntityInteractor interactor)
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

        if (newBoatRider.currentBoat) {
            if (newBoatRider.currentBoat == boat) {
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

    public override void OnRemoveInteractBuilding(EntityInteractor interactor)
    {
        BoatRider boatRider = TryGetBoatRider(interactor?.gameObject);
        if (!boatRider) return;

        if (boatRider.isEnteringBoat) {
            boatRider.StopEnteringBoat();
        }

        if (boatRider.isRidingOnBoat) {      
            boatRider.currentBoat?.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnStartInteracting(EntityInteractor interactor)
    {
        interactor.StartCoroutine(WaitForBoatAndEnter(interactor));
    }

    public override void OnStopInteracting(EntityInteractor interactor)
    {

    }

    public override void OnInteracting(EntityInteractor interactor)
    {
        
    }

    private IEnumerator WaitForBoatAndEnter(EntityInteractor interactor)
    {
        BoatRider boatRider = TryGetBoatRider(interactor.gameObject);
        if (!boatRider) yield break;

        int index = interactor.workerIndex;
        Boat boat = BoatsManager.Instance.GetBoatByInteractorIndex(index);

        while (boat != null && boat.currentState != BoatStateEnum.Idle) {
            yield return new WaitForSeconds(0.5f);
        }

        if (interactor.InteractBuilding != building) yield break;

        if (boat != null && boatRider != null) {
            boatRider.StartEnteringBoat(boat);
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