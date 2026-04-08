using UnityEngine;

public class WandererState : HumanState
{
    public WandererState(Human human) : base(human)
    {

    }

    public override void Enter()
    {
        human.SelectComponent.SetClickable(false);
        CreaturesManager.instance.RegisterWanderer(human);
    }

    public override void Exit()
    {
        human.SelectComponent.SetClickable(true);
        CreaturesManager.instance.UnregisterWanderer(human);
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

    public override void OnRevived()
    {

    }

    public override void OnDied()
    {

    }

    public override void OnDisable()
    {
        CreaturesManager.instance.UnregisterWanderer(human);
    }
}