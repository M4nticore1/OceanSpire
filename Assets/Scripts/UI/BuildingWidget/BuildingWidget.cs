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

    private CityStorage cityStorage => CityStorage.Instance;

    public Building BuildingPrefab { get; private set; }

    private void OnEnable()
    {
        buildButton.OnReleased.AddListener(OnBuildButtonCliked);
        informationButton.OnReleased.AddListener(OnInformationButtonClicked);

        UpdateBuildTime();
    }

    private void OnDisable()
    {
        buildButton.OnReleased.RemoveListener(OnBuildButtonCliked);
        informationButton.OnReleased.RemoveListener(OnInformationButtonClicked);
    }

    public void Init(Building building)
    {
        if (!building) {
            Debug.LogError("Building is not valid");
            return;
        }

        BuildingPrefab = building;

        CreateResourcesToBuild();
        UpdateBuildTime();
        UpdateBuildName();
        UpdateBildingImage();
    }


    public Dictionary<string, string> GetLocalization()
    {
        var buildTime = "";
        var constructionTime = BuildingPrefab.LevelData.UpgradeTime;

        if (constructionTime > 0) {
            var speedBonus = BuilderEnergyManager.Instance.CurrentEnergy;
            var timeWithBonus = (int)(constructionTime * (1f - speedBonus));

            var constructionTimeText = TimeFormatter.SecondsToTimer(timeWithBonus);
            var bonusText = $"(-{speedBonus * 100}%)";
            buildTime = speedBonus > 0 ? $"<color=green>{constructionTimeText} {bonusText}</color>" : constructionTimeText;
        }
        else {
            buildTime = LocalizationManager.Instance.GetText(instantlyLocalization);
        }

        return new Dictionary<string, string>()
        {
            { "buildTime", $"{buildTime}" }
        };
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
            resourceWidget.AddAmount(buildResource);
            //resourceWidget.AddAmount(storageItem);
            //resourceWidget.SetLimit(buildResource);

            spawnedBuildingResourceWidgets.Add(resourceWidget);
        }
    }

    private void UpdateBuildName()
    {
        buildingNameText.SetLocalizationItem(BuildingPrefab.BuildingData.NameLocalizationItem);
    }

    private void UpdateBuildTime()
    {
        if (!BuildingPrefab) return;

        buildTimeText.SetPlaceHolderLocalization(this);
    }

    private void UpdateBildingImage()
    {
        buildingImage.sprite = BuildingPrefab.LevelData.BuildingThumb;
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