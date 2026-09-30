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

    private ReviveComponent reviveComponent => citizen?.ReviveComponent;

    // Таймер для оптимизации UI (обновление раз в секунду вместо каждого кадра)
    private float _nextUiUpdateTime;

    protected override void Subscribe()
    {
        base.Subscribe();

        reviveButton.OnReleased.AddListener(HandleReviveButtonClicked);

        if (reviveManager != null) {
            reviveManager.OnRevivesCountChanged += HandleRemainingRevivesCountChanged;
        }

        if (selectManager != null) {
            selectManager.OnComponentSelected += HandleComponentSelected;
        }

        ReviveComponent.OnGlobalRevived += HandleRevived;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        reviveButton.OnReleased.RemoveListener(HandleReviveButtonClicked);

        if (reviveManager != null) {
            reviveManager.OnRevivesCountChanged -= HandleRemainingRevivesCountChanged;
        }

        if (selectManager != null) {
            selectManager.OnComponentSelected -= HandleComponentSelected;
        }

        ReviveComponent.OnGlobalRevived -= HandleRevived;
    }

    private void Update()
    {
        if (citizen == null || !IsShown) return;

        UpdateMenuShown();

        if (Time.time >= _nextUiUpdateTime) {
            _nextUiUpdateTime = Time.time + 1f;
            UpdateTimeToDie();
            UpdateNextChargeTimeText();
        }
    }

    protected override void HandleShown()
    {
        base.HandleShown();

        if (reviveComponent != null) {
            remainingReviveTimeText.SetPlaceHolderLocalization(reviveComponent);
        }

        UpdateRemainingRevivesCountText();
        UpdateButtonEnabled();
    }

    private void UpdateMenuShown()
    {
        if (citizen.ReviveComponent == null) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;

        if (dieTime == null || dieTime.Value > currentTime) return;

        Hide();
    }

    private void UpdateButtonEnabled()
    {
        if (citizen == null || reviveManager == null) return;

        var enoughRevives = reviveManager.RemainingRevivesCount > 0;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;
        var enoughTime = dieTime != null && currentTime <= dieTime.Value;

        reviveButton.SetState(enoughRevives && enoughTime ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateRemainingRevivesCountText()
    {
        if (reviveManager == null || remainingRevivesCountText == null) return;

        var maxRevivesCount = reviveManager.MaxRevivesCount;
        var remainingRevivesCount = reviveManager.RemainingRevivesCount;

        remainingRevivesCountText.SetText($"{remainingRevivesCount}/{maxRevivesCount}");
    }

    private void UpdateTimeToDie()
    {
        if (citizen == null || citizen.ReviveComponent == null) return;
        remainingReviveTimeText.UpdateText();
    }

    private void UpdateNextChargeTimeText()
    {
        if (reviveManager == null || nextReviveChargeTimeText == null) return;

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

    private void HandleRemainingRevivesCountChanged(int value)
    {
        UpdateButtonEnabled();
        UpdateRemainingRevivesCountText();
    }

    private void HandleRevived(ReviveComponent targetComponent)
    {
        if (targetComponent == null || citizen == null) return;
        if (targetComponent != citizen.ReviveComponent) return;

        Hide();
    }

    private void HandleReviveButtonClicked()
    {
        if (citizen == null || reviveManager == null) return;

        reviveManager.CreateRewardAndApply(citizen);
    }

    private void HandleComponentSelected(SelectComponent component)
    {
        var selectedCitizen = SelectManager.Instance?.GetSelectedHuman() as Citizen;
        if (selectedCitizen == null || selectedCitizen.HealthComponent.IsAlive) return;

        Show(selectedCitizen);
    }
}