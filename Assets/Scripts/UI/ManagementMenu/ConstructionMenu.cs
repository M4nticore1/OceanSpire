using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionMenu : ManagementMenu
{
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    private List<BuildingWidget> spawnedWidgets = new List<BuildingWidget>();

    protected override void Start()
    {
        base.Start();

        int listsCount = lists.Length;

        for (int i = 0; i < listsCount; i++) {
            RectTransform rect = lists[i].GetComponent<RectTransform>();

            Vector2 size = new Vector2(lists[i].cellSize.x, rect.transform.childCount * (lists[i].cellSize.y + lists[i].spacing.y) - lists[i].spacing.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
    }

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
    }
}
