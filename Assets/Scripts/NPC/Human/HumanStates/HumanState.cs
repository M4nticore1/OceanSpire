using UnityEngine;

public abstract class HumanState
{
    protected Human human;

    public HumanState(Human human)
    {
        this.human = human;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Tick();

    public abstract void OnAttackStarted();
    public abstract void OnAttackStopped();

    public abstract void OnSetedInteractBuilding(Building building);
    public abstract void OnRemovedInteractBuilding(Building building);
    public abstract void OnInteractionStarted();
    public abstract void OnInteractionStopped();

    public abstract void OnStoppedMoving();

    public abstract void OnEnteredBuilding(Building building);

    public abstract void OnEnteredBoat(Boat boat);
    public abstract void OnExitedBoat(Boat boat);

    public abstract void OnRevived();
    public abstract void OnDied();
}