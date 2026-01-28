using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    public ConstructionComponent constructionComponent { get; private set; } = null;
    [SerializeField] private BuildingResourceWidget buildingResourceWidget = null;
    private List<BuildingResourceWidget> spawnedBuildingResourceWidgets = new List<BuildingResourceWidget>();

    [SerializeField] private Image buildingImage = null;
    [SerializeField] private CustomSelectable buildButton = null;
    [SerializeField] private CustomSelectable informationButton = null;

    [SerializeField] private TextMeshProUGUI buildingNameText = null;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup = null;

    int resourcesToBuildNumber = 0;

    private void OnEnable()
    {
        buildButton.onReleased += OnBuildButtonCliked;
        informationButton.onReleased += OnInformationButtonClicked;
    }

    private void OnDisable()
    {
        buildButton.onReleased -= OnBuildButtonCliked;
        informationButton.onReleased -= OnInformationButtonClicked;
    }

    public void InitializeBuildingWidget(ConstructionComponent construction)
    {
        constructionComponent = construction;

        Building building = construction.GetComponentInChildren<Building>();
        if (building) {
            buildingNameText.SetText(building.BuildingData.BuildingName);

            if (building.ConstructionLevelsData.Count >= 1 && building.ConstructionLevelsData[0])
                resourcesToBuildNumber = building.ConstructionLevelsData[0].ResourcesToBuild.Count();
            else
                Debug.LogWarning($"{building.BuildingData.BuildingName} has no LevelData by index 0 or has not instance");

            if (building.BuildingData.ThumbImage)
                buildingImage.sprite = building.BuildingData.ThumbImage;
        }
        DrawResourcesToBuild();
    }

    private void DrawResourcesToBuild()
    {
        Building building = constructionComponent.GetComponentInChildren<Building>();

        for (int i = 0; i < resourcesToBuildNumber; i++)
        {
            if(!constructionComponent)
                Debug.Log("building is NULL");
            if (!buildingResourceWidget)
                Debug.Log("buildingResourceWidget is NULL");
            if (building && !building.ConstructionLevelsData[0])
                Debug.Log("building.buildingLevelsData[0] is NULL");
            //if (building.buildingLevelsData[0].ResourcesToBuild[i])
                //Debug.Log("building.buildingLevelsData[0].ResourcesToBuild[i] is NULL");

            BuildingResourceWidget spawnedBuildingResourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);
            spawnedBuildingResourceWidgets.Add(spawnedBuildingResourceWidget);

            //spawnedBuildingResourceWidgets[i].Initialize(building.buildingLevelsData[0].resourcesToBuild[i].Amount, building.buildingLevelsData[0].resourcesToBuild[i].ItemData.itemIcon);
        }
    }

    private void OnBuildButtonCliked()
    {
        EventBus.Instance.InvokeBuildingWidgetBuildClicked(this);
    }

    private void OnInformationButtonClicked()
    {
        EventBus.Instance.InvokeBuildingWidgetInformationClicked(this);
    }

    public void UpdateResourcesToBuild()
    {
        bool enoughResources = true;
        Building building = constructionComponent.GetComponentInChildren<Building>();
        for (int i = 0; i < resourcesToBuildNumber; i++) {
            ItemInstance resource = building.ConstructionLevelsData[0].ResourcesToBuild[i];
            int amountToBuilding = resource.Amount;
            int id = resource.ItemData.ItemId;
            int currentAmount = CityManager.Instance.items[id].Amount;
            spawnedBuildingResourceWidgets[i].SetResourceText(currentAmount, amountToBuilding);

            if (enoughResources && currentAmount < amountToBuilding) {
                enoughResources = false;
            }
        }

        if (enoughResources)
            buildButton.SetState(CustomSelectableState.Idle);
        else
            buildButton.SetState(CustomSelectableState.Disabled);
        buildButton.SetStateTransitionAlpha(1f);
    }
}
