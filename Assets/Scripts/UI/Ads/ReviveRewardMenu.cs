using TMPro;
using UnityEngine;

public class ReviveRewardMenu : AdRewardMenu
{
    [Header("Revive Reward Menu")]
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesText;

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
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

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!ReviveManager.Instance) return;

        ReviveManager.Instance.onRevivesCountChanged += OnRemainingRevivesCountChanged;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!ReviveManager.Instance) return;

        ReviveManager.Instance.onRevivesCountChanged -= OnRemainingRevivesCountChanged;

        isSubscribed = false;
    }
}