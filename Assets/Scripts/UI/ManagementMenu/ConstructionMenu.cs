using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionMenu : ManagementMenu
{
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    private List<BuildingWidget> spawnedWidgets = new List<BuildingWidget>();

    protected override void CreateWidgets()
    {
        int categoriesCount = Enum.GetValues(typeof(BuildingCategory)).Length;

        foreach (var building in BuildingsList.Instance.Buildings) {
            if (!building) {
                Debug.LogError("building is NULL");
                continue;
            }
            if (!building.BuildingData) {
                Debug.LogError($"Building {building} does not have a Building Data");
                continue;
            }

            if (!building.BuildingData.IsDemolishable) continue;

            BuildingCategory buildingCategory = building.BuildingData.BuildingCategory;
            BuildingWidget spawnedBuildingWidget = null;
            spawnedBuildingWidget = Instantiate(buildingWidgetPrefab, transform);
            spawnedBuildingWidget.Init(building);
            spawnedBuildingWidget.transform.SetParent(lists[(int)buildingCategory].transform);
            spawnedWidgets.Add(spawnedBuildingWidget);
        }

        for (int i = 0; i < categoriesCount; i++) {
            RectTransform rectTransform = lists[i].GetComponent<RectTransform>();
            Vector2 initialSizeDelta = rectTransform.rect.size;
            Vector2 size = lists[i].transform.childCount * (lists[i].cellSize + lists[i].spacing) - lists[i].spacing;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            if (rectTransform.sizeDelta.y < initialSizeDelta.y) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, initialSizeDelta.y);
            }
        }
    }
}
