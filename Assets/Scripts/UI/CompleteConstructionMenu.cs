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
        Building.OnBuildingConstructionCompleted += OnBuildingConstructionFinished;
    }

    private void OnDisable()
    {
        completeButton.OnReleased.RemoveListener(OnCompleteButtonReleased);
        closeMenuButton.OnReleased.RemoveListener(OnCloseMenuButtonClicked);
        Building.OnBuildingConstructionCompleted -= OnBuildingConstructionFinished;
    }

    private void Update()
    {
        constructionTime.UpdateText();
    }

    public void Open(Building building)
    {
        gameObject.SetActive(true);

        buildingName.SetLocalizationItem(building.BuildingData.NameLocalizationItem);
        buildingName.UpdateText();

        buildingLevel.SetPlaceHolderLocalization(building);
        buildingLevel.UpdateText();

        constructionTime.SetPlaceHolderLocalization(building);
        constructionTime.UpdateText();

        buildingImage.sprite = building.NextLevelData.BuildingThumb;
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