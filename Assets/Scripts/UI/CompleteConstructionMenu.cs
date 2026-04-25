using UnityEngine;
using UnityEngine.UI;

public class CompleteConstructionMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private TextLocalizer buildingName;
    [SerializeField] private TextLocalizer buildingLevel;
    [SerializeField] private TextLocalizer constructionTime;
    [SerializeField] private Image buildingImage;
    [SerializeField] private CustomButton completeButton;
    private Building building;

    private void OnEnable()
    {
        completeButton.onReleased += OnCompleteButtonReleased;
        Building.onBuildingConstructionFinished += OnBuildingConstructionFinished;
    }

    private void OnDisable()
    {
        completeButton.onReleased -= OnCompleteButtonReleased;
        Building.onBuildingConstructionFinished -= OnBuildingConstructionFinished;
    }

    private void Update()
    {
        constructionTime.UpdateText();
    }

    public void Open(Building building)
    {
        gameObject.SetActive(true);

        buildingName.SetLocalizationItem(building.BuildingData.LocalizationItem);
        buildingName.UpdateText();

        buildingLevel.SetPlaceHolderLocalization(building);
        buildingLevel.UpdateText();

        constructionTime.SetPlaceHolderLocalization(building);
        constructionTime.UpdateText();

        buildingImage.sprite = building.BuildingData.ThumbImage;
        this.building = building;

        InputStateManager.instance.SetGameplayInputBlocked(true);
    }

    public void Open()
    {
        Open(null);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        InputStateManager.instance.SetGameplayInputBlocked(false);
    }

    private void OnCompleteButtonReleased()
    {
        CompleteConstructionAdRewardInstance reward = new CompleteConstructionAdRewardInstance(building.ConstructionComponent);
        RewardedAdsManager.instance.SetCurrentReward(reward);
        RewardedAdsManager.instance.ShowAd();
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (building != this.building) return;

        Close();
    }
}