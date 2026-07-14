using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour, ILocalizable
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
    [SerializeField] private LocalizationItem instantlyLocalization;

    [Header("Other")]
    [SerializeField] private Image buildingImage;
    [SerializeField] private LayoutGroup resourcesToBuildLayoutGroup;

    public static event Action<BuildingWidget> OnWidgetInformationClicked;

    private CityStorage cityStorage => CityStorage.Instance;
    public Building BuildingPrefab { get; private set; }

    private void OnEnable()
    {
        buildButton.OnReleased.AddListener(OnBuildButtonCliked);
        informationButton.OnReleased.AddListener(OnInformationButtonClicked);
        cityStorage.Inventory.OnItemAmountAdded += OnCityItemAdded;

        UpdateBuildButtonEnabled();
        UpdateBuildTime();
    }

    private void OnDisable()
    {
        buildButton.OnReleased.RemoveListener(OnBuildButtonCliked);
        informationButton.OnReleased.RemoveListener(OnInformationButtonClicked);
        cityStorage.Inventory.OnItemAmountAdded -= OnCityItemAdded;
    }

    public void Init(Building building)
    {
        if (!building) {
            Debug.LogError("Building is not valid");
            return;
        }

        BuildingPrefab = building;

        CreateResourcesToBuild();
        UpdateBuildButtonEnabled();
        UpdateBuildTime();
        UpdateBuildName();
        UpdateBildingImage();
    }

    public Dictionary<string, string> GetLocalization()
    {
        var buildTime = "";
        var constructionTime = BuildingPrefab.LevelDefinition.UpgradeTime;

        if (constructionTime > 0) {
            var speedBonus = BuilderEnergyManager.Instance.CurrentEnergy;
            var speedBonusText = (speedBonus * 100).ToString("F0");
            var timeWithBonus = (int)(constructionTime * (1f - speedBonus));

            var constructionTimeText = TimeFormatter.SecondsToTimer(timeWithBonus);
            var bonusText = $"(-{speedBonusText}%)";
            buildTime = speedBonus > 0 ? $"<color=green>{constructionTimeText} {bonusText}</color>" : constructionTimeText;
        }
        else {
            buildTime = LocalizationManager.Instance.GetLocalizedText(instantlyLocalization);
        }

        return new Dictionary<string, string>()
        {
            { "buildTime", $"{buildTime}" }
        };
    }

    private void CreateResourcesToBuild()
    {
        var buildResources = BuildingPrefab.LevelDefinition.ResourcesToBuild;

        for (int i = 0; i < buildResources.Length; i++) {
            var resourceWidget = Instantiate(buildingResourceWidget, resourcesToBuildLayoutGroup.transform);

            var buildResource = buildResources[i];
            var id = buildResource.Definition.ItemId;
            var storageItem = cityStorage.Inventory.GetItem(id);

            resourceWidget.SetItem(buildResource.Definition);
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
        if (!BuildingPrefab) {
            BuildButton.SetState(CustomButtonState.Disabled);
            BuildButton.EndTransitionAnimation();
            return;
        }

        if (!cityStorage) {
            Debug.LogError($"[{nameof(BuildingWidget)}] CityStorage is not valid!");
            BuildButton.SetState(CustomButtonState.Disabled);
            BuildButton.EndTransitionAnimation();
            return;
        }

        foreach (var buildItem in BuildingPrefab.LevelDefinition.ResourcesToBuild) {
            var storageItem = cityStorage.Inventory.GetItem(buildItem.Definition.ItemId);

            if (buildItem.Amount <= storageItem.Amount) continue;

            BuildButton.SetState(CustomButtonState.Disabled);
            BuildButton.EndTransitionAnimation();
            return;
        }

        BuildButton.SetState(CustomButtonState.Idle);
        BuildButton.EndTransitionAnimation();
    }

    private void UpdateBuildTime()
    {
        if (!BuildingPrefab) return;

        buildTimeText.SetPlaceHolderLocalization(this);
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
        OnWidgetInformationClicked?.Invoke(this);
    }

    private void OnCityItemAdded(ItemInstance itemInstance)
    {
        UpdateBuildButtonEnabled();
    }
}