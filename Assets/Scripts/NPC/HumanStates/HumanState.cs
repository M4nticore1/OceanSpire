using UnityEngine;

public abstract class HumanState
{
    protected Human human;

    public HumanState(Human human)
    {
        this.human = human;
    }

    public abstract void OnSetedInteractBuilding(Building building);
    public abstract void OnRemovedInteractBuilding();
    public abstract void OnStoppedMoving();
}