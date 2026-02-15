using UnityEngine;

public class WorkBuildingStrategy : BuildingStrategy
{
    public override void OnEnter(EntityCityNavigator navigator)
    {

    }

    public override void OnExit(EntityCityNavigator navigator)
    {

    }

    public override void OnSetInteractBuilding(EntityInteractor navigator)
    {
        
    }

    public override void OnRemoveInteractBuilding(EntityInteractor navigator)
    {
        
    }

    public override void OnStartInteracting(EntityInteractor interactor)
    {

    }

    public override void OnStopInteracting(EntityInteractor interactor)
    {
        
    }

    public override void OnInteracting(EntityInteractor interactor)
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
