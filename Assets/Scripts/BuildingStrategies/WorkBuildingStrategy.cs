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

    public override BuildingAction GetInteractPoint(Human human)
    {
        var citizen = human as Citizen;
        var raider = human as Raider;

        if (citizen) {
            return GetInteractionPosition(building.CitizensHandler, human);
        }
        else if (raider) {
            return GetInteractionPosition(building.RaidersHandler, human);
        }

        return null;
    }

    private BuildingAction GetInteractionPosition(BuildingInteractorsHandler interactorsHandler, Human human)
    {
        if (!human) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Human is not valid!");
            return null;
        }

        var interactors = interactorsHandler.Interactors;
        if (!interactors.Contains(human)) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Interactors does not contains Human {human}!");
            return null;
        }

        var index = interactors.ToList().IndexOf(human);

        var construction = building.SpawnedConstruction;
        if (!construction) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Building construction is not valid at {building}!");
            return null;
        }

        var interaction = construction.GetInteractPoint(index);
        if (interaction == null) {
            Debug.LogError($"[{nameof(WorkBuildingStrategy)}] Interaction at index {index} is not valid at {building}!");
            return null;
        }

        return interaction;
    }
}
