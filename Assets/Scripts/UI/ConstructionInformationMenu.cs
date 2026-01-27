using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionInformationMenu : MonoBehaviour
{
    [SerializeField] private BuildingCharacteristicWidget characteristicWidgetPrefab;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private TextMeshProUGUI levelNumberText = null;
    [SerializeField] private TextMeshProUGUI descriptionText = null;
    [SerializeField] private LayoutGroup buildingCharacteristicsLayourGroup = null;
    private List<BuildingCharacteristicWidget> spawnedBuildingCharacteristicWidgets = new List<BuildingCharacteristicWidget>();

    private void OnEnable()
    {
        EventBus.Instance.onBuildingWidgetInformationClicked += OnBuildingWidgetInformationClicked;
    }

    private void OnDisable()
    {
        EventBus.Instance.onBuildingWidgetInformationClicked -= OnBuildingWidgetInformationClicked;
    }

    private void OnBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        ConstructionComponent constructionComponent = widget.constructionComponent;
        Open(constructionComponent);
    }

    public void Open(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();

        foreach (var widget in spawnedBuildingCharacteristicWidgets) {
            Destroy(widget.gameObject);
        }
        spawnedBuildingCharacteristicWidgets.Clear();

        slidePanel.OpenSlidePanel();

        nameText.SetText(building.BuildingData.BuildingName);
        levelNumberText.SetText("Level " + (building.LevelIndex + 1).ToString());
        //buildingInformationMenuDescriptionText.SetText(building.BuildingData.description);

        ProductionBuilding productionBuilding = building.GetComponent<ProductionBuilding>();
        StorageBuildingComponent storageBuilding = building.GetComponent<StorageBuildingComponent>();

        int maxResidentsCount = building.LevelData.maxResidentsCount;
        if (maxResidentsCount > 0)
            CreateCharacteristicWidget("Max residents", maxResidentsCount);

        if (productionBuilding) {
            ProductionBuildingLevelData levelData = productionBuilding.ProductionLevelData;
            ItemInstance producedResource = levelData.producedResources[productionBuilding.currentProducedItemIndex].producedResource;
            CreateCharacteristicWidget("Produces", producedResource.Amount, producedResource.ItemData.ItemIcon);
            CreateCharacteristicWidget("Consumes", producedResource.Amount, producedResource.ItemData.ItemIcon);
        }

        if (storageBuilding) {
            ItemInstance[] items = storageBuilding.StorageLevelsData[0].storageItems;
            foreach (ItemInstance item in items) {
                CreateCharacteristicWidget("Storage capacity", item.Amount, item.ItemData.ItemIcon);
            }
        }
    }

    public void Close()
    {
        slidePanel.CloseSlidePanel();
    }

    private void CreateCharacteristicWidget(string characteristicName, int characteristicValueText)
    {
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText);
    }

    private void CreateCharacteristicWidget(string characteristicName, Sprite characteristicValueSprite)
    {
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueSprite);
    }

    private void CreateCharacteristicWidget(string characteristicName, int characteristicValueText, Sprite characteristicValueSprite)
    {
        Debug.Log("CreateCharacteristicWidget");
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText, characteristicValueSprite);
    }

    private BuildingCharacteristicWidget CreateCharacteristicWidget(string characteristicName)
    {
        BuildingCharacteristicWidget productionWidget = Instantiate(characteristicWidgetPrefab, buildingCharacteristicsLayourGroup.transform);
        spawnedBuildingCharacteristicWidgets.Add(productionWidget);

        return productionWidget;
    }
}
