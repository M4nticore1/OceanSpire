using UnityEngine;

public abstract class HumanState
{
    protected Human human;

    public HumanState(Human human)
    {
        this.human = human;
    }

    public abstract void Tick();
    public abstract void OnSetedInteractBuilding(Building building);
    public abstract void OnRemovedInteractBuilding();
    public abstract void OnStoppedMoving();
    public abstract void OnEnteredBuilding(Building building);
}