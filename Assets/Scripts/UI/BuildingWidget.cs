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

        buildingPrefab = prefab;

        Building building = prefab.GetComponentInChildren<Building>();

        if (building) {
            buildingNameTextLocalizer.SetLocalizationItem(building.BuildingData.LocalizationItem);
            buildingNameTextLocalizer.UpdateText();

            if (building.BuildingData.ThumbImage)
                buildingImage.sprite = building.BuildingData.ThumbImage;
        }

        CreateResourcesToBuild();
        isInited = true;
    }

    private void CreateResourcesToBuild()
    {
        ItemInstance[] buildResources = buildingPrefab.LevelData.ResourcesToBuild;

        for (int i = 0; i < buildResources.Length; i++) {
            ResourceWidget resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);

            ItemInstance buildResource = buildResources[i];
            int id = buildResource.Definition.ItemId;
            ItemInstance storageItem = cityStorage.Inventory.GetItemById(id);

            resourceWidget.SetItemAndApply(buildResource.Definition);
            resourceWidget.SetLimit(storageItem);
            resourceWidget.SetLimit(buildResource);

            spawnedBuildingResourceWidgets.Add(resourceWidget);
        }
    }

    private void OnBuildButtonCliked()
    {
        EventBus.InvokeBuildingPlacingStarted(buildingPrefab);
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