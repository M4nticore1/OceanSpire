using UnityEngine;
using UnityEngine.UI;

public class RecievedAdItemRewardMenu : MonoBehaviour
{
    [SerializeField] SlidePanel slidePanel;
    [SerializeField] Image rewardImage;
    [SerializeField] TextLocalizer receiveText;

    [SerializeField] float showTime = 1f;
    private float currentShowTime = 0f;

    private bool isOpened = false;

    private void OnEnable()
    {
        RewardInstance.OnRewardReceived += OnAdRewardRecieved;
    }

    private void OnDisable()
    {
        RewardInstance.OnRewardReceived -= OnAdRewardRecieved;
    }

    private void Update()
    {
        if (!isOpened) return;

        currentShowTime += Time.deltaTime;
        if (currentShowTime >= showTime) {
            Close();
        }
    }

    private void OnAdRewardRecieved(RewardInstance reward)
    {
        if (reward as ItemRewardInstance == null) return;

        Open();
        AssignImage(reward);
        AssignText(reward);
    }

    private void Open()
    {
        slidePanel.Show();

        currentShowTime = 0;
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Hide();
        isOpened = false;
    }

    private void AssignImage(RewardInstance reward)
    {
        ItemRewardInstance itemReward = RewardedAdsManager.Instance.CurrentReward as ItemRewardInstance;
        rewardImage.sprite = itemReward.ItemRewardDefinition.RewardIcon;
    }

    private void AssignText(RewardInstance reward)
    {
        ItemRewardInstance itemReward = RewardedAdsManager.Instance.CurrentReward as ItemRewardInstance;
        receiveText.SetLocalizationItem(itemReward.ItemRewardDefinition.ReceievedRewardLocalization);
        receiveText.SetPlaceHolderLocalization(itemReward);
    }
}