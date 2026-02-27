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

    public abstract void OnEntityEnter(EntityCityNavigator navigator);
    public abstract void OnEntityExit(EntityCityNavigator navigator);
    public abstract void OnSetInteractBuilding(EntityInteractor navigator);
    public abstract void OnRemoveInteractBuilding(EntityInteractor navigator);
    public abstract void OnStartInteracting(EntityInteractor interactor);
    public abstract void OnStopInteracting(EntityInteractor interactor);
    public abstract void OnInteracting(EntityInteractor interactor);
}
