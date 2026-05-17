using UnityEngine;

public class RaiderState : HumanState
{
    public RaiderState(Human human) : base(human)
    {

    }

    public override void Enter()
    {
        CreaturesManager.Instance.RegisterRaider(human as Raider);
    }

    public override void Exit()
    {
        CreaturesManager.Instance.UnregisterRaider(human as Raider);
    }

    public override void Tick()
    {

    }

    private void ProcessRaidBuilding()
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