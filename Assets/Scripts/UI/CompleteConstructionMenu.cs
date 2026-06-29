using UnityEngine;
using UnityEngine.UI;

public class CompleteConstructionMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private TextLocalizer buildingName;
    [SerializeField] private TextLocalizer buildingLevel;
    [SerializeField] private TextLocalizer constructionTime;
    [SerializeField] private Image buildingImage;
    [SerializeField] private CustomButton completeButton;
    [SerializeField] private CustomButton closeMenuButton;
    private Building building;

    private void OnEnable()
    {
        completeButton.OnReleased.AddListener(OnCompleteButtonReleased);
        closeMenuButton.OnReleased.AddListener(OnCloseMenuButtonClicked);
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
    }

    private void OnDisable()
    {
        completeButton.OnReleased.RemoveListener(OnCompleteButtonReleased);
        closeMenuButton.OnReleased.RemoveListener(OnCloseMenuButtonClicked);
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
    }

    private void Update()
    {
        constructionTime.UpdateText();
    }

    public void Open(Building building)
    {
        if (!building) {
            Debug.LogError("building is null to open Complete Construction Menu");
            return;
        }

        gameObject.SetActive(true);

        buildingName.SetLocalizationItem(building.BuildingData.NameLocalizationItem);
        buildingName.UpdateText();

        buildingLevel.SetPlaceHolderLocalization(building);
        buildingLevel.UpdateText();

        constructionTime.SetPlaceHolderLocalization(building);
        constructionTime.UpdateText();

        buildingImage.sprite = building.UpgradeComponent.IsUnderUpgrade ? building.NextLevelData.BuildingThumb : building.LevelData.BuildingThumb;

        this.building = building;

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Show()
    {
        Open(null);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void OnCompleteButtonReleased()
    {
        var reward = new SkipConstructionRewardInstance(null, building.ConstructionComponent);
        RewardedAdsManager.Instance.SetCurrentReward(reward);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnCloseMenuButtonClicked()
    {
        Hide();
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (building != this.building) return;

        Hide();
    }
}