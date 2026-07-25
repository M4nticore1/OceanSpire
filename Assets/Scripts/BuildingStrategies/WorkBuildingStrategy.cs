using UnityEngine;

public class WorkBuildingStrategy : BuildingStrategy
{
    public WorkBuildingStrategy(Building building) : base(building)
    {

    }

    public override void OnEntityEnter(CreatureCityNavigator navigator)
    {

    }

    public override void OnEntityExit(CreatureCityNavigator navigator)
    {

    }

    public override void OnInteractBuildingSet(CreatureInteractComponent navigator)
    {
        
    }

    public override void OnInteractBuildingRemove(CreatureInteractComponent navigator)
    {
        
    }

    public override void OnStartedInteracting(CreatureInteractComponent interactor)
    {

    }

    public override void OnStoppedInteracting(CreatureInteractComponent interactor)
    {
        
    }

    public override void OnInteracting(CreatureInteractComponent interactor)
    {

    }

    public override void OnConstructionStarted()
    {
        building.RemoveWorkers();
    }

    public override void OnConstructionFinished()
    {

    }
}
