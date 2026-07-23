using System;
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

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

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

    public void Show()
    {
        IsShowed = true;
        gameObject.SetActive(true);
        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Show(Building building)
    {
        if (!building) {
            Debug.LogError("building is null to open Complete Construction Menu");
            return;
        }

        this.building = building;
        buildingName.SetLocalizationItem(building.Definition.NameLocalizationItem);
        buildingLevel.SetPlaceHolderLocalization(building);
        constructionTime.SetPlaceHolderLocalization(building);

        buildingImage.sprite = building.UpgradeComponent.IsUnderUpgrade ? building.NextLevelDefinition.BuildingThumb : building.LevelDefinition.BuildingThumb;
        Show();
    }

    public void Hide()
    {
        IsShowed = false;
        gameObject.SetActive(false);
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void OnCompleteButtonReleased()
    {
        var reward = new SkipConstructionRewardInstance(null, building.ConstructionComponent);
        if (reward == null) {
            Debug.LogError($"[{nameof(CompleteConstructionMenu)}] Complete reward is not valid!");
        }

        RewardedAdsManager.Instance.SetReward(reward);
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