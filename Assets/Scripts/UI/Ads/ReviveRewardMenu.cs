using TMPro;
using UnityEngine;

public class ReviveRewardMenu : AdRewardMenu
{
    [Header("Revive Reward Menu")]
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesText;

    protected override void OnEnable()
    {
        base.OnEnable();

        ReviveManager.Instance.onRevivesCountChanged += OnRemainingRevivesCountChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ReviveManager.Instance.onRevivesCountChanged -= OnRemainingRevivesCountChanged;
    }

    private void Update()
    {
        if (!isOpened) return;

        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        float time = human.ReviveComponent.ReviveLimitTime - human.ReviveComponent.CurrentDiedTime;
        remainingTimeText.SetText(TimeFormatter.SecondsToMinuteTime((int)time));
    }

    protected override void OnButtonClicked()
    {

    }

    protected override void OnOpen()
    {
        AssignButtonEnabled();
        UpdateRemainingRevivesText();
    }

    protected override void OnClose()
    {
        
    }

    private void AssignButtonEnabled()
    {
        bool enoughRevives = ReviveManager.Instance.RemainingRevivesCount > 0;
        watchButton.SetState(enoughRevives ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateRemainingRevivesText()
    {
        int maxRevivesCount = ReviveManager.Instance.MaxRevivesCount;
        int remainingRevivesCount = ReviveManager.Instance.RemainingRevivesCount;

        remainingRevivesText.SetText(remainingRevivesCount + "/" + remainingRevivesCount);
    }

    private void OnRemainingRevivesCountChanged()
    {
        AssignButtonEnabled();
    }
}