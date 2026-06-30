using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReviveMenu : UIBehaviour
{
    [Header("Managers")]
    [SerializeField] private ReviveManager reviveManager;
    [SerializeField] private SelectManager selectManager;
    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private SkillsPanel skillsPanel;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextLocalizer citizenNameText;

    [Header("Remaining")]
    [SerializeField] private TextLocalizer remainingReviveTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesCountText;

    [Header("Next Revive Charge")]
    [SerializeField] private TextMeshProUGUI nextReviveChargeTimeText;
    [SerializeField] private TextMeshProUGUI nextReviveChargeText;

    private Citizen citizen;
    private bool isOpened = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.OnReleased.AddListener(OnButtonClicked);
        slidePanel.OnClosed += OnClosed;
        reviveManager.OnRevivesCountChanged += OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected += OnComponentSelected;
        ReviveComponent.OnGlobalRevived += OnRevived;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.OnReleased.RemoveListener(OnButtonClicked);
        slidePanel.OnClosed -= OnClosed;
        reviveManager.OnRevivesCountChanged -= OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected -= OnComponentSelected;
        ReviveComponent.OnGlobalRevived -= OnRevived;
    }

    private void Update()
    {
        if (!citizen) return;
        if (!isOpened) return;

        UpdateMenuShowed();
        UpdateTimeToDie();
        UpdateNextChargeTimeText();
    }

    public void Open(Citizen citizen)
    {
        if (!citizen) {
            Debug.LogError("Citizen to open revive menu is not valid");
            return;
        }

        this.citizen = citizen;

        isOpened = true;
        slidePanel.Show();

        skillsPanel.SetSkills(citizen.SkillsComponent);
        UpdateCitizenNameText();

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Hide();
        UpdateButtonEnabled();
    }

    private void OnClosed()
    {
        isOpened = false;
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void UpdateMenuShowed()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;
        var remainingTimeToDie = dieTime - currentTime;

        if (remainingTimeToDie > 0) return;

        Close();
        UpdateButtonEnabled();
    }

    private void UpdateButtonEnabled()
    {
        if (!citizen) return;

        var enoughRevives = reviveManager.RemainingRevivesCount > 0;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dieTime = citizen.ReviveComponent.DieTime;
        var enoughTime = dieTime != null ? currentTime <= dieTime.Value : false;

        button.SetState(enoughRevives && enoughTime ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateCitizenNameText()
    {
        citizenNameText.SetPlaceHolderLocalization(citizen.NameComponent);
        citizenNameText.UpdateText();
    }

    private void UpdateRemainingRevivesCountText()
    {
        var maxRevivesCount = reviveManager.MaxRevivesCount;
        var remainingRevivesCount = reviveManager.RemainingRevivesCount;

        remainingRevivesCountText.SetText($"{remainingRevivesCount}/{maxRevivesCount}");
    }

    private void UpdateTimeToDie()
    {
        if (!citizen) return;

        var reviveComponent = citizen.ReviveComponent;
        if (!reviveComponent) return;

        remainingReviveTimeText.SetPlaceHolderLocalization(reviveComponent);
        remainingReviveTimeText.UpdateText();
    }

    private void UpdateNextChargeTimeText()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var chargeTime = reviveManager.NextChargeReviveTimeInSeconds;

        if (chargeTime != null && chargeTime.Value > currentTime) {
            var remainingTime = chargeTime.Value - currentTime;

            nextReviveChargeText.gameObject.SetActive(true);
            nextReviveChargeTimeText.SetText(TimeFormatter.SecondsToMinuteTime((int)remainingTime));
        }
        else {
            nextReviveChargeText.gameObject.SetActive(false);
            nextReviveChargeTimeText.SetText(string.Empty);
        }
    }

    private void OnRemainingRevivesCountChanged(int value)
    {
        UpdateButtonEnabled();
        UpdateRemainingRevivesCountText();
    }

    private void OnRevived(ReviveComponent reviveComponent)
    {
        if (!reviveComponent) return;
        if (!citizen) return;
        if (reviveComponent != citizen.ReviveComponent) return;

        Close();
    }

    private void OnButtonClicked()
    {
        if (!citizen) return;

        reviveManager.CreateReward(citizen);
        rewardedAdsManager.ShowAd();
    }

    private void OnComponentSelected(SelectComponent component)
    {
        var citizen = SelectManager.Instance.GetSelectedHuman() as Citizen;
        if (!citizen) return;

        if (citizen.HealthComponent.IsAlive) return;

        Open(citizen);
    }
}