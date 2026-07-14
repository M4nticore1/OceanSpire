using UnityEngine;

public class InformationMenuManager : MonoBehaviour
{
    [SerializeField] private InformationMenu buildingInformationMenu;

    private void OnEnable()
    {
        BuildingWidget.OnWidgetInformationClicked += OnBuildingWidgetInformationClicked;
    }

    private void OnDisable()
    {
        BuildingWidget.OnWidgetInformationClicked -= OnBuildingWidgetInformationClicked;
    }

    private void OnBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        var building = widget.BuildingPrefab;
        if (!building) return;

        buildingInformationMenu.Show(building);
    }
}