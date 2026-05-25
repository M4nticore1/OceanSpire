using UnityEngine;

public class BoatMovingToLootState : BoatFindingLootState
{
    public BoatMovingToLootState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        base.Enter();

        StartMovingToLoot();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void OnReachedPath()
    {
        base.OnReachedPath();

        boat.SetState(BoatStateEnum.CollectingLoot);
    }

    private void StartMovingToLoot()
    {
        boat.Movement.TryMoveTo(boat.TargetLootContainer.transform.position);
    }
}