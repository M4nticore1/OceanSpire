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
    public abstract void OnInteractBuildingSet(BuildingInteractComponent navigator);
    public abstract void OnInteractBuildingRemove(BuildingInteractComponent navigator);
    public abstract void OnStartedInteracting(BuildingInteractComponent interactor);
    public abstract void OnStoppedInteracting(BuildingInteractComponent interactor);
    public abstract void OnInteracting(BuildingInteractComponent interactor);
}
