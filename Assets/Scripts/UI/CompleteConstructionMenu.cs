using System;
using UnityEngine;
using UnityEngine.UI;

public class CompleteConstructionMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private SlidePanel slidePanel;
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
        slidePanel.OnHidden += HandleHidden;
        completeButton.OnReleased.AddListener(HandleCompleteButtonReleased);
        closeMenuButton.OnReleased.AddListener(HandleCloseMenuButtonClicked);
        Building.OnBuildingConstructionFinished += HandleBuildingConstructionFinished;
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= HandleHidden;
        completeButton.OnReleased.RemoveListener(HandleCompleteButtonReleased);
        closeMenuButton.OnReleased.RemoveListener(HandleCloseMenuButtonClicked);
        Building.OnBuildingConstructionFinished -= HandleBuildingConstructionFinished;
    }

    private void Update()
    {
        constructionTime.UpdateText();
    }

    public void Show()
    {
        if (IsShowed) return;

        IsShowed = true;
        slidePanel.Show();
        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Show(Building building)
    {
        if (building == null) {
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
        slidePanel.Hide();
    }

    private void HandleHidden()
    {
        if (!IsShowed) return;

        IsShowed = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void HandleCompleteButtonReleased()
    {
        var reward = new SkipConstructionRewardInstance(null, building.ConstructionComponent);
        if (reward == null) {
            Debug.LogError($"[{nameof(CompleteConstructionMenu)}] Complete reward is not valid!");
        }

        RewardedAdsManager.Instance.SetReward(reward);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void HandleCloseMenuButtonClicked()
    {
        Hide();
    }

    private void HandleBuildingConstructionFinished(Building building)
    {
        if (building == null) return;
        if (building != this.building) return;

        Hide();
    }
}