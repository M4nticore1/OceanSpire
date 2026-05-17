using UnityEngine;

public class WandererState : HumanState
{
    public WandererState(Human human) : base(human)
    {

    }

    public override void Enter()
    {
        human.SelectComponent.SetClickable(false);
        CreaturesManager.Instance.RegisterWanderer(human as Wanderer);
    }

    public override void Exit()
    {
        human.SelectComponent.SetClickable(true);
        CreaturesManager.Instance.UnregisterWanderer(human as Wanderer);
    }

    public override void Tick()
    {

    }

    public override void OnAttackStarted()
    {

    }

    public override void OnAttackStopped()
    {

    }

    public override void OnSetedInteractBuilding(Building building)
    {
        
    }

    public override void OnRemovedInteractBuilding(Building building)
    {

    }

    public override void OnInteractionStarted()
    {

    }

    public override void OnInteractionStopped()
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

    }

    public override void OnExitedBoat(Boat boat)
    {
        
    }

    public override void OnRevived()
    {

    }

    public override void OnDied()
    {

    }
}