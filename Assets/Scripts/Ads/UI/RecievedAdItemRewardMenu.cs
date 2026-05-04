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
        AdRewardInstance.onRewardReceived += OnAdRewardRecieved;
    }

    private void OnDisable()
    {
        AdRewardInstance.onRewardReceived -= OnAdRewardRecieved;
    }

    private void Update()
    {
        if (!isOpened) return;

        currentShowTime += Time.deltaTime;
        if (currentShowTime >= showTime) {
            Close();
        }
    }

    private void OnAdRewardRecieved(AdRewardInstance reward)
    {
        if (reward as ItemAdRewardInstance == null) return;

        Open();
        AssignImage(reward);
        AssignText(reward);
    }

    private void Open()
    {
        slidePanel.Open();

        currentShowTime = 0;
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
        isOpened = false;
    }

    private void AssignImage(AdRewardInstance reward)
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.Instance.currentReward as ItemAdRewardInstance;
        rewardImage.sprite = itemReward.ItemRewardDefinition.RewardIcon;
    }

    private void AssignText(AdRewardInstance reward)
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.Instance.currentReward as ItemAdRewardInstance;
        receiveText.SetLocalizationItem(itemReward.ItemRewardDefinition.ReceievedRewardLocalization);
        receiveText.SetPlaceHolderLocalization(itemReward);
        receiveText.UpdateText();
    }
}