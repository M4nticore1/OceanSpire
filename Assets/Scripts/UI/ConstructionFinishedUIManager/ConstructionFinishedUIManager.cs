using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionFinishedUIManager : MonoBehaviour
{
    [SerializeField] private ConstructionFinishedWidget constructionFinishedWidget;
    [SerializeField] private ConstructionFinishedWidget upgradeFinishedWidget;
    [SerializeField] private VerticalLayoutGroup layoutGroup;

    private List<ConstructionFinishedWidget> spawnedWidget = new();

    private void OnEnable()
    {
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished += OnBuildingUpgradeFinished;
        ConstructionFinishedWidget.OnWidgetDestroyed += OnWidgetDestroyed;
    }

    private void OnDisable()
    {
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished -= OnBuildingUpgradeFinished;
        ConstructionFinishedWidget.OnWidgetDestroyed -= OnWidgetDestroyed;
    }

    private void Update()
    {
        for (int i = spawnedWidget.Count - 1; i >= 0; i--) {
            var widget = spawnedWidget[i];
            if (widget == null) {
                spawnedWidget.RemoveAt(i);
                continue;
            }

            widget.Tick();
        }
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(ConstructionFinishedUIManager)}] Building is not valid");
            return;
        }

        if (!ShouldCreateWidget(building)) return;

        if (!constructionFinishedWidget) {
            Debug.LogError($"[{nameof(ConstructionFinishedUIManager)}] Construction Finished Widget is not valid");
            return;
        }

        foreach (var spawnedWidget in spawnedWidget) {
            if (spawnedWidget.Localizable == building as ILocalizable)
                return;
        }
        
        var widget = ConstructionFinishedWidgetFactory.CreateWidget(constructionFinishedWidget, layoutGroup.transform, building);
        spawnedWidget.Add(widget);
    }

    private void OnBuildingUpgradeFinished(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(ConstructionFinishedUIManager)}] Building is not valid");
            return;
        }

        if (!ShouldCreateWidget(building)) return;

        if (!upgradeFinishedWidget) {
            Debug.LogError($"[{nameof(ConstructionFinishedUIManager)}] Upgrade Finished Widget is not valid");
            return;
        }

        foreach (var spawnedWidget in spawnedWidget) {
            if (spawnedWidget == null) continue;

            if (spawnedWidget.Localizable == building as ILocalizable)
                return;
        }

        var widget = ConstructionFinishedWidgetFactory.CreateWidget(upgradeFinishedWidget, layoutGroup.transform, building);
        spawnedWidget.Add(widget);
    }

    private void OnWidgetDestroyed(ConstructionFinishedWidget widget)
    {
        spawnedWidget.Remove(widget);
    }

    private bool ShouldCreateWidget(Building building)
    {
        var constructionComponent = building.ConstructionComponent;
        var startTime = constructionComponent.ConstructionStartTime;
        var finishTime = constructionComponent.ConstructionStartTime;

        if (startTime != null && finishTime != null && startTime <= finishTime) return false;

        return true;
    }
}