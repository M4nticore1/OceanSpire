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
        if (spawnedWidget.Count == 0) return;

        foreach (var widget in spawnedWidget.ToArray()) {
            widget.Tick();
        }
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (!building) {
            Debug.LogError("building is not valid");
            return;
        }

        if (!constructionFinishedWidget) {
            Debug.LogError("constructionFinishedWidget is not valid");
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
        Debug.Log("UpgradeFinished");
        if (!building) {
            Debug.LogError("building is not valid");
            return;
        }

        if (!upgradeFinishedWidget) {
            Debug.LogError("upgradeFinishedWidget is not valid");
            return;
        }

        foreach (var spawnedWidget in spawnedWidget) {
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
}