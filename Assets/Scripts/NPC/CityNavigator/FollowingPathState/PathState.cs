using UnityEngine;

public abstract class PathState
{
    protected CreatureCityNavigator cityNavigator { get; private set; }

    public PathState(CreatureCityNavigator cityNavigator)
    {
        this.cityNavigator = cityNavigator;
    }

    public abstract void Enter();
    public abstract void Exit();
}