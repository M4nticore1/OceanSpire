using UnityEngine;

public class RaiderState : HumanState
{
    public RaiderState(Human human) : base(human)
    {

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
}