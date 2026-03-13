using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    private CityStorage cityStorage;
    public Building buildingPrefab { get; private set; } = null;
    [SerializeField] private ResourceWidget buildingResourceWidget = null;
    private List<ResourceWidget> spawnedBuildingResourceWidgets = new List<ResourceWidget>();

    [SerializeField] private TextLocalizer buildingNameTextLocalizer = null;
    [SerializeField] private Image buildingImage = null;
    [SerializeField] private CustomButton buildButton = null;
    [SerializeField] private CustomButton informationButton = null;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup = null;

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
        cityStorage = FindAnyObjectByType<CityStorage>();

        buildingPrefab = prefab;

        Building building = prefab.GetComponentInChildren<Building>();
        if (building) {
            buildingNameTextLocalizer.SetLocalizationItem(building.BuildingData.LocalizationItem);

            if (building.BuildingData.ThumbImage)
                buildingImage.sprite = building.BuildingData.ThumbImage;
        }

        CreateResourcesToBuild();
    }

    private void CreateResourcesToBuild()
    {
        ItemInstance[] resourcesToBuild = buildingPrefab.LevelData.ResourcesToBuild;
        for (int i = 0; i < resourcesToBuild.Length; i++) {
            ItemInstance maxAmountItem = resourcesToBuild[i];
            int id = maxAmountItem.ItemData.ItemId;

            ItemInstance amountItem = cityStorage.Inventory.itemsDict[id].item;

            ResourceWidget resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);
            resourceWidget.Init(amountItem, maxAmountItem);
            spawnedBuildingResourceWidgets.Add(resourceWidget);
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

    //public void UpdateResourcesToBuild()
    //{
    //    bool enoughResources = true;

    //    for (int i = 0; i < resourcesToBuildNumber; i++) {
    //        ItemInstance resource = buildingPrefab.ConstructionLevelsData[0].ResourcesToBuild[i];
    //        int amountToBuilding = resource.Amount;
    //        int id = resource.ItemData.ItemId;
    //        int currentAmount = cityStorage.Inventory.itemsDict[id].item.Amount;
    //        spawnedBuildingResourceWidgets[i].SetAmount(currentAmount, amountToBuilding);

    //        if (enoughResources && currentAmount < amountToBuilding) {
    //            enoughResources = false;
    //        }
    //    }

    //    if (enoughResources)
    //        buildButton.SetState(CustomSelectableState.Idle);
    //    else
    //        buildButton.SetState(CustomSelectableState.Disabled);

    //    buildButton.FinishTransitionAnimation();
    //}
}
