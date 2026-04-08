using UnityEngine;

public enum BuildingStrategyEnum
{
    WorkBuilding,
    Pier
}

public abstract class BuildingStrategy
{
    protected Building building = null;

    public BuildingStrategy(Building building)
    {
        this.building = building;
    }

    public abstract void OnEntityEnter(CreatureCityNavigator navigator);
    public abstract void OnEntityExit(CreatureCityNavigator navigator);
    public abstract void OnSetInteractBuilding(CreatureInteractor navigator);
    public abstract void OnRemoveInteractBuilding(CreatureInteractor navigator);
    public abstract void OnStartInteracting(CreatureInteractor interactor);
    public abstract void OnStopInteracting(CreatureInteractor interactor);
    public abstract void OnInteracting(CreatureInteractor interactor);
}
