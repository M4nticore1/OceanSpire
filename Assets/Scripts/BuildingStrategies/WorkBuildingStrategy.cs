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

    public override void OnSetInteractBuilding(BuildingInteractComponent navigator)
    {
        
    }

    public override void OnRemoveInteractBuilding(BuildingInteractComponent navigator)
    {
        
    }

    public override void OnStartedInteracting(BuildingInteractComponent interactor)
    {

    }

    public override void OnStoppedInteracting(BuildingInteractComponent interactor)
    {
        
    }

    public override void OnInteracting(BuildingInteractComponent interactor)
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
