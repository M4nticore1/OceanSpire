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

        buildingName.SetLocalizationItem(building.Definition.NameLocalizationItem);
        buildingLevel.SetPlaceHolderLocalization(building);
        constructionTime.SetPlaceHolderLocalization(building);

        buildingImage.sprite = building.UpgradeComponent.IsUnderUpgrade ? building.NextLevelDefinition.BuildingThumb : building.LevelDefinition.BuildingThumb;

        this.building = building;

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Open()
    {
        Open(null);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void OnCompleteButtonReleased()
    {
        var reward = new SkipConstructionRewardInstance(null, building.ConstructionComponent);
        RewardedAdsManager.Instance.SetReward(reward);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnCloseMenuButtonClicked()
    {
        Close();
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (building != this.building) return;

        Close();
    }
}