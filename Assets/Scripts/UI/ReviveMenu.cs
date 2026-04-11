using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveMenu : MonoBehaviour
{
    [SerializeField] private AdsManager adsManager;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextMeshProUGUI citizenName;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI remainingTime;
    [SerializeField] private CustomButton reviveButton;

    private bool isOpened = false;
    private Human selectedHuman;

    private void OnEnable()
    {
        Human.onHumanSelected += OnHumanSelected;
        slidePanel.onClosed += OnClosed;
        reviveButton.onReleased += OnClickedRiviveButton;
        EventBus.onAdRewardRecieved += OnAdRewardRecieved;
        CreaturesManager.onCitizenUnregistered += OnCitizenUnregistered;
    }

    private void OnDisable()
    {
        Human.onHumanSelected -= OnHumanSelected;
        slidePanel.onClosed -= OnClosed;
        reviveButton.onReleased -= OnClickedRiviveButton;
        EventBus.onAdRewardRecieved -= OnAdRewardRecieved;
        CreaturesManager.onCitizenUnregistered -= OnCitizenUnregistered;
    }

    private void Update()
    {
        if (!slidePanel.isOpened && !slidePanel.isMoving) return;

        float progressAlpha = selectedHuman.currentDeadTime / selectedHuman.MaxDeadTime;
        progressBar.fillAmount = progressAlpha;

        float remainingTime = selectedHuman.MaxDeadTime - selectedHuman.currentDeadTime;
        this.remainingTime.SetText(remainingTime.ToString("F1"));
    }

    private void Open(Human human)
    {
        citizenName.SetText(human.firstName + " " + human.lastName);

        slidePanel.Open();
        InputStateManager.instance.SetGameplayInputBlocked(true);
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void OnClosed()
    {
        SelectManager.Instance.Deselect();
        InputStateManager.instance.SetGameplayInputBlocked(false);
        isOpened = false;
    }

    private void OnHumanSelected(Human human)
    {
        if (human.currentStatusEnum != HumanStatusEnum.Citizen) return;
        if (human.Health.isAlive) return;

        selectedHuman = human;
        Open(human);
    }

    private void OnClickedRiviveButton()
    {
        if (!isOpened) return;

        ReviveCitizenAdReward reward = new ReviveCitizenAdReward(selectedHuman);
        RewardedAdsManager.instance.SetCurrentReward(reward);
        adsManager.ShowAd();
    }

    private void OnAdRewardRecieved(AdRewardInstance reward)
    {
        if (reward as ReviveCitizenAdReward == null) return;

        Close();
    }

    private void OnCitizenUnregistered(Human human)
    {
        if (human != selectedHuman) return;

        Close();
    }
}