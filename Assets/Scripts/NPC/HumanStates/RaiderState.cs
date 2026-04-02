using UnityEngine;

public class RaiderState : HumanState
{
    public RaiderState(Human human) : base(human)
    {

    }

    public override void OnSetedInteractBuilding(Building building)
    {

    }

    public override void OnRemovedInteractBuilding()
    {
        human.Interactor.RemoveInteractBuilding();
    }

    public override void OnStoppedMoving()
    {
        
    }
}