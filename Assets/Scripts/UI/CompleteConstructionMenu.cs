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
        completeButton.OnReleased.AddListener(OnCompleteButtonReleased);
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
    }

    private void OnDisable()
    {
        completeButton.OnReleased.RemoveListener(OnCompleteButtonReleased);
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
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

        buildingImage.sprite = building.BuildingData.ThumbImage;
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
        SkipConstructionRewardInstance reward = new SkipConstructionRewardInstance(building.ConstructionComponent);
        RewardedAdsManager.Instance.SetCurrentReward(reward);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (building != this.building) return;

        Close();
    }
}