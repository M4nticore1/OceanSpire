using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    public Building buildingPrefab { get; private set; } = null;
    [SerializeField] private BuildingResourceWidget buildingResourceWidget = null;
    private List<BuildingResourceWidget> spawnedBuildingResourceWidgets = new List<BuildingResourceWidget>();

    [SerializeField] private Image buildingImage = null;
    [SerializeField] private CustomButton buildButton = null;
    [SerializeField] private CustomButton informationButton = null;

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

    public void Init(Building prefab)
    {
        buildingPrefab = prefab;

        Building building = prefab.GetComponentInChildren<Building>();
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
        for (int i = 0; i < resourcesToBuildNumber; i++) {
            BuildingResourceWidget spawnedBuildingResourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);
            spawnedBuildingResourceWidgets.Add(spawnedBuildingResourceWidget);
        }
    }

    private void OnBuildButtonCliked()
    {
        EventBus.InvokeBuildingWidgetBuildClicked(this);
    }

    private void OnInformationButtonClicked()
    {
        EventBus.InvokeBuildingWidgetInformationClicked(this);
    }

    public void UpdateResourcesToBuild()
    {
        bool enoughResources = true;
        for (int i = 0; i < resourcesToBuildNumber; i++) {
            ItemInstance resource = buildingPrefab.ConstructionLevelsData[0].ResourcesToBuild[i];
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
