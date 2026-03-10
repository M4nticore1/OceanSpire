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

    public override void Process()
    {
        base.Process();
    }

    public override void HandleReachedPath()
    {
        base.HandleReachedPath();

        boat.SetState(BoatStateEnum.CollectingLoot);
    }

    private void StartMovingToLoot()
    {
        boat.Movement.MoveTo(boat.targetLootContainer.transform.position);
    }
}