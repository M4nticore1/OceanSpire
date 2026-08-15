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
        if (!building) {
            Debug.LogError($"[{nameof(BuildingStrategy)}] Building is not valid!");
            return;
        }

        this.building = building;
    }

    public abstract void OnEntityEnter(CreatureCityNavigator navigator);
    public abstract void OnEntityExit(CreatureCityNavigator navigator);

    public abstract void OnInteractBuildingSet(CreatureInteractComponent interactor);
    public abstract void OnInteractBuildingRemove(CreatureInteractComponent interactor);

    public abstract void OnStartedInteracting(CreatureInteractComponent interactor);
    public abstract void OnStoppedInteracting(CreatureInteractComponent interactor);

    public abstract void OnInteracting(CreatureInteractComponent interactor);

    public abstract void OnConstructionStarted();
    public abstract void OnConstructionFinished();

    public abstract BuildingAction GetInteractPoint(CreatureInteractComponent interactor);
}
