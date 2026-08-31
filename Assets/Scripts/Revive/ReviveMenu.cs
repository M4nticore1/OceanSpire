using System;
using TMPro;
using UnityEngine;

public class ReviveMenu : MonoBehaviour, IOpenable
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
    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);
        slidePanel.OnHidden += OnClosed;
        reviveManager.OnRevivesCountChanged += OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected += OnComponentSelected;
        ReviveComponent.OnGlobalRevived += OnRevived;
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
        slidePanel.OnHidden -= OnClosed;
        reviveManager.OnRevivesCountChanged -= OnRemainingRevivesCountChanged;
        selectManager.OnComponentSelected -= OnComponentSelected;
        ReviveComponent.OnGlobalRevived -= OnRevived;
    }

    private void Update()
    {
        if (citizen == null) return;
        if (!IsShowed) return;

        UpdateMenuShowed();
        UpdateTimeToDie();
        UpdateNextChargeTimeText();
    }

    public void Show()
    {
        IsShowed = true;
        InputStateManager.Instance.AddBlockTarget(this);
        OnShowed?.Invoke();
    }

    public void Show(Citizen citizen)
    {
        if (citizen == null) {
            Debug.LogError($"[{nameof(ReviveMenu)}] Citizen is not valid!");
            return;
        }

        this.citizen = citizen;
        slidePanel.Show();

        skillsPanel.SetSkills(citizen.SkillsComponent);
        UpdateCitizenNameText();

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
        UpdateButtonEnabled();

        OnHidden?.Invoke();
    }

    private void OnClosed()
    {
        IsShowed = false;
        InputStateManager.Instance.RemoveBlockTarget(this);
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

        button.SetState(enoughRevives && enoughTime ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateCitizenNameText()
    {
        citizenNameText.SetPlaceHolderLocalization(citizen.NameComponent);
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