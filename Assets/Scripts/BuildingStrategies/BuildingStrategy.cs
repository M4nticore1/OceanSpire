using UnityEngine;

public enum BuildingStrategyEnum
{
    WorkBuilding,
    Pier
}

public abstract class BuildingStrategy
{
    public abstract void OnEnter(EntityCityNavigator navigator);
    public abstract void OnExit(EntityCityNavigator navigator);
    public abstract void OnSetInteractBuilding(EntityInteractor navigator);
    public abstract void OnRemoveInteractBuilding(EntityInteractor navigator);
    public abstract void OnStartInteracting(EntityInteractor interactor);
    public abstract void OnStopInteracting(EntityInteractor interactor);
    public abstract void OnInteracting(EntityInteractor interactor);
}
