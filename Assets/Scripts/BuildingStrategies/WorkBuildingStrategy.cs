using System.Linq;
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

    public override BuildingAction GetInteractPoint(CreatureInteractComponent interactor)
    {
        if (interactor == null) return null;

        var citizen = interactor.GetComponent<Citizen>();
        var raider = interactor.GetComponent<Raider>();

        if (citizen) {
            return GetInteractionPosition(building.CitizensHandler, citizen);
        }
        else if (raider) {
            return GetInteractionPosition(building.RaidersHandler, raider);
        }

        return null;
    }

    private BuildingAction GetInteractionPosition(BuildingInteractorsHandler interactorsHandler, Human human)
    {
        if (human == null) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Human is not valid!");
            return null;
        }

        if (interactorsHandler == null) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Interactors Handler is not valid!");
            return null;
        }

        var interactors = interactorsHandler.Interactors;
        if (!interactors.Contains(human)) return null;

        var construction = building.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Building construction is not valid at {building}!");
            return null;
        }

        var interaction = construction.InteractionPointsHandler.GetInteractPoint(human.CityNavigator);
        if (interaction == null) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Interaction is not valid at {building} with {human.CityNavigator}!");
            return null;
        }

        return interaction;
    }
}