using UnityEngine;

public class BuildingWorkersIndicatorController : BuildingIndicatorController
{
    private BuildingCitizensHandler citizensHandler => building.CitizensHandler;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (citizensHandler != null) {
            citizensHandler.OnInteractorAdded += HandleWorkerAdded;
            citizensHandler.OnInteractorRemoved += HandleWorkerRemoved;
        }
        else {
            Debug.LogError($"[{nameof(BuildingWorkersIndicatorController)}] Citizens Handler is not valid!");
        }
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        if (citizensHandler != null) {
            citizensHandler.OnInteractorAdded -= HandleWorkerAdded;
            citizensHandler.OnInteractorRemoved -= HandleWorkerRemoved;
        }
    }

    protected override void HandleClick()
    {
        var workersMenu = WorkersControlMenu.Instance;
        if (workersMenu == null) return;

        workersMenu.Show(building);
    }

    protected override bool ShouldShow()
    {
        if (building == null) return false;
        if (building.Definition == null) return false;
        if (!building.Definition.IsWorkable) return false;
        if (constructionComponent != null && constructionComponent.IsUnderConstruction) return false;
        if (building.LevelDefinition.MaxHumansCount <= 0) return false;

        return citizensHandler.Interactors.Count <= 0;
    }

    // Events
    private void HandleWorkerAdded(Human human)
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleWorkerRemoved(Human human)
    {
        RunUpdateShownEndOfFrame();
    }
}