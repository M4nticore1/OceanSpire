using UnityEngine;

public class RaiderState : HumanState
{
    private float raidTime = 10f;

    public RaiderState(Human human) : base(human)
    {

    }

    public override void Tick()
    {
        ProcessRaidBuilding();
    }

    private void ProcessRaidBuilding()
    {
        Building currentBuilding = human.CityNavigator.currentBuilding;
        if (!currentBuilding) return;

        Building interactBuilding = human.Interactor.InteractBuilding;
        if (currentBuilding != interactBuilding) return;
    }

    public override void OnSetedInteractBuilding(Building building)
    {
        if (human.BoatRider.isRidingOnBoat) {
            human.BoatRider.currentBoat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            human.CityNavigator.TryFindPathToTargetBuilding();
        }
    }

    public override void OnRemovedInteractBuilding()
    {
        human.Interactor.RemoveInteractBuilding();
    }

    public override void OnStoppedMoving()
    {
        
    }

    public override void OnEnteredBuilding(Building building)
    {
        if (building != human.Interactor.InteractBuilding) return;

        if (building.currentWorkers.Count > 0) {
            Health target = building.currentWorkers[0].GetComponent<Health>();
            Debug.Log("Attack");
        }
        else {
            Debug.Log("Not attack");
        }
    }
}