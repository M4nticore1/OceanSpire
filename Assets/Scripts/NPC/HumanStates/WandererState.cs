using UnityEngine;

public class WandererState : HumanState
{
    public WandererState(Human human) : base(human)
    {

    }

    public override void Tick()
    {

    }

    public override void OnStoppedAttacking()
    {

    }

    public override void OnSetedInteractBuilding(Building building)
    {
        
    }

    public override void OnRemovedInteractBuilding()
    {

    }

    public override void OnStoppedMoving()
    {
        
    }

    public override void OnEnteredBuilding(Building building)
    {

    }

    public override void OnEnteredBoat(Boat boat)
    {
        boat.SetState(BoatStateEnum.MovingToDock);
    }

    public override void OnDeath()
    {

    }
}