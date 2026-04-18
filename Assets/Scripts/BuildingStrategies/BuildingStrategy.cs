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
    public abstract void OnSetInteractBuilding(BuildingInteractHandler navigator);
    public abstract void OnRemoveInteractBuilding(BuildingInteractHandler navigator);
    public abstract void OnStartInteracting(BuildingInteractHandler interactor);
    public abstract void OnStopInteracting(BuildingInteractHandler interactor);
    public abstract void OnInteracting(BuildingInteractHandler interactor);
}
