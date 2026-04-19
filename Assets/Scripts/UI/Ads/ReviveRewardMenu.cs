using TMPro;
using UnityEngine;

public class ReviveRewardMenu : AdRewardMenu
{
    [Header("Revive Reward Menu")]
    [SerializeField] private TextMeshProUGUI remainingRevivesText;

    protected override void OnEnable()
    {
        base.OnEnable();

        ReviveManager.onRewardCreated += OnRewardCreated;
        ReviveManager.onRevivesCountChanged += OnRemaingRevivesCountChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ReviveManager.onRewardCreated -= OnRewardCreated;
        ReviveManager.onRevivesCountChanged -= OnRemaingRevivesCountChanged;
    }

    protected override void OnOpened()
    {
        base.OnOpened();

        UpdateRemainingRevivesText();
    }

    private void UpdateRemainingRevivesText()
    {
        int maxRevivesCount = ReviveManager.Instance.MaxRevivesCount;
        int remainingRevivesCount = ReviveManager.Instance.RemainingRevivesCount;

        remainingRevivesText.SetText(remainingRevivesCount + "/" + remainingRevivesCount);
    }

    private void OnRewardCreated()
    {
        Open();
    }

    private void OnRemaingRevivesCountChanged()
    {
        UpdateRemainingRevivesText();
    }
}