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
    public abstract void OnSetInteractBuilding(InteractComponent navigator);
    public abstract void OnRemoveInteractBuilding(InteractComponent navigator);
    public abstract void OnStartInteracting(InteractComponent interactor);
    public abstract void OnStopInteracting(InteractComponent interactor);
    public abstract void OnInteracting(InteractComponent interactor);
}
