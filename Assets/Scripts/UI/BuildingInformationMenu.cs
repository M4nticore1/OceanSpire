using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingInformationMenu : MonoBehaviour
{
    [SerializeField] private BuildingCharacteristicWidget characteristicWidget;
    [SerializeField] private GameObject buildingInformationMenu = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuNameText = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuLevelNumberText = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuDescriptionText = null;
    [SerializeField] private RectTransform buildingCharacteristics = null;
    private float buildingCharacteristicHeight = 0;
    private List<BuildingCharacteristicWidget> spawnedBuildingCharacteristicWidgets = new List<BuildingCharacteristicWidget>();
    private SlidePanel slidePanel;

    private void Awake()
    {
        slidePanel = gameObject.GetComponent<SlidePanel>();
    }

    private void Start()
    {
        buildingCharacteristicHeight = characteristicWidget.characteristicValueBox.sizeDelta.y;
    }

    public void Open(ConstructionComponent construction)
    {
        Building building = construction.ownedBuilding;

        foreach (var widget in spawnedBuildingCharacteristicWidgets) {
            Destroy(widget.gameObject);
        }
        spawnedBuildingCharacteristicWidgets.Clear();

        slidePanel.OpenSlidePanel();

        buildingInformationMenuNameText.SetText(building.BuildingData.BuildingName);
        buildingInformationMenuLevelNumberText.SetText("Level " + (building.LevelIndex + 1).ToString());
        //buildingInformationMenuDescriptionText.SetText(building.BuildingData.description);

        ProductionBuilding productionBuilding = building.GetComponent<ProductionBuilding>();
        StorageBuildingComponent storageBuilding = building.GetComponent<StorageBuildingComponent>();

        int index = 0;

        if (productionBuilding) {
            ProductionBuildingLevelData levelData = productionBuilding.ProductionLevelsData[0];
            ItemInstance producedResource = levelData.producedResources[productionBuilding.currentProducedItemIndex].producedResource;
            CreateCharacteristicWidget("Produces", producedResource.Amount, producedResource.ItemData.ItemIcon, ref index);
            CreateCharacteristicWidget("Consumes", producedResource.Amount, producedResource.ItemData.ItemIcon, ref index);
        }

        if (storageBuilding) {
            StorageBuildingLevelData levelData = storageBuilding.StorageLevelsData[0];
            CreateCharacteristicWidget("Storage capacity", levelData.storageItems[0].Amount, levelData.storageItems[0].ItemData.ItemIcon, ref index);
        }
    }

    public void Close()
    {
        slidePanel.CloseSlidePanel();
    }

    private void CreateCharacteristicWidget(string characteristicName, int characteristicValueText, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText);
    }

    private void CreateCharacteristicWidget(string characteristicName, Sprite characteristicValueSprite, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueSprite);
    }

    private void CreateCharacteristicWidget(string characteristicName, int characteristicValueText, Sprite characteristicValueSprite, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText, characteristicValueSprite);
    }

    private BuildingCharacteristicWidget CreateCharacteristicWidget(string characteristicName, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = Instantiate(characteristicWidget, buildingCharacteristics.transform);
        productionWidget.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -buildingCharacteristicHeight * index);
        spawnedBuildingCharacteristicWidgets.Add(productionWidget);
        index++;

        return productionWidget;
    }
}
