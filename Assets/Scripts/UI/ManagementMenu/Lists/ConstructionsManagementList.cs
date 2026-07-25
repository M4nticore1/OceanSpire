using System.Collections.Generic;
using UnityEngine;

public class ConstructionsManagementList : ManagementList
{
    [SerializeField] private BuildingWidget BuildingWidgetPrefab;
    [SerializeField] private BuildingCategory buildingCategory;

    private Dictionary<BuildingIdEnum, BuildingWidget> spawnedWidgets = new();
    public IReadOnlyDictionary<BuildingIdEnum, BuildingWidget> SpawnedWidgets => spawnedWidgets;

    public BuildingWidget GetBuildingWidget(BuildingIdEnum buildingId)
    {
        return spawnedWidgets[buildingId];
    }

    protected override void CreateWidgets()
    {
        foreach (var building in BuildingsList.Instance.Buildings) {
            if (building.Definition.BuildingCategory != buildingCategory) continue;
            if (!building.Definition.IsDemolishable) continue;

            var widget = Instantiate(BuildingWidgetPrefab, LayoutGroup.transform);
            widget.Init(building);

            spawnedWidgets.Add(building.Definition.BuildingId, widget);
        }
    }
}