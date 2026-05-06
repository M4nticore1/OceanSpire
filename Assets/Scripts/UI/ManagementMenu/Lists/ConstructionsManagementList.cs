using System.Collections.Generic;
using UnityEngine;

public class ConstructionsManagementList : ManagementList
{
    [SerializeField] private BuildingWidget BuildingWidgetPrefab;
    [SerializeField] private BuildingCategory buildingCategory;

    protected override void CreateWidgets()
    {
        foreach (var building in BuildingsList.Instance.Buildings) {
            if (building.BuildingData.BuildingCategory != buildingCategory) continue;
            if (!building.BuildingData.IsDemolishable) continue;

            var widget = Instantiate(BuildingWidgetPrefab, LayoutGroup.transform);
            widget.Init(building);
        }
    }
}