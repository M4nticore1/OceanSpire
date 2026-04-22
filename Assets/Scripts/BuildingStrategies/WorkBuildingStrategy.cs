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

    public override void OnSetInteractBuilding(InteractComponent navigator)
    {
        
    }

    public override void OnRemoveInteractBuilding(InteractComponent navigator)
    {
        
    }

    public override void OnStartInteracting(InteractComponent interactor)
    {

    }

    public override void OnStopInteracting(InteractComponent interactor)
    {
        
    }

    public override void OnInteracting(InteractComponent interactor)
    {
        //if (interactBuilding.spawnedConstruction.BuildingInteractions.Length > interactorIndex) {
        //    BuildingAction buildingAction = interactBuilding.spawnedConstruction.BuildingInteractions[interactorIndex];

        //    if (buildingAction.actionTimes[currentActionIndex] > 0) {
        //        currentActionTime += Time.deltaTime;
        //        if (currentActionTime >= buildingAction.actionTimes[currentActionIndex]) {
        //            if (currentActionIndex < buildingAction.actionTimes.Length - 1)
        //                currentActionIndex++;
        //            else
        //                currentActionIndex = 0;

        //            currentActionTime = 0;

        //            Vector3 position = buildingAction.waypoints[currentActionIndex].position;
        //            movement.MoveTo(position);
        //        }
        //    }
        //}
    }
}
