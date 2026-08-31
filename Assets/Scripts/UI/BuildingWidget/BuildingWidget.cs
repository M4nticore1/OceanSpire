using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
    [Header("Prfabs")]
    [SerializeField] private ResourceWidget buildingResourceWidget;
    private List<ResourceWidget> spawnedBuildingResourceWidgets = new List<ResourceWidget>();

    [Header("Buttons")]
    [SerializeField] private CustomButton buildButton;
    public CustomButton BuildButton => buildButton;

    [SerializeField] private CustomButton informationButton;

    [Header("Texts")]
    [SerializeField] private TextLocalizer buildingNameText;
    [SerializeField] private TextLocalizer buildTimeText;

    [Header("Other")]
    [SerializeField] private Image buildingImage;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup;

    [Header("Localization")]
    [SerializeField] private LocalizationItem buildTimeLocalization;
    [SerializeField] private LocalizationItem instantlyBuildLocalization;

    private CityStorage cityStorage => CityStorage.Instance;
    private RaidManager raidManager => RaidManager.Instance;

    public Building BuildingPrefab { get; private set; }

    private void OnEnable()
    {
        buildButton.OnReleased.AddListener(OnBuildButtonCliked);
        informationButton.OnReleased.AddListener(OnInformationButtonClicked);
        cityStorage.Inventory.OnItemAmountAdded += HandleCityItemAdded;

        raidManager.OnRaidStarted += HandleRaidStarted;
        raidManager.OnRaidEnded += HandleRaidEnded;

        UpdateBuildButtonEnabled();
        UpdateBuildTime();
    }

    private void OnDisable()
    {
        buildButton.OnReleased.RemoveListener(OnBuildButtonCliked);
        informationButton.OnReleased.RemoveListener(OnInformationButtonClicked);
        cityStorage.Inventory.OnItemAmountAdded -= HandleCityItemAdded;

        raidManager.OnRaidStarted -= HandleRaidStarted;
        raidManager.OnRaidEnded -= HandleRaidEnded;
    }

    public void Init(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(BuildingWidget)}] Building is not valid");
            return;
        }

        BuildingPrefab = building;

        CreateResourcesToBuild();
        UpdateBuildButtonEnabled();
        UpdateBuildTime();
        UpdateBuildName();
        UpdateBildingImage();
    }

    private void CreateResourcesToBuild()
    {
        var buildResources = BuildingPrefab.LevelDefinition.ResourcesToBuild;

        for (int i = 0; i < buildResources.Length; i++) {
            var resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);

            var buildResource = buildResources[i];
            var id = buildResource.Definition.ItemId;
            var storageItem = cityStorage.Inventory.GetItem(id);

            resourceWidget.SetItemDefinition(buildResource.Definition);
            resourceWidget.AddAmount(storageItem);
            resourceWidget.SetLimit(buildResource);

            spawnedBuildingResourceWidgets.Add(resourceWidget);
        }
    }

    private void UpdateBuildName()
    {
        buildingNameText.SetLocalizationItem(BuildingPrefab.Definition.NameLocalizationItem);
    }

    private void UpdateBuildButtonEnabled()
    {
        if (ShouldEnableBuildButton()) {
            BuildButton.SetState(CustomButtonState.Idle);
        }
        else {
            BuildButton.SetState(CustomButtonState.Disabled);
        }
        BuildButton.EndTransitionAnimation();
    }

    private void UpdateBuildTime()
    {
        if (!BuildingPrefab) return;
        if (!BuildingPrefab.LevelDefinition) return;

        if (BuildingPrefab.LevelDefinition.UpgradeTime > 0) {
            buildTimeText.SetLocalizationItem(buildTimeLocalization);
            buildTimeText.SetPlaceHolderLocalization(BuildingPrefab.LevelDefinition);
        }
        else {
            buildTimeText.SetLocalizationItem(instantlyBuildLocalization);
        }
    }

    private void UpdateBildingImage()
    {
        buildingImage.sprite = BuildingPrefab.LevelDefinition.BuildingThumb;
    }

    private void OnBuildButtonCliked()
    {
        EventBus.InvokeBuildingPlacingStarted(BuildingPrefab);
    }

    private void OnInformationButtonClicked()
    {
        var informationMenu = BuildingInformationMenu.Instance;
        if (informationMenu == null) return;

        informationMenu.Show(BuildingPrefab);
    }

    private void HandleCityItemAdded(ItemInstance itemInstance)
    {
        UpdateBuildButtonEnabled();
    }

    private void HandleRaidStarted()
    {
        UpdateBuildButtonEnabled();
    }

    private void HandleRaidEnded(RaidEndedResult result)
    {
        UpdateBuildButtonEnabled();
    }

    private bool ShouldEnableBuildButton()
    {
        if (BuildingPrefab == null) return false;
        if (!BuildingPrefab.ShouldBuild()) return false;

        return true;
    }
}