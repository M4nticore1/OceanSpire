using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReviveMenu : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI remainingRevivesText;

    private bool isOpened = false;
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

    protected override void Start()
    {
        base.Start();

        TrySubscribe();
    }

    private void Update()
    {
        if (!isOpened) return;

        var human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        float time = human.ReviveComponent.ReviveLimitTime - human.ReviveComponent.CurrentDiedTime;
        remainingTimeText.SetText(TimeFormatter.SecondsToMinuteTime((int)time));
    }

    private void OnButtonClicked()
    {
        var human = SelectManager.Instance.GetSelectedHuman();
        var rewrad = new ReviveAdRewardInstance(null, human);

        RewardedAdsManager.Instance.SetCurrentReward(rewrad);
        RewardedAdsManager.Instance.ShowAd();
    }

    public void Open()
    {
        isOpened = true;
        slidePanel.Show();
        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Hide();
        AssignButtonEnabled();
        UpdateRemainingRevivesText();
    }

    private void OnClosed()
    {
        isOpened = false;
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }

    private void AssignButtonEnabled()
    {
        bool enoughRevives = ReviveManager.Instance.RemainingRevivesCount > 0;
        button.SetState(enoughRevives ? CustomButtonState.Idle : CustomButtonState.Disabled);
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
        UpdateRemainingRevivesText();
    }

    private void OnRevived(RewardInstance reward)
    {
        Close();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!ReviveManager.Instance) return;

        button.OnReleased.AddListener(OnButtonClicked);
        slidePanel.OnClosed += OnClosed;
        ReviveManager.Instance.onRevivesCountChanged += OnRemainingRevivesCountChanged;
        ReviveAdRewardInstance.onRewardReceived += OnRevived;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!ReviveManager.Instance) return;

        button.OnReleased.RemoveListener(OnButtonClicked);
        slidePanel.OnClosed -= OnClosed;
        ReviveManager.Instance.onRevivesCountChanged -= OnRemainingRevivesCountChanged;
        ReviveAdRewardInstance.onRewardReceived -= OnRevived;

        isSubscribed = false;
    }
}