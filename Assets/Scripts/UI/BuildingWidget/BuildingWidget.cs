using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    public Building BuildingPrefab { get; private set; }
    [SerializeField] private ResourceWidget buildingResourceWidget;
    private List<ResourceWidget> spawnedBuildingResourceWidgets = new List<ResourceWidget>();

    [SerializeField] private CustomButton buildButton;
    public CustomButton BuildButton => buildButton;

    [SerializeField] private TextLocalizer buildingNameTextLocalizer;
    [SerializeField] private Image buildingImage;
    [SerializeField] private CustomButton informationButton;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup;

    private CityStorage cityStorage => CityStorage.Instance;

    private void OnEnable()
    {
        buildButton.OnReleased.AddListener(OnBuildButtonCliked);
        informationButton.OnReleased.AddListener(OnInformationButtonClicked);

        
    }

    private void OnDisable()
    {
        buildButton.OnReleased.RemoveListener(OnBuildButtonCliked);
        informationButton.OnReleased.RemoveListener(OnInformationButtonClicked);
    }

    public void Init(Building prefab)
    {
        BuildingPrefab = prefab;

        var building = prefab.GetComponentInChildren<Building>();

        if (building) {
            buildingNameTextLocalizer.SetLocalizationItem(building.BuildingData.NameLocalizationItem);
            buildingNameTextLocalizer.UpdateText();

            buildingImage.sprite = building.LevelData.BuildingThumb;
        }

        CreateResourcesToBuild();
    }

    private void CreateResourcesToBuild()
    {
        var buildResources = BuildingPrefab.LevelData.ResourcesToBuild;

        for (int i = 0; i < buildResources.Length; i++) {
            var resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);

            var buildResource = buildResources[i];
            int id = buildResource.Definition.ItemId;
            var storageItem = cityStorage.Inventory.GetItemById(id);

            resourceWidget.SetItem(buildResource.Definition);
            resourceWidget.AddAmount(storageItem);
            resourceWidget.SetLimit(buildResource);

            spawnedBuildingResourceWidgets.Add(resourceWidget);
        }
    }

    private void UpdateBuildButtonEnabled()
    {

    }

    private void OnBuildButtonCliked()
    {
        EventBus.InvokeBuildingPlacingStarted(BuildingPrefab);
    }

    private void OnInformationButtonClicked()
    {
        EventBus.InvokeBuildingWidgetInformationClicked(this);
    }
}