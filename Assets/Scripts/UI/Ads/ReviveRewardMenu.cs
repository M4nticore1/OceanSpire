using TMPro;
using UnityEngine;

public class ReviveRewardMenu : AdRewardMenu
{
    [Header("Revive Reward Menu")]
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesText;

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
        UpdateRemainingRevivesText();
    }

    protected override void OnClose()
    {
        
    }

    private void UpdateRemainingRevivesText()
    {
        int maxRevivesCount = ReviveManager.Instance.MaxRevivesCount;
        int remainingRevivesCount = ReviveManager.Instance.RemainingRevivesCount;

        remainingRevivesText.SetText(remainingRevivesCount + "/" + remainingRevivesCount);
    }
}