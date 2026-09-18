using System;
using TMPro;
using UnityEngine;

public class ReviveMenu : CitizenMenu, IOpenable
{
    [Header("ReviveMenu")]
    [SerializeField] private ReviveManager reviveManager;
    [SerializeField] private SelectManager selectManager;
    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    [Header("UI")]
    [SerializeField] private CustomButton reviveButton;

    [Header("Remaining")]
    [SerializeField] private TextLocalizer remainingReviveTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesCountText;

    [Header("Next Revive Charge")]
    [SerializeField] private TextMeshProUGUI nextReviveChargeTimeText;
    [SerializeField] private TextMeshProUGUI nextReviveChargeText;

    private void OnEnable()
    {
        reviveButton.OnReleased.AddListener(OnButtonClicked);
        reviveManager.OnRevivesCountChanged += OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected += OnComponentSelected;
        ReviveComponent.OnGlobalRevived += OnRevived;
    }

    private void OnDisable()
    {
        reviveButton.OnReleased.RemoveListener(OnButtonClicked);
        reviveManager.OnRevivesCountChanged -= OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected -= OnComponentSelected;
        ReviveComponent.OnGlobalRevived -= OnRevived;
    }

    private void Update()
    {
        if (citizen == null) return;
        if (!IsShown) return;

        UpdateMenuShowed();
        UpdateTimeToDie();
        UpdateNextChargeTimeText();
    }

    private void UpdateMenuShowed()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;
        var remainingTimeToDie = dieTime - currentTime;

        if (remainingTimeToDie > 0) return;

        Hide();
        UpdateButtonEnabled();
    }

    private void UpdateButtonEnabled()
    {
        if (citizen == null) return;

        var enoughRevives = reviveManager.RemainingRevivesCount > 0;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;
        var enoughTime = dieTime != null ? currentTime <= dieTime.Value : false;

        reviveButton.SetState(enoughRevives && enoughTime ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateRemainingRevivesCountText()
    {
        var maxRevivesCount = reviveManager.MaxRevivesCount;
        var remainingRevivesCount = reviveManager.RemainingRevivesCount;

        remainingRevivesCountText.SetText($"{remainingRevivesCount}/{maxRevivesCount}");
    }

    private void UpdateTimeToDie()
    {
        if (citizen == null) return;

        var reviveComponent = citizen.ReviveComponent;
        if (reviveComponent == null) return;

        remainingReviveTimeText.SetPlaceHolderLocalization(reviveComponent);
        remainingReviveTimeText.UpdateText();
    }

    private void UpdateNextChargeTimeText()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var chargeTime = reviveManager.NextChargeReviveTimeInSeconds;

        if (chargeTime != null && chargeTime.Value > currentTime) {
            var remainingTime = chargeTime.Value - currentTime;

            nextReviveChargeTimeText.SetText(TimeFormatter.SecondsToMinuteTimer((int)remainingTime));
        }
        else {
            nextReviveChargeTimeText.SetText("-");
        }
    }

    private void OnRemainingRevivesCountChanged(int value)
    {
        UpdateButtonEnabled();
        UpdateRemainingRevivesCountText();
    }

    private void OnRevived(ReviveComponent reviveComponent)
    {
        if (reviveComponent == null) return;
        if (citizen == null) return;
        if (reviveComponent != citizen.ReviveComponent) return;

        Hide();
    }

    private void OnButtonClicked()
    {
        if (!citizen) return;

        reviveManager.CreateRewardAndApply(citizen);
    }

    private void OnComponentSelected(SelectComponent component)
    {
        var citizen = SelectManager.Instance.GetSelectedHuman() as Citizen;
        if (citizen == null) return;
        if (citizen.HealthComponent.IsAlive) return;

        Show(citizen);
    }
}