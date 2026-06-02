using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    private CityStorage cityStorage;
    public Building BuildingPrefab { get; private set; }
    [SerializeField] private ResourceWidget buildingResourceWidget;
    private List<ResourceWidget> spawnedBuildingResourceWidgets = new List<ResourceWidget>();

    [SerializeField] private CustomButton buildButton;
    public CustomButton BuildButton => buildButton;

    [SerializeField] private TextLocalizer buildingNameTextLocalizer;
    [SerializeField] private Image buildingImage;
    [SerializeField] private CustomButton informationButton;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup;

    private bool isInited = false;

    private void OnEnable()
    {
        buildButton.OnReleased.AddListener(OnBuildButtonCliked);
        informationButton.OnReleased.AddListener(OnInformationButtonClicked);
        //EventBus.onMainStorageItemAmountChanged += OnMainStorageItemAmountChanged;

        //UpdateResourcesToBuild();
    }

    private void OnDisable()
    {
        buildButton.OnReleased.RemoveListener(OnBuildButtonCliked);
        informationButton.OnReleased.RemoveListener(OnInformationButtonClicked);
        //EventBus.onMainStorageItemAmountChanged -= OnMainStorageItemAmountChanged;
    }

    public void Init(Building prefab)
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        BuildingPrefab = prefab;

        Building building = prefab.GetComponentInChildren<Building>();

        if (building) {
            buildingNameTextLocalizer.SetLocalizationItem(building.BuildingData.NameLocalizationItem);
            buildingNameTextLocalizer.UpdateText();

            if (building.BuildingData.ThumbImage)
                buildingImage.sprite = building.BuildingData.ThumbImage;
        }

        CreateResourcesToBuild();
        isInited = true;
    }

    private void CreateResourcesToBuild()
    {
        ItemInstance[] buildResources = BuildingPrefab.LevelData.ResourcesToBuild;

        for (int i = 0; i < buildResources.Length; i++) {
            ResourceWidget resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);

            ItemInstance buildResource = buildResources[i];
            int id = buildResource.Definition.ItemId;
            ItemInstance storageItem = cityStorage.Inventory.GetItemById(id);

            resourceWidget.SetItem(buildResource.Definition);
            resourceWidget.SetLimit(storageItem);
            resourceWidget.SetLimit(buildResource);

            spawnedBuildingResourceWidgets.Add(resourceWidget);
        }
    }

    private void OnBuildButtonCliked()
    {
        EventBus.InvokeBuildingPlacingStarted(BuildingPrefab);
    }

    private void OnInformationButtonClicked()
    {
        EventBus.InvokeBuildingWidgetInformationClicked(this);
    }

    //private void UpdateResourcesToBuild()
    //{
    //    if (!isInited) return;

    //    bool enoughResources = true;

    //    foreach (var resource in buildingPrefab.GetResourcesToBuild()) {
    //        int amountToBuilding = resource.Amount;
    //        int resourceId = resource.Definition.ItemId;
    //        int currentAmount = cityStorage.Inventory.GetItemById(resourceId).Amount;

    //        if (enoughResources && currentAmount < amountToBuilding) {
    //            enoughResources = false;
    //            break;
    //        }
    //    }

    //    if (enoughResources)
    //        buildButton.SetState(CustomButtonState.Idle);
    //    else
    //        buildButton.SetState(CustomButtonState.Disabled);

    //    buildButton.EndTransitionAnimation();
    //}

    //private void OnMainStorageItemAmountChanged(ItemInstance item)
    //{
    //    UpdateResourcesToBuild();
    //}
}